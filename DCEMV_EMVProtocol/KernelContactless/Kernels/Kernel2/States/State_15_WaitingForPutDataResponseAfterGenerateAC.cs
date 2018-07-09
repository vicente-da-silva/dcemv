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
    public static class State_15_WaitingForPutDataResponseAfterGenerateAC
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_15_WaitingForPutDataResponseAfterGenerateAC:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_15_WaitingForPutDataResponseAfterGenerateAC:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        private static SignalsEnum DoCommon(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
            #region 15.9.1
            if (database.IsNotEmpty(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2.Tag) &&
               ((int)Formatting.ConvertToInt32(database.Get(EMVTagsEnum.POS_CARDHOLDER_INTERACTION_INFORMATION_DF4B_KRN2).Value) & 0x00030F) != 0x000000)
            #endregion
            {
                #region 15.10
                CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, true);
                #endregion

                //TODO: this changes ui in db which means when response is dequeued, it will have been
                //changed by code below, should we not create a copy in Kernel2Response constructor
                #region 15.11
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.N_A,
                    KernelStatusEnum.READY_TO_READ,
                    new byte[] { 0x00, 0x00, 0x00 },
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
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
                #region 15.12
                CommonRoutines.PostUIOnly(database, qManager, KernelMessageidentifierEnum.CLEAR_DISPLAY, KernelStatusEnum.CARD_READ_SUCCESSFULLY, false);

                //TODO: we created a temp ui and never saved it to db, so what ui is used below
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.N_A,
                    KernelStatusEnum.N_A,
                    new byte[] { 0x00, 0x00, 0x00 },
                    Kernel2OutcomeStatusEnum.N_A,
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

        /*
        * 15.2
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw)
        {
            #region 15.5
            if (cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 15.6
                if(database.TagsToWriteAfterGenACYet.Count != 0)
                #endregion
                {
                    #region 15.7
                    TLV tagToPut = database.TagsToWriteAfterGenACYet.GetFirstAndRemoveFromList();
                    EMVPutDataRequest requestPutData = new EMVPutDataRequest(tagToPut);
                    #endregion
                    #region 15.8
                    cardQManager.EnqueueToInput(new CardRequest(requestPutData, CardinterfaceServiceRequestEnum.ADPU));
                    return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC;
                    #endregion
                }
                else
                {
                    #region 15.9
                    byte pgacps = database.Get(EMVTagsEnum.POSTGEN_AC_PUT_DATA_STATUS_DF810E_KRN2).Value[0];
                    pgacps = (byte)(pgacps | 0x80);
                    database.Get(EMVTagsEnum.POSTGEN_AC_PUT_DATA_STATUS_DF810E_KRN2).Value[0] = pgacps;
                    #endregion
                }
            }

            return DoCommon(database, cardResponse, qManager);
        }

        /*
         * 15.1
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
            return DoCommon(database, cardResponse, qManager);
        }
        /*
         * 15.3
         */
        private static SignalsEnum EntryPointSTOP(Kernel2Database database, KernelQ qManager)
        {
            return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC;
        }
        /*
         * 15.4
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            return SignalsEnum.WAITING_FOR_PUT_DATA_RESPONSE_AFTER_GEN_AC;
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }
}
