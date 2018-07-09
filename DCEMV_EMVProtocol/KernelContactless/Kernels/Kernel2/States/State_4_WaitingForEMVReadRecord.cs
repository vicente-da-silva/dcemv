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
    public static class State_4_WaitingForEMVReadRecord
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_4_WaitingForEMVReadRecord:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
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
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_4_WaitingForEMVReadRecord:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        /*
        * S4.3
        */
        private static SignalsEnum EntryPointRA(Kernel2Database database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, TornTransactionLogManager tornTransactionLogManager, Stopwatch sw)
        {
            bool signedFlag;
            byte sfi;

            #region 4.9
            if (!cardResponse.ApduResponse.Succeeded)
            #endregion
            {
                #region 4.10
                CommonRoutines.CreateEMVDiscretionaryData(database);
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

            #region 4.11
            if (database.ActiveAFL.Value.Entries[0].OfflineDataAuthenticationRecordLength > 0)
            #endregion
            {
                #region 4.12
                signedFlag = true;
                #endregion
            }
            else
            {
                #region 4.13
                signedFlag = false;
                #endregion
            }

            #region 4.14
            sfi = database.ActiveAFL.Value.Entries[0].SFI;
            database.ActiveAFL.Value.Entries.RemoveAt(0);
            #endregion

            #region 4.15
            TLV nextTLV = database.TagsToReadYet.GetNextGetDataTagFromList();
            if (nextTLV != null)
                database.ActiveTag = nextTLV.Tag.TagLable;
            else
                database.ActiveTag = null;
            #endregion
            if (database.ActiveTag != null)
            {
                #region 4.16 - 4.19
                EMVGetDataRequest request = new EMVGetDataRequest(Formatting.HexStringToByteArray(database.ActiveTag));
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                database.NextCommandEnum = NextCommandEnum.GET_DATA;
                #endregion
            }
            else
            {
                #region 4.19
                if (database.ActiveAFL.Value.Entries.Count == 0)
                #endregion
                {
                    #region 4.20
                    database.NextCommandEnum = NextCommandEnum.NONE;
                    #endregion
                }
                else
                {
                    #region 4.21 - 4.23
                    EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                    cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                    database.NextCommandEnum = NextCommandEnum.READ_RECORD;
                    #endregion
                }

            }

            bool parsingResult;
            #region 4.24
            if (sfi <= 10)
            #endregion
            {
                if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x70)
                    parsingResult = database.ParseAndStoreCardResponse(cardResponse.ApduResponse.ResponseData);
                else
                    parsingResult = false;
            }
            else //Processing of records in proprietary files is beyond the scope of thisspecification
                parsingResult = false;
            
            #region 4.25
            if (!parsingResult)
            #endregion
            {
                #region 4.26
                if (database.NextCommandEnum == NextCommandEnum.NONE)
                #endregion
                {
                    #region 4.27.1 - 4.27.2
                    CommonRoutines.CreateEMVDiscretionaryData(database);
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
                else
                {
                    return SignalsEnum.TEMINATE_ON_NEXT_RA;
                }
            }

            TLVList responseTags;
            if (cardResponse.ApduResponse is EMVReadRecordResponse)
                responseTags = (cardResponse.ApduResponse as EMVReadRecordResponse).GetResponseTags();
            else if (cardResponse.ApduResponse is EMVGetDataResponse)
                responseTags = (cardResponse.ApduResponse as EMVGetDataResponse).GetResponseTags();
            else
                throw new EMVProtocolException("Invalid card response in State4");

            #region 4.28
            TLVList cdols = responseTags.FindAll(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN.Tag);
            if (cdols.Count > 0)
            #endregion
            {
                TLVList cdolList = TLV.DeserializeChildrenWithNoV(cdols.GetFirst().Value, 0);
                DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                #region 4.29
                foreach (TLV tlv in cdolList)
                {
                    if (database.IsEmpty(tlv.Tag.TagLable))
                        dataNeeded.Value.Tags.Add(tlv.Tag.TagLable);
                }
                dataNeeded.UpdateDB();
                #endregion
            }

            #region 4.30
            TLVList dsdols = responseTags.FindAll(EMVTagsEnum.DSDOL_9F5B_KRN2.Tag);
            if (dsdols.Count > 0)
            #endregion
            {
                #region 4.31
                IDS_STATUS_DF8128_KRN2 ids = new IDS_STATUS_DF8128_KRN2(database);
                if (ids.Value.IsRead)
                #endregion
                {
                    #region 4.32
                    if (database.IsNotEmpty(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2.Tag) && new DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2(database).Value.LockedSlot)
                    #endregion
                    {
                        DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                        #region 4.33
                        foreach (TLV tlv in dsdols.GetFirst().Children)
                        {
                            if (database.IsEmpty(tlv.Tag.TagLable))
                                dataNeeded.Value.Tags.Add(tlv.Tag.TagLable);
                        }
                        dataNeeded.UpdateDB();
                        #endregion
                    }
                    else
                    {
                        #region 4.34
                        Do434(database, signedFlag, sfi, cardResponse, responseTags);
                        #endregion
                    }

                }
                else
                {
                    #region 4.34
                    Do434(database, signedFlag, sfi, cardResponse, responseTags);
                    #endregion
                }

            }
            else
            {
                #region 4.34
                Do434(database, signedFlag, sfi, cardResponse, responseTags);
                #endregion
            }

            return State_4_5_6_CommonProcessing.DoCommonProcessing("State_4_WaitingForEMVReadRecord", database, qManager, cardQManager, sw, tornTransactionLogManager);
        }
        private static void Do434(Kernel2Database database, bool signedFlag, int sfi, CardResponse cardResponse, TLVList responseTags)
        {
            #region 4.34
            if (signedFlag && database.ODAStatus == 0x80)
            {
                #region 4.35
                if (sfi <= 10)
                {
                    int length = cardResponse.ApduResponse.ResponseData.Length - 2; //without tag '70' and length
                    if (2048 - database.StaticDataToBeAuthenticated.Serialize().Length >= length)
                    {
                        database.StaticDataToBeAuthenticated.AddListToList(responseTags);
                    }
                    else
                    {
                        TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                        tvr.Value.CDAFailed = true;
                        tvr.UpdateDB();
                    }
                }
                else
                {
                    int length = cardResponse.ApduResponse.ResponseData.Length;
                    if (cardResponse.ApduResponse.ResponseData[0] == 0x70 && 2048 - database.StaticDataToBeAuthenticated.Serialize().Length >= length)
                    {
                        TLV template = TLV.Create(EMVTagsEnum.READ_RECORD_RESPONSE_MESSAGE_TEMPLATE_70_KRN.Tag);
                        template.Children.AddListToList(responseTags);
                        template.Serialize();
                        database.StaticDataToBeAuthenticated.AddToList(template);
                    }
                    else
                    {
                        TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                        tvr.Value.CDAFailed = true;
                        tvr.UpdateDB();
                    }
                }
                #endregion
            }
            #endregion
        }

        /*
         * S4.1 - S4.2
         */
        private static SignalsEnum EntryPointDET(Kernel2Database database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;// RA_NEXT_CMD_EMV_READ_RECORD;
        }
        /*
         * S4.4 - S4.6
         */
        private static SignalsEnum EntryPointL1RSP(Kernel2Database database, CardResponse cardResponse, KernelQ qManager)
        {
            CommonRoutines.InitializeDiscretionaryData(database);
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
        }
        /*
         * S4.7 - S4.8
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
