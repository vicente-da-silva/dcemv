/*
*************************************************************************
DC EMV
Open Source EMV
Copyright (C) 2018  Vicente Da Silva

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see http://www.gnu.org/licenses/
*************************************************************************
*/
using DCEMV.FormattingUtils;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_14_WaitingForCCCResponse2
    {
        public static SignalsEnum Execute(
            Kernel2Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
            TornTransactionLogManager tornTransactionLogManager,
            PublicKeyCertificateManager publicKeyCertificateManager,
            Stopwatch sw)
        {
            if (qManager.GetOutputQCount() > 0) //there is a pending request to the terminal
            {
                KernelRequest kernel1Request = qManager.DequeueFromInput(false);
                switch (kernel1Request.KernelTerminalReaderServiceRequestEnum)
                {
                    case KernelTerminalReaderServiceRequestEnum.STOP:
                        return EntryPointSTOP(database, qManager);

                    case KernelTerminalReaderServiceRequestEnum.DET:
                        return EntryPointDET(database, kernel1Request);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_14_WaitingForCCCResponse2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, tornTransactionLogManager, sw, publicKeyCertificateManager);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager, tornTransactionLogManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_14_WaitingForCCCResponse2:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        private static SignalsEnum DoInvalidResponse(Kernel2Database database, KernelQ qManager)
        {
            #region 14.40
            int waitTime = ((2 ^ database.FailedMSCntr) * 300);
            Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
            #endregion
            #region 14.41
            database.FailedMSCntr = Math.Min(database.FailedMSCntr + 1, 5);
            #endregion

            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.N_A,
                database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                null,
                L2Enum.NOT_SET,
                L3Enum.NOT_SET);
        }
        /*
        * 14.6
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager)
        {
            #region 14.9
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 14.10
                CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.STATUS_BYTES, L3Enum.NOT_SET);
                return DoInvalidResponse(database, qManager);
                #endregion
            }

            #region 14.11
            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            {
                EMVComputeCryptographicChecksumResponse response = cardResponse.ApduResponse as EMVComputeCryptographicChecksumResponse;
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
                parsingResult = false;
            #endregion

            #region 14.12
            if (!parsingResult)
            #endregion
            {
                #region 14.13
                CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                return DoInvalidResponse(database, qManager);
                #endregion

            }

            #region 14.12.1
            CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
            #endregion

            #region 14.14
            if (database.IsEmpty(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag) ||
                database.IsEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag))
            #endregion
            {
                #region 14.17
                CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                return DoInvalidResponse(database, qManager);
                #endregion

            }

            long nUN = 0;
            #region 14.15
            if (database.IsNotEmpty(EMVTagsEnum.CVC3_TRACK2_9F61_KRN2.Tag))
            #endregion
            {
                #region 14.16
                if (database.IsNotEmpty(EMVTagsEnum.TRACK_1_DATA_56_KRN2.Tag) &&
                    (database.IsNotPresent(EMVTagsEnum.CVC3_TRACK1_9F60_KRN2.Tag) ||
                    database.IsEmpty(EMVTagsEnum.CVC3_TRACK1_9F60_KRN2.Tag)))
                #endregion
                {
                    #region 14.17
                    CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                    return DoInvalidResponse(database, qManager);
                    #endregion
                }

                #region 14.20
                byte pcii = database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value[1];
                #endregion
                if ((pcii & 0x10) == 0x10)   //OD-CVM verification successful
                {
                    #region 14.24
                    nUN = (nUN + 5) % 10;
                    #endregion
                }
                else
                {
                    #region 14.21
                    long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
                    long rctl = database.ReaderContactlessTransactionLismit;
                    if (aa > rctl)
                    #endregion
                    {
                        #region 14.21.1
                        CommonRoutines.UpdateErrorIndication(database, cardResponse, L1Enum.NOT_SET, L2Enum.CARD_DATA_ERROR, L3Enum.NOT_SET);
                        return DoInvalidResponse(database, qManager);
                        #endregion
                    }
                    else
                    {
                        #region 14.25
                        nUN = (int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value);
                        #endregion
                    }
                }

                #region 14.25.1
                database.FailedMSCntr = 0;
                #endregion

                #region 14.26
                TRACK_2_DATA_9F6B_KRN2 t2d = new TRACK_2_DATA_9F6B_KRN2(database);
                ushort t2 = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.NATC_TRACK2_9F67_KRN2).Value);

                TLV PCVC3_TRACK2_9F65_KRN2 = database.Get(EMVTagsEnum.PCVC3_TRACK2_9F65_KRN2);
                MagBitmap bitmapPCVC3_TRACK2_9F65_KRN2 = new MagBitmap(PCVC3_TRACK2_9F65_KRN2.Value);

                TLV CVC3_TRACK2_9F61_KRN2 = database.Get(EMVTagsEnum.CVC3_TRACK2_9F61_KRN2);
                ushort cvc3T2AsShort = Formatting.ConvertToInt16(CVC3_TRACK2_9F61_KRN2.Value.Reverse().ToArray());

                string q2LeastSigDigits = Convert.ToString(cvc3T2AsShort);
                q2LeastSigDigits = q2LeastSigDigits.Substring(q2LeastSigDigits.Length - bitmapPCVC3_TRACK2_9F65_KRN2.NonZeroCount);
                t2d.Value.DiscretionaryData = Formatting.StringToBcd(bitmapPCVC3_TRACK2_9F65_KRN2.ReplaceValues(Formatting.BcdToString(t2d.Value.DiscretionaryData), q2LeastSigDigits, bitmapPCVC3_TRACK2_9F65_KRN2.NonZeroCount, true), false);

                TLV PUNATC_TRACK2_9F66_KRN2 = database.Get(EMVTagsEnum.PUNATC_TRACK2_9F66_KRN2);
                MagBitmap bitmapPUNATC_TRACK2_9F66_KRN2 = new MagBitmap(PUNATC_TRACK2_9F66_KRN2.Value);
                uint unpredInt = Formatting.ConvertToInt32(database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_NUMERIC_9F6A_KRN2).Value.Reverse().ToArray());
                string unpredString = Convert.ToString(unpredInt);
                unpredString = unpredString.Substring(unpredString.Length - database.NUN);
                t2d.Value.DiscretionaryData = Formatting.StringToBcd(bitmapPUNATC_TRACK2_9F66_KRN2.ReplaceValues(Formatting.BcdToString(t2d.Value.DiscretionaryData), unpredString, database.NUN, true), false);

                if (t2 != 0)
                {
                    TLV APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN = database.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN);
                    uint atcAsShort = Formatting.ConvertToInt32(APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Value.Reverse().ToArray());
                    string atcAsShortLeastSigDigits = Convert.ToString(atcAsShort);
                    atcAsShortLeastSigDigits = atcAsShortLeastSigDigits.PadLeft(t2, '0').Substring(atcAsShortLeastSigDigits.Length - t2);
                    t2d.Value.DiscretionaryData = Formatting.StringToBcd(bitmapPUNATC_TRACK2_9F66_KRN2.ReplaceValues(Formatting.BcdToString(t2d.Value.DiscretionaryData), atcAsShortLeastSigDigits, t2, false), false);
                }
                #endregion

                #region 14.27
                StringBuilder dd = new StringBuilder(Formatting.BcdToString(t2d.Value.DiscretionaryData));
                dd[dd.Length - 1] = Convert.ToString(nUN)[0];
                t2d.Value.DiscretionaryData = Formatting.StringToBcd(dd.ToString(), false);
                #endregion

                t2d.Serialize(); //reserialize in case the length of discretionary data changed
                t2d.UpdateDB();

                #region 14.28
                if (database.IsNotEmpty(EMVTagsEnum.TRACK_1_DATA_56_KRN2.Tag))
                #endregion
                {
                    #region 14.29
                    TRACK_1_DATA_56_KRN2 t1d = new TRACK_1_DATA_56_KRN2(database);
                    ushort t1 = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.NATC_TRACK1_9F64_KRN2).Value);

                    TLV PCVC3_TRACK1_9F62_KRN2 = database.Get(EMVTagsEnum.PCVC3_TRACK1_9F62_KRN2);
                    MagBitmap bitmapPCVC3_TRACK1_9F62_KRN2 = new MagBitmap(PCVC3_TRACK1_9F62_KRN2.Value);

                    TLV CVC3_TRACK1_9F60_KRN2 = database.Get(EMVTagsEnum.CVC3_TRACK1_9F60_KRN2);
                    ushort cvc3T1AsShort = Formatting.ConvertToInt16(CVC3_TRACK1_9F60_KRN2.Value.Reverse().ToArray());

                    string q1LeastSigDigits = Convert.ToString(cvc3T1AsShort);
                    q1LeastSigDigits = q1LeastSigDigits.Substring(q1LeastSigDigits.Length - bitmapPCVC3_TRACK1_9F62_KRN2.NonZeroCount);
                    t1d.Value.DiscretionaryData = Formatting.ASCIIStringToByteArray(bitmapPCVC3_TRACK1_9F62_KRN2.ReplaceValues(Formatting.ByteArrayToASCIIString(t1d.Value.DiscretionaryData), q1LeastSigDigits, bitmapPCVC3_TRACK1_9F62_KRN2.NonZeroCount, true));

                    TLV PUNATC_TRACK1_9F63_KRN2 = database.Get(EMVTagsEnum.PUNATC_TRACK1_9F63_KRN2);
                    MagBitmap bitmapPUNATC_TRACK1_9F63_KRN2 = new MagBitmap(PUNATC_TRACK1_9F63_KRN2.Value);
                    t1d.Value.DiscretionaryData = Formatting.ASCIIStringToByteArray(bitmapPUNATC_TRACK1_9F63_KRN2.ReplaceValues(Formatting.ByteArrayToASCIIString(t1d.Value.DiscretionaryData), unpredString, database.NUN, true));

                    if (t2 != 0)
                    {
                        TLV APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN = database.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN);
                        uint atcAsShort = Formatting.ConvertToInt32(APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Value.Reverse().ToArray());
                        string atcAsShortLeastSigDigits = Convert.ToString(atcAsShort);
                        atcAsShortLeastSigDigits = atcAsShortLeastSigDigits.PadLeft(t2, '0').Substring(atcAsShortLeastSigDigits.Length - t1);
                        t2d.Value.DiscretionaryData = Formatting.ASCIIStringToByteArray(bitmapPUNATC_TRACK1_9F63_KRN2.ReplaceValues(Formatting.ByteArrayToASCIIString(t1d.Value.DiscretionaryData), atcAsShortLeastSigDigits, t1, false));
                    }
                    #endregion

                    #region 14.30
                    StringBuilder dd1 = new StringBuilder(Formatting.ByteArrayToASCIIString(t1d.Value.DiscretionaryData));
                    dd1[dd1.Length - 1] = Convert.ToString(nUN)[0];
                    t1d.Value.DiscretionaryData = Formatting.ASCIIStringToByteArray(dd1.ToString());
                    #endregion

                    t1d.Serialize(); //reserialize in case the length of discretionary data changed
                    t1d.UpdateDB();
                }

                #region 14.32
                Kernel2OutcomeStatusEnum k2OutcomeStatus = Kernel2OutcomeStatusEnum.ONLINE_REQUEST;
                KernelCVMEnum cvmEnum = KernelCVMEnum.N_A;
                bool receipt = false;

                pcii = database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value[1];
                if ((pcii & 0x10) == 0x10)   //OD-CVM verification successful
                #endregion
                {
                    #region 14.34
                    cvmEnum = KernelCVMEnum.CONFIRMATION_CODE_VERIFIED;
                    long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
                    long rctl = database.ReaderContactlessTransactionLismit;
                    if (aa > rctl)
                        receipt = true;
                    #endregion
                }
                else
                {
                    #region 14.33
                    cvmEnum = KernelCVMEnum.NO_CVM;
                    #endregion
                }
                CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                CommonRoutines.CreateMSDataRecord(database);
                return CommonRoutines.PostOutcomeOnly(database, qManager, k2OutcomeStatus, cvmEnum, receipt);
            }
            else
            {
                #region 14.19.1
                if (((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x00030F) != 0x000000)
                #endregion
                {
                    KernelMessageidentifierEnum k2MessageIdentifier = KernelMessageidentifierEnum.DECLINED;
                    KernelStatusEnum k2Status = KernelStatusEnum.READY_TO_READ;
                    byte[] holdTime = new byte[] { 0x00, 0x00, 0x00 };

                    #region 14.22
                    PHONE_MESSAGE_TABLE_DF8131_KRN2 pmt = (PHONE_MESSAGE_TABLE_DF8131_KRN2)database.GetDefault(EMVTagsEnum.PHONE_MESSAGE_TABLE_DF8131_KRN2);
                    foreach (PhoneMessageTableEntry_DF8131 entry in pmt.Value.Entries)
                    {
                        int pcii = (int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value);
                        int pciMask = (int)Formatting.ConvertToInt32(entry.PCIIMask);
                        int pciValue = (int)Formatting.ConvertToInt32(entry.PCIIValue);
                        if ((pciMask & pcii) == pciValue)
                        {
                            k2MessageIdentifier = entry.MessageIdentifier;
                            k2Status = entry.Status;
                            holdTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;
                            break;
                        }
                    }
                    #endregion

                    #region 14.21.1
                    int waitTime = ((2 ^ database.FailedMSCntr) * 300);
                    Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
                    #endregion

                    #region 14.21.2
                    database.FailedMSCntr = Math.Min(database.FailedMSCntr + 1, 5);
                    #endregion

                    #region 14.23
                    CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                    CommonRoutines.CreateMSDataRecord(database);

                    return CommonRoutines.PostOutcome(database, qManager,
                        k2MessageIdentifier,
                        k2Status,
                        holdTime,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.B,
                        false,
                        KernelMessageidentifierEnum.N_A,
                        L1Enum.NOT_SET,
                        null,
                        L2Enum.NOT_SET,
                        L3Enum.NOT_SET);
                    #endregion
                }
                else
                {
                    #region 14.19.2.1
                    int waitTime = ((2 ^ database.FailedMSCntr) * 300);
                    Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
                    #endregion
                    #region 14.19.2.2
                    database.FailedMSCntr = Math.Min(database.FailedMSCntr + 1, 5);
                    #endregion

                    #region 14.19.3
                    CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                    CommonRoutines.CreateMSDataRecord(database);

                    return CommonRoutines.PostOutcome(database, qManager,
                        KernelMessageidentifierEnum.DECLINED,
                        KernelStatusEnum.NOT_READY,
                        database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value,
                        Kernel2OutcomeStatusEnum.END_APPLICATION,
                        Kernel2StartEnum.N_A,
                        true,
                        KernelMessageidentifierEnum.N_A,
                        L1Enum.NOT_SET,
                        null,
                        L2Enum.NOT_SET,
                        L3Enum.NOT_SET);

                    #endregion
                }
            }
        }
        /*
         * 14.1
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
            #region 14.2
            int waitTime = ((2 ^ database.FailedMSCntr) * 300);
            Task.Delay(TimeSpan.FromMilliseconds(waitTime)).Wait();
            #endregion

            #region 14.3
            database.FailedMSCntr = Math.Min(database.FailedMSCntr + 1, 5);
            #endregion

            #region 14.4 - 14.5
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.TRY_AGAIN,
                KernelStatusEnum.READY_TO_READ,
                new byte[] { 0x00, 0x00, 0x00 },
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.B,
                false,
                KernelMessageidentifierEnum.TRY_AGAIN,
                L1Enum.NOT_SET,
                cardResponse.ApduResponse.SW12,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
            #endregion
        }
        /*
        * 14.8
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_CCC_RESPONSE_2;
        }
        /*
         * 14.7
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_CCC_RESPONSE_2;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}
