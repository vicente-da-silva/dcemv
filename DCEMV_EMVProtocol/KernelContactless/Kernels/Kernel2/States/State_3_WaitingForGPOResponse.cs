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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class State_3_WaitingForGPOResponse
    {
        public static SignalsEnum Execute(
            Kernel2Database database, 
            KernelQ qManager, 
            CardQ cardQManager,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_3_WaitingForGPOResponse:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }
        /*
        * S3.1
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw)
        {
            #region S3.8
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 3.9.1 and 3.9.2
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.SELECT_NEXT,
                    Kernel2StartEnum.C,
                    null,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    L1Enum.NOT_SET,cardResponse.ApduResponse.SW12, 
                    L2Enum.STATUS_BYTES,
                    L3Enum.NOT_SET);
                #endregion
            }

            #region S3.10
            bool parsingResult = false;
            if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x77)
            {
                EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                parsingResult = database.ParseAndStoreCardResponse(response.ResponseData);
            }
            else
            {
                if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x80)
                {
                    EMVGetProcessingOptionsResponse response = cardResponse.ApduResponse as EMVGetProcessingOptionsResponse;
                    if (cardResponse.ApduResponse.ResponseData.Length < 6 || 
                        ((cardResponse.ApduResponse.ResponseData.Length - 2) % 4 != 0) ||
                            database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag) ||
                            database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                    {
                        parsingResult = false;
                    }
                    else
                    {
                        //database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN).Value = new byte[] { cardResponse.ApduResponse.ResponseData[0], cardResponse.ApduResponse.ResponseData[1]};
                        //byte[] afl = new byte[cardResponse.ApduResponse.ResponseData.Length - 2];
                        //Array.Copy(cardResponse.ApduResponse.ResponseData, 2, afl, 0, afl.Length);
                        //database.Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN).Value = afl;

                        //EMVGetProcessingOptionsResponse does the unpacking of the tags
                        foreach (TLV tlv in response.GetResponseTags())
                        {
                            parsingResult = database.ParseAndStoreCardResponse(tlv);
                            if (!parsingResult) break;
                        }
                    }
                }
            }
            #endregion

            #region S3.11
            if (!parsingResult)
            {
                #region S3.12
                return State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.PARSING_ERROR, L3Enum.NOT_SET);
                #endregion
            }
            else
            {
                #region S3.13
                if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag)) &&
                        database.IsNotEmpty(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.Tag))
                {
                    #region S3.14
                    return State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.CARD_DATA_MISSING, L3Enum.NOT_SET);
                    #endregion
                }
                else
                {
                    #region S3.15
                    KERNEL_CONFIGURATION_DF811B_KRN2 kc = new KERNEL_CONFIGURATION_DF811B_KRN2(database);
                    if (!kc.Value.EMVModeContactlessTransactionsNotSupported)
                    {
                        #region S3.16
                        APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
                        if (aip.Value.EMVModeIsSupported)
                        {
                            //goto 3.30
                            return DoEMVMode(database, qManager, cardQManager, cardResponse, sw);
                        }
                        #endregion
                    }

                    #region S3.17
                    if (kc.Value.MagStripeModeContactlessTransactionsNotSupported)
                    {
                        #region S3.18
                        return State_3_R1_CommonProcessing.DoInvalidReponse(database, qManager, L1Enum.NOT_SET, L2Enum.MAGSTRIPE_NOT_SUPPORTED, L3Enum.NOT_SET);
                        #endregion
                    }
                    else
                    {
                        //goto 3.70
                        return DoMagMode(database, qManager, cardQManager);
                    }
                    #endregion

                    #endregion
                }
                #endregion
            }
            #endregion
        }
        private static SignalsEnum DoEMVMode(Kernel2Database database, KernelQ qManager, CardQ cardQManager, CardResponse cardResponse, Stopwatch sw)
        {
            #region 3.30
            KERNEL_CONFIGURATION_DF811B_KRN2 kc = new KERNEL_CONFIGURATION_DF811B_KRN2(database);

            TLV aflRaw = database.Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN);
            byte[] kcValCheck = new byte[4];
            Array.Copy(aflRaw.Value, 0, kcValCheck, 0, kcValCheck.Length);
            if(aflRaw.Value.Length >= 4 && Formatting.ByteArrayToHexString(kcValCheck) == "08010100" && !kc.Value.MagStripeModeContactlessTransactionsNotSupported)
            {
                #region 3.32
                byte[] activeAFLBytes = new byte[aflRaw.Value.Length - 4];
                Array.Copy(aflRaw.Value, 4, activeAFLBytes, 0, activeAFLBytes.Length);

                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)activeAFLBytes.Length },
                    activeAFLBytes
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);

                //database.ActiveAFL.Value.Entries.Add(new FILE_LOCATOR_ENTRY(activeAFLBytes));
                #endregion
            }
            else
            {
                #region 3.31
                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)aflRaw.Value.Length },
                    aflRaw.Value
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);

                //database.ActiveAFL.Value.Entries.Add(new FILE_LOCATOR_ENTRY(aflRaw.Value));
                #endregion
            }
            #endregion

            #region 3.33
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            if(aip.Value.OnDeviceCardholderVerificationIsSupported && kc.Value.OnDeviceCardholderVerificationSupported)
            {
                #region 3.35
                database.ReaderContactlessTransactionLismit = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CONTACTLESS_TRANSACTION_LIMIT_ONDEVICE_CVM_DF8125_KRN2).Value);
                #endregion
            }
            else
            {
                #region 3.34
                database.ReaderContactlessTransactionLismit = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CONTACTLESS_TRANSACTION_LIMIT_NO_ONDEVICE_CVM_DF8124_KRN2).Value);
                #endregion
            }
            #endregion

            #region 3.60
            if (kc.Value.RelayResistanceProtocolSupported && aip.Value.RelayResistanceProtocolIsSupported)
            #endregion
            {
                #region 3.61
                database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value = Formatting.GetRandomNumber();
                database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN).Value;
                #endregion
                #region 3.62
                EMVExchangeRelayResistanceDataRequest request = new EMVExchangeRelayResistanceDataRequest(database.Get(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2).Value);
                #endregion
                #region 3.63
                sw.Start();
                #endregion
                #region 3.64
                cardQManager.EnqueueToInput(new CardRequest(request,CardinterfaceServiceRequestEnum.ADPU));
                #endregion
                return SignalsEnum.WAITING_FOR_EXCHANGE_RELAY_RESISTANCE_DATA_RESPONSE;
            }
            else
            {
                #region 3.65
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                tvr.Value.RelayResistancePerformedEnum = RelayResistancePerformedEnum.RRP_NOT_PERFORMED;
                tvr.UpdateDB();
                #endregion
                return State_3_R1_CommonProcessing.DoCommonProcessing("State_3_WaitingForGPOResponse", database, qManager, cardQManager, cardResponse);
            }
        }
        private static SignalsEnum DoMagMode(Kernel2Database database, KernelQ qManager, CardQ cardQManager)
        {
            KERNEL_CONFIGURATION_DF811B_KRN2 kc = new KERNEL_CONFIGURATION_DF811B_KRN2(database);

            TLV aflRaw = database.Get(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN);
            byte[] kcValCheck = new byte[4];
            Array.Copy(aflRaw.Value, 0, kcValCheck, 0, kcValCheck.Length);
            #region 3.70
            if (aflRaw.Value.Length >= 4 && Formatting.ByteArrayToHexString(kcValCheck) == "08010100")
            #endregion
            {
                #region 3.72
                byte[] activeAFLBytes = new byte[4];
                Array.Copy(aflRaw.Value, 0, activeAFLBytes, 0, activeAFLBytes.Length);

                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)activeAFLBytes.Length },
                    activeAFLBytes
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);
                //database.ActiveAFL.Value.Entries.Add(new FILE_LOCATOR_ENTRY(activeAFLBytes));
                #endregion
            }
            else
            {
                #region 3.71
                List<byte[]> bytes = new List<byte[]>
                {
                    new byte[] { (byte)aflRaw.Value.Length },
                    aflRaw.Value
                };
                database.ActiveAFL.Value.Deserialize(bytes.SelectMany(a => a).ToArray(), 0);
                //database.ActiveAFL.Value.Entries.Add(new FILE_LOCATOR_ENTRY(aflRaw.Value));
                #endregion
            }

            #region 3.73
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            if (aip.Value.OnDeviceCardholderVerificationIsSupported && kc.Value.OnDeviceCardholderVerificationSupported)
            {
                #region 3.75
                database.ReaderContactlessTransactionLismit = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CONTACTLESS_TRANSACTION_LIMIT_ONDEVICE_CVM_DF8125_KRN2).Value);
                #endregion
            }
            else
            {
                #region 3.74
                database.ReaderContactlessTransactionLismit = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CONTACTLESS_TRANSACTION_LIMIT_NO_ONDEVICE_CVM_DF8124_KRN2).Value);
                #endregion
            }
            #endregion

            #region 3.76
            TLVList dataToSend = database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children;
            TLVList tagsToRemove = new TLVList();
            foreach (TLV ttry in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(ttry.Tag.TagLable))
                {
                    dataToSend.AddToList(ttry);
                    tagsToRemove.AddToList(ttry);
                }
            }
            foreach (TLV ttr in tagsToRemove)
                database.TagsToReadYet.RemoveFromList(ttr);
            #endregion

            #region 3.77
            if (database.IsNotEmptyList(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2.Tag) ||
                (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0))
            {
                #region 3.78
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                #endregion
            }
            
            #region 3.80
            EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
            #endregion
            #region 3.81
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            #endregion
            return SignalsEnum.WAITING_FOR_MAG_STRIPE_READ_RECORD_RESPONSE;
            #endregion
        }
        /*
         * S3.2 - S3.3
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_GPO_REPONSE;
        }
        /*
         * S3.4 - S3.5
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.TRY_AGAIN, Kernel2StartEnum.B, cardResponse.L1Enum, L2Enum.NOT_SET, L3Enum.NOT_SET);
        }
        /*
         * S3.6, S3.7
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
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
