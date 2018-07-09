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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_7_WaitingForMagStripeReadRecordResponse
    {
        public static SignalsEnum Execute(
            Kernel2Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
            TornTransactionLogManager tornTransactionLogManager,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_7_WaitingForMagStripeReadRecordResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, tornTransactionLogManager, sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager, tornTransactionLogManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_7_WaitingForMagStripeReadRecordResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * 7.3
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw)
        {
            #region 7.9
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 7.10.1 - 7.10.2
                CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
                    true,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    L1Enum.NOT_SET,
                    cardResponse.ApduResponse.SW12,
                    L2Enum.STATUS_BYTES,
                    L3Enum.NOT_SET);
                #endregion
                
            }

            #region 7.11
            bool parsingResult = false;
            if(cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x70)
            {
                parsingResult = database.ParseAndStoreCardResponse(cardResponse.ApduResponse.ResponseData);
            }
            else
            {
                parsingResult = false;
            }
            #endregion

            if (!parsingResult)
            {
                #region 7.13.1
                CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
                    true,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    L1Enum.NOT_SET,
                    null,
                    L2Enum.PARSING_ERROR,
                    L3Enum.NOT_SET);
                #endregion
            }

            #region 7.14
            if((cardResponse.ApduResponse as EMVReadRecordResponse).GetResponseTags().IsPresent(EMVTagsEnum.UDOL_9F69_KRN2.Tag))
            #endregion
            {
                #region 7.15
                TLV udol = (cardResponse.ApduResponse as EMVReadRecordResponse).GetResponseTags().Get(EMVTagsEnum.UDOL_9F69_KRN2.Tag);
                foreach(TLV tlv in udol.Children)
                {
                    if (database.IsEmpty(tlv.Tag.TagLable))
                    {
                        database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Children.AddToList(tlv);
                    }
                }
                #endregion
            }

            #region 7.16
            database.ActiveAFL.Value.Entries.RemoveAt(0);
            #endregion

            #region 7.17
            if(database.ActiveAFL.Value.Entries.Count != 0)
            #endregion
            {
                #region 7.18 - 7.19
                EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_MAG_STRIPE_READ_RECORD_RESPONSE;
                #endregion
            }

            #region 7.20
            if(database.IsEmpty(EMVTagsEnum.TRACK_2_DATA_9F6B_KRN2.Tag) ||
                database.IsEmpty(EMVTagsEnum.PUNATC_TRACK2_9F66_KRN2.Tag) ||
                database.IsEmpty(EMVTagsEnum.PCVC3_TRACK2_9F65_KRN2.Tag) ||
                database.IsEmpty(EMVTagsEnum.NATC_TRACK2_9F67_KRN2.Tag))
            #endregion
            {
                #region 7.21.1
                CommonRoutines.CreateMSDiscretionaryDataRecord(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
                    true,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    L1Enum.NOT_SET,
                    null,
                    L2Enum.CARD_DATA_MISSING,
                    L3Enum.NOT_SET);
                #endregion
            }

            #region 7.22
            TLV punatc2 = database.Get(EMVTagsEnum.PUNATC_TRACK2_9F66_KRN2);
            int natc2 = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.NATC_TRACK2_9F67_KRN2).Value);
            int nonZeroBits = 0;
            for(int i = 0; i < 8; i++)
            {
                if(((punatc2.Value[0] >> i) & 0x01) == 0x01)
                    nonZeroBits++;
                if (((punatc2.Value[1] >> i) & 0x01) == 0x01)
                    nonZeroBits++;
            }
            int check2 = nonZeroBits - natc2;
            database.NUN = check2;
            if(check2 < 0 && check2 > 8)
            {
                #region 7.24.1
                return DoInvalidResponse(database, qManager);
                #endregion
            }

            if (database.IsNotEmpty(EMVTagsEnum.TRACK_1_DATA_56_KRN2.Tag))
            {
                int check1 = 0;
                if (database.IsNotEmpty(EMVTagsEnum.NATC_TRACK1_9F64_KRN2.Tag) && database.IsNotEmpty(EMVTagsEnum.PUNATC_TRACK1_9F63_KRN2.Tag))
                {
                    TLV punatc1 = database.Get(EMVTagsEnum.PUNATC_TRACK1_9F63_KRN2);
                    int natc1 = Formatting.ConvertToInt16(database.Get(EMVTagsEnum.NATC_TRACK1_9F64_KRN2).Value);
                    int nonZeroBits1 = 0;
                    for (int i = 0; i < 8; i++)
                    {
                        if (((punatc1.Value[0] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                        if (((punatc1.Value[1] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                        if (((punatc1.Value[2] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                        if (((punatc1.Value[3] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                        if (((punatc1.Value[4] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                        if (((punatc1.Value[5] >> i) & 0x01) == 0x01)
                            nonZeroBits1++;
                    }
                    check1 = nonZeroBits1 - natc1;
                }
                else
                {
                    #region 7.24.1
                    return DoInvalidResponse(database, qManager);
                    #endregion
                }

                if (   (database.IsNotPresent(EMVTagsEnum.NATC_TRACK1_9F64_KRN2.Tag) || database.IsEmpty(EMVTagsEnum.NATC_TRACK1_9F64_KRN2.Tag)) ||
                       (database.IsNotPresent(EMVTagsEnum.PCVC3_TRACK1_9F62_KRN2.Tag) || database.IsEmpty(EMVTagsEnum.PCVC3_TRACK1_9F62_KRN2.Tag)) ||
                       (database.IsNotPresent(EMVTagsEnum.PUNATC_TRACK1_9F63_KRN2.Tag) || database.IsEmpty(EMVTagsEnum.PUNATC_TRACK1_9F63_KRN2.Tag)) ||
                       check1 != check2
                       )
                {
                    #region 7.24.1
                    return DoInvalidResponse(database, qManager);
                    #endregion
                }
            }
            #region 7.23
            TRACK_2_DATA_9F6B_KRN2 t2d = new TRACK_2_DATA_9F6B_KRN2(database);
            TLV ddCardT2 = database.Get(EMVTagsEnum.DD_CARD_TRACK2_DF812B_KRN2);
            if (ddCardT2 == null)
                ddCardT2 = TLV.Create(EMVTagsEnum.DD_CARD_TRACK2_DF812B_KRN2.Tag);
            ddCardT2.Value = t2d.Value.DiscretionaryData;
            if (database.IsNotEmpty(EMVTagsEnum.TRACK_1_DATA_56_KRN2.Tag))
            {
                TRACK_1_DATA_56_KRN2 t1d = new TRACK_1_DATA_56_KRN2(database);

                TLV ddCardT1 = database.Get(EMVTagsEnum.DD_CARD_TRACK1_DF812A_KRN2);
                if (ddCardT1 == null)
                    ddCardT1 = TLV.Create(EMVTagsEnum.DD_CARD_TRACK1_DF812A_KRN2.Tag);

                ddCardT1.Value = t1d.Value.DiscretionaryData;
            }
            return State_7_8_CommonProcessing.DoCommonProcessing("State_7_WaitingForMagStripeReadRecordResponse", database, qManager, cardQManager, sw);
            #endregion

            #endregion
        }

        private static SignalsEnum DoInvalidResponse(Kernel2Database database, KernelQ qManager)
        {
            #region 7.24.1
            CommonRoutines.CreateMSDiscretionaryDataRecord(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                KernelStatusEnum.NOT_READY,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                L1Enum.NOT_SET,
                null,
                L2Enum.CARD_DATA_ERROR,
                L3Enum.NOT_SET);
            #endregion
        }
        /*
        * 7.1
        */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_MAG_STRIPE_READ_RECORD_RESPONSE;
        }
        /*
         * 7.4, 7.5, 7.6
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, TornTransactionLogManager tornTransactionLogManager)
        {
            CommonRoutines.CreateMSDiscretionaryDataRecord(database);
            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.TRY_AGAIN,
                KernelStatusEnum.READY_TO_READ,
                new byte[] { 0x00, 0x00, 0x00 },
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.B,
                false,
                KernelMessageidentifierEnum.TRY_AGAIN,
                cardResponse.L1Enum,
                null,
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
        }
        /*
         * 7.7,7.8
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateMSDiscretionaryDataRecord(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}
