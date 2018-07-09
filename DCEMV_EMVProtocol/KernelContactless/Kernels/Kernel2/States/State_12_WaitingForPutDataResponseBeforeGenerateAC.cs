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
    public static class State_12_WaitingForPutDataResponseBeforeGenerateAC
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_12_WaitingForPutDataResponseBeforeGenerateAC:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_12_WaitingForPutDataResponseBeforeGenerateAC:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        /*
        * S12.2
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw)
        {
            #region 12.8
            if (cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 12.9
                if(database.TagsToWriteBeforeGenACYet.Count != 0)
                #endregion
                {
                    #region 12.10
                    TLV tagToPut = database.TagsToWriteBeforeGenACYet.GetFirstAndRemoveFromList();
                    EMVPutDataRequest requestPutData = new EMVPutDataRequest(tagToPut);
                    #endregion
                    #region 12.11
                    cardQManager.EnqueueToInput(new CardRequest(requestPutData, CardinterfaceServiceRequestEnum.ADPU));
                    return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_BEFORE_GEN_AC;
                    #endregion
                }
                else
                {
                    #region 12.12
                    byte pgacps = database.Get(EMVTagsEnum.PREGEN_AC_PUT_DATA_STATUS_DF810F_KRN2).Value[0];
                    pgacps = (byte)(pgacps | 0x80);
                    database.Get(EMVTagsEnum.PREGEN_AC_PUT_DATA_STATUS_DF810F_KRN2).Value[0] = pgacps;
                    #endregion
                }
            }

            #region 12.13
            int mnttl = (int)Formatting.ConvertToInt32(database.GetDefault(EMVTagsEnum.MAX_NUMBER_OF_TORN_TRANSACTION_LOG_RECORDS_DF811D_KRN2).Value);
            if(database.IsNotEmpty(EMVTagsEnum.DRDOL_9F51_KRN2.Tag) && mnttl != 0)
            #endregion
            {
                #region 12.14
                foreach(TORN_RECORD_FF8101_KRN2 ttr in tornTransactionLogManager.TornTransactionLogs)
                {
                    string pan = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN).Value);
                    string panR = Formatting.ByteArrayToHexString(ttr.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag).Value);

                    if (ttr.Children.IsNotEmpty(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag))
                    {
                        string sn = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN).Value);
                        string snR = Formatting.ByteArrayToHexString(ttr.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag).Value);

                        if(pan == panR  && sn == snR)
                        {
                            database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value = ttr.Value;

                            #region 12.17
                            database.TornTempRecord.Value = database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value;
                            #endregion

                            #region 12.18
                            database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2).Value = database.TornTempRecord.Children.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2.Tag).Value;
                            EMVRecoverACRequest requestRecover = new EMVRecoverACRequest(database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2));
                            #endregion

                            #region 12.19
                            cardQManager.EnqueueToInput(new CardRequest(requestRecover, CardinterfaceServiceRequestEnum.ADPU));
                            return SignalsEnum.WAITING_FOR_RECOVER_AC;
                            #endregion
                        }
                    }
                    else
                    {
                        if (pan == panR)
                        {
                            database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value = ttr.Value;

                            #region 12.17
                            database.TornTempRecord.Value = database.Get(EMVTagsEnum.TORN_RECORD_FF8101_KRN2).Value;
                            #endregion

                            #region 12.18
                            database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2).Value = database.TornTempRecord.Children.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2.Tag).Value;
                            EMVRecoverACRequest requestRecover = new EMVRecoverACRequest(database.Get(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2));
                            #endregion

                            #region 12.19
                            cardQManager.EnqueueToInput(new CardRequest(requestRecover, CardinterfaceServiceRequestEnum.ADPU));
                            return SignalsEnum.WAITING_FOR_RECOVER_AC;
                            #endregion
                        }
                    }
                }
                #endregion
            }

            #region 12.15
            EMVGenerateACRequest request = PrepareGenACCommandProcedure_7_6.PrepareGenACCommand(database, qManager, cardQManager);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            return SignalsEnum.WAITING_FOR_GEN_AC_1;
            #endregion
        }
        /*
         * 12.4
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_BEFORE_GEN_AC;
        }
        /*
         * 12.1,12.5, 12.6
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
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
                L2Enum.NOT_SET,
                L3Enum.NOT_SET);
        }
        /*
         * S12.3, 12.7
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
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
