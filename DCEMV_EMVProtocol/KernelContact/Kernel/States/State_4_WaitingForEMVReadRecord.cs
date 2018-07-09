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
using DCEMV.EMVProtocol.Kernels.K2;
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_4_WaitingForEMVReadRecord
    {
        public static SignalsEnum Execute(
           KernelDatabase database,
           KernelQ qManager,
           CardQ cardQManager,
           EMVSelectApplicationResponse emvSelectApplicationResponse,
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
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, emvSelectApplicationResponse, sw);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_4_WaitingForEMVReadRecord:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }


        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, EMVSelectApplicationResponse emvSelectApplicationResponse, Stopwatch sw)
        {
            bool signedFlag;
            byte sfi;

            if (!cardResponse.ApduResponse.Succeeded)
            {
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
            }

            if (database.ActiveAFL.Value.Entries[0].OfflineDataAuthenticationRecordLength > 0)
            {
                signedFlag = true;
            }
            else
            {
                signedFlag = false;
            }

            sfi = database.ActiveAFL.Value.Entries[0].SFI;
            database.ActiveAFL.Value.Entries.RemoveAt(0);

            TLV nextTLV = database.TagsToReadYet.GetNextGetDataTagFromList();
            if (nextTLV != null)
                database.ActiveTag = nextTLV.Tag.TagLable;
            else
                database.ActiveTag = null;

            if (database.ActiveAFL.Value.Entries.Count == 0)
            {
                database.NextCommandEnum = NextCommandEnum.NONE;
            }
            else
            {
                EMVReadRecordRequest request = new EMVReadRecordRequest(database.ActiveAFL.Value.Entries[0].SFI, database.ActiveAFL.Value.Entries[0].FirstRecordNumber);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                database.NextCommandEnum = NextCommandEnum.READ_RECORD;
            }

            bool parsingResult;
            if (sfi <= 10)
            {
                //added for cards returning misformtted data, read records instruction returns no data
                if (cardResponse.ApduResponse.ResponseData.Length == 0)
                    parsingResult = true;
                else
                {
                    if (cardResponse.ApduResponse.ResponseData.Length > 0 && cardResponse.ApduResponse.ResponseData[0] == 0x70)
                        parsingResult = database.ParseAndStoreCardResponse(cardResponse.ApduResponse.ResponseData);
                    else
                        parsingResult = false;
                }
            }
            else //Processing of records in proprietary files is beyond the scope of this specification
                parsingResult = false;

            if (!parsingResult)
            {
                if (database.NextCommandEnum == NextCommandEnum.NONE)
                {
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

                TLVList cdols = responseTags.FindAll(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN.Tag);
                if (cdols.Count > 0)
                {
                    TLVList cdolList = TLV.DeserializeChildrenWithNoV(cdols.GetFirst().Value, 0);
                    DATA_NEEDED_DF8106_KRN2 dataNeeded = new DATA_NEEDED_DF8106_KRN2(database);
                    foreach (TLV tlv in cdolList)
                    {
                        if (database.IsEmpty(tlv.Tag.TagLable))
                            dataNeeded.Value.Tags.Add(tlv.Tag.TagLable);
                    }
                    dataNeeded.UpdateDB();
                }

                UpdateStaticDataToBeAuthenticated(database, signedFlag, sfi, cardResponse, responseTags);
            
            return DoCommonProcessing("State_4_WaitingForEMVReadRecord", database, qManager, cardQManager, sw, emvSelectApplicationResponse);
        }
        private static void UpdateStaticDataToBeAuthenticated(KernelDatabase database, bool signedFlag, int sfi, CardResponse cardResponse, TLVList responseTags)
        {
            if (signedFlag)
            {
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
            }
        }


        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;// RA_NEXT_CMD_EMV_READ_RECORD;
        }

        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
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

        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }
        public static SignalsEnum DoCommonProcessing(string source, KernelDatabase database, KernelQ qManager, CardQ cardQManager, Stopwatch sw, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            if (database.NextCommandEnum == NextCommandEnum.READ_RECORD)
            {
                DoDEKIfNeeded(database, qManager);
                return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
            }

            DoDEKIfNeeded(database, qManager);

            if (database.IsEmpty(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag))
            {
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.AMOUNT_NOT_PRESENT);
            }

            if (!(database.IsNotEmpty(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag) &&
                database.IsNotEmpty(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN.Tag)))
            {
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcome(database, qManager,
                    KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                    KernelStatusEnum.NOT_READY,
                    null,
                    Kernel2OutcomeStatusEnum.END_APPLICATION,
                    Kernel2StartEnum.N_A,
                    true,
                    KernelMessageidentifierEnum.N_A,
                    L1Enum.NOT_SET,
                    null,
                    L2Enum.CARD_DATA_MISSING,
                    L3Enum.NOT_SET);
            }

            #region Book 3 Section 10.3 
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aipCheck = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            if ((aipCheck.Value.CDASupported && tc.Value.CDACapable) ||
                aipCheck.Value.DDAsupported && tc.Value.DDACapable ||
                aipCheck.Value.SDASupported && tc.Value.SDACapable)
            {
                if (aipCheck.Value.CDASupported && tc.Value.CDACapable)
                {
                    //some checking in preperation for ODA is done here, ODA is done after 1st gen ac
                    if (!(
                        database.IsNotEmpty(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN.Tag) &&
                        database.IsNotEmpty(EMVTagsEnum.ISSUER_PUBLIC_KEY_CERTIFICATE_90_KRN.Tag) &&
                        database.IsNotEmpty(EMVTagsEnum.ISSUER_PUBLIC_KEY_EXPONENT_9F32_KRN.Tag) &&
                        database.IsNotEmpty(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_CERTIFICATE_9F46_KRN.Tag) &&
                        database.IsNotEmpty(EMVTagsEnum.INTEGRATED_CIRCUIT_CARD_ICC_PUBLIC_KEY_EXPONENT_9F47_KRN.Tag) //&&
                        //database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN.Tag)
                        ))
                    {
                        tvr.Value.ICCDataMissing = true;
                        tvr.Value.CDAFailed = true;
                    }
                }

                string aid = emvSelectApplicationResponse.GetDFName();
                string rid = aid.Substring(0, 10);
                RIDEnum ridEnum = (RIDEnum)Enum.Parse(typeof(RIDEnum), rid);
                if (database.PublicKeyCertificateManager.GetCAPK(ridEnum, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]) == null)
                {
                    if (aipCheck.Value.CDASupported && tc.Value.CDACapable)
                    {
                        tvr.Value.CDAFailed = true;
                    }
                    if (aipCheck.Value.DDAsupported && tc.Value.DDACapable)
                    {
                        tvr.Value.DDAFailed = true;
                    }
                    if (aipCheck.Value.SDASupported && tc.Value.SDACapable)
                    {
                        tvr.Value.SDAFailed = true;
                    }
                }

                //bool aipFound = false;
                //TLV aip = null;
                //if (database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN.Tag))
                //{
                //    TLV sdal = database.Get(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_TAG_LIST_9F4A_KRN);
                //    TLVList list = TLV.DeserializeChildrenWithNoLV(sdal.Value, 0);
                //    //if (list.Count == 1)
                //    //{
                //    aip = list.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                //    if (aip != null)
                //    {
                //        aipFound = true;
                //    }
                //    //}
                //}
                //else
                //{
                //    if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag))
                //    {
                //        aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                //        aipFound = true;
                //    }
                //}
                //if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag))
                //{
                //    aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                //    aipFound = true;
                //}

                TLV aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN);
                if (database.IsEmpty(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag))
                {
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
                        L2Enum.CARD_DATA_ERROR,
                        L3Enum.NOT_SET);
                }

                int length = database.StaticDataToBeAuthenticated.Serialize().Length;
                if (2048 - length >= aip.Value.Length)
                {
                    //will be removed later when the SDA is done
                    database.StaticDataToBeAuthenticated.AddToList(aip);
                }
                //else
                //{
                //    if (aipCheck.Value.CDASupported && tc.Value.CDACapable)
                //    {
                //        tvr.Value.CDAFailed = true;
                //    }
                //    if (aipCheck.Value.DDAsupported && tc.Value.DDACapable)
                //    {
                //        tvr.Value.DDAFailed = true;
                //    }
                //    if (aipCheck.Value.SDASupported && tc.Value.SDACapable)
                //    {
                //        tvr.Value.SDAFailed = true;
                //    }
                //}
                tvr.UpdateDB();
            }
            #endregion

            #region Book 3 Section 10.4
            //using kernel 2 processing restrictions
            ProcessingRestrictions_7_7.ProcessingRestrictions(database);
            #endregion
            
            return SignalsEnum.WAITING_FOR_CVM_PROCESSING;
        }


        private static bool DoDEKIfNeeded(KernelDatabaseBase database, KernelQ qManager)
        {
            TLVList toRemove = new TLVList();
            foreach (TLV tlv in database.TagsToReadYet)
            {
                if (database.IsNotEmpty(tlv.Tag.TagLable))
                {
                    database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Children.AddToList(tlv);
                    toRemove.AddToList(tlv);
                }
            }
            foreach (TLV tlv in toRemove)
                database.TagsToReadYet.RemoveFromList(tlv);

            if (database.IsNotEmptyList(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.Tag) && database.TagsToReadYet.Count == 0)
            {
                CommonRoutines.PostDEK(database, qManager);
                database.Get(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2).Initialize();
                database.Get(EMVTagsEnum.DATA_NEEDED_DF8106_KRN2).Initialize();
                return true;
            }
            return false;
        }
    }
}
