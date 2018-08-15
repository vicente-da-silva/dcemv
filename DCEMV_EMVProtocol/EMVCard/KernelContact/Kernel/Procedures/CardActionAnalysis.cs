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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.EMVProtocol.Kernels.K;
using DCEMV.ISO7816Protocol;
using System;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Contact
{
    public static class CardActionAnalysis
    {
        private static ACTypeEnum GetDSACType(KernelDatabaseBase database)
        {
            return ((KernelDatabase)database).ACType.Value.DSACTypeEnum;
        }
        
        public static TLVList BuildScriptList(TLV scriptList)
        {
            TLVList scripts = new TLVList();
            foreach(TLV tlv in scriptList.Children)
            {
                if (tlv.Tag.TagLable == EMVTagsEnum.ISSUER_SCRIPT_COMMAND_86_KRN.Tag)
                    scripts.AddToList(tlv);
            }
            return scripts;
        }

        public static SignalsEnum Initiate2ndCardActionAnalysis(KernelDatabaseBase database, KernelQ qManager, CardQ cardQManager, EMVSelectApplicationResponse emvSelectApplicationResponse, bool entryFromScriptProcessingCompleted = false)
        {
            if (!entryFromScriptProcessingCompleted)
            {
                //check if scripts need to be run
                TLV _71 = database.Get(EMVTagsEnum.ISSUER_SCRIPT_TEMPLATE_1_71_KRN);
                if (_71 != null)
                {
                    ((KernelDatabase)database).ScriptsToRunBeforeGenAC = BuildScriptList(_71);
                    if (((KernelDatabase)database).ScriptsToRunBeforeGenAC.Count > 0)
                    {
                        ((KernelDatabase)database).IsScriptProcessingBeforeGenACInProgress = true;
                        //post first script
                        TLV firstScript = ((KernelDatabase)database).ScriptsToRunBeforeGenAC.GetFirstAndRemoveFromList();
                        EMVScriptCommandRequest scriptRequest = new EMVScriptCommandRequest();
                        scriptRequest.Deserialize(firstScript.Value);
                        cardQManager.EnqueueToInput(new CardRequest(scriptRequest, CardinterfaceServiceRequestEnum.ADPU));
                        return SignalsEnum.WAITING_FOR_SCRIPT_PROCESSING;
                    }
                }
            }
             
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);

            //section 6.5.5 in Book 3
            TLV cdol2 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_2_CDOL2_8D_KRN);
            byte[] cdol2Data = CommonRoutines.PackRelatedDataTag(database, cdol2);
                        
            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpST = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcpST.Value.ACTypeEnum = GetDSACType(database);
            if (aip.Value.CDASupported && tc.Value.CDACapable)
                rcpST.Value.CDASignatureRequested = true;
            else
                rcpST.Value.CDASignatureRequested = false;
            rcpST.UpdateDB();

            EMVGenerateACRequest request = new EMVGenerateACRequest(cdol2Data, null, rcpST);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));

            return SignalsEnum.WAITING_FOR_GEN_AC_2;
        }
        public static SignalsEnum InitiateCardActionAnalysis(KernelDatabaseBase database, KernelQ qManager, CardQ cardQManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
          
            //time to get signature data for dda cards, in order to do oda after 1st gen ac
            EMVGenerateACRequest request = null;
            if (aip.Value.CDASupported && tc.Value.CDACapable)
            {
                //cda done after gen ac 1, call gen ac1 now with signature requested
                request = CreateGenAC(database, true);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_GEN_AC_1;
            }
            if (aip.Value.DDAsupported && tc.Value.DDACapable)
            {
                #region Book 3 Section 10.3
                //do dda signature request, internal authenticate will do oda once it has the signature
                TLV ddol = database.Get(EMVTagsEnum.DYNAMIC_DATA_AUTHENTICATION_DATA_OBJECT_LIST_DDOL_9F49_KRN);
                byte[] ddolRelatedData;
                if (ddol == null)
                {
                    TLV unpred = database.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN);
                    unpred.Val.PackValue(unpred.Val.GetLength());
                    ddolRelatedData = unpred.Value;
                }
                else
                {
                    ddolRelatedData = CommonRoutines.PackRelatedDataTag(database, ddol);
                }
                EMVInternalAuthenticateRequest requestDDA = new EMVInternalAuthenticateRequest(ddolRelatedData);
                cardQManager.EnqueueToInput(new CardRequest(requestDDA, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE;
                #endregion
            }
            if (aip.Value.SDASupported && tc.Value.SDACapable)
            {
                string aid = emvSelectApplicationResponse.GetDFName();
                string rid = aid.Substring(0, 10);
                RIDEnum ridEnum = (RIDEnum)Enum.Parse(typeof(RIDEnum), rid);
                CAPublicKeyCertificate capk = database.PublicKeyCertificateManager.GetCAPK(ridEnum, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);

                TLV ssadTLV = database.Get(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN);
                if (capk == null || ssadTLV == null)
                {
                    tvr.Value.SDAFailed = true;
                    tvr.UpdateDB();
                }
                else
                {
                    TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
                    tsi.Value.OfflineDataAuthenticationWasPerformed = true;
                    tsi.UpdateDB();

                    byte[] sdadRaw = database.Get(EMVTagsEnum.SIGNED_STATIC_APPLICATION_DATA_93_KRN).Value;
                    byte[] authCode = VerifySAD.VerifySSAD(ICCDynamicDataType.DYNAMIC_NUMBER_ONLY, database, capk, sdadRaw);
                    if (authCode == null)
                    {
                        tvr.Value.SDAFailed = true;
                        tvr.UpdateDB();
                    }
                    else
                    {
                        TLV dataAuthenticationCode = database.Get(EMVTagsEnum.DATA_AUTHENTICATION_CODE_9F45_KRN);
                        if (dataAuthenticationCode == null)
                            dataAuthenticationCode = TLV.Create(EMVTagsEnum.DATA_AUTHENTICATION_CODE_9F45_KRN.Tag, authCode);
                        else
                            dataAuthenticationCode.Value = authCode;
                    }
                }

                //sda done after gen ac 1, call gen ac 1 now with no signature requested
                request = CreateGenAC(database, false);
                cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
                return SignalsEnum.WAITING_FOR_GEN_AC_1;
            }

            tvr.Value.OfflineDataAuthenticationWasNotPerformed = true;
            tvr.UpdateDB();
            //to do: ceck this, is this correct for a card where no oda is supported
            request = CreateGenAC(database, false);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            return SignalsEnum.WAITING_FOR_GEN_AC_1;
        }

        private static EMVGenerateACRequest CreateGenAC(KernelDatabaseBase database, bool cdaSigRequest)
        {
            TLV cdol1 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN);
            CommonRoutines.PackRelatedDataTag(database, EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2, cdol1);

            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpST = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcpST.Value.ACTypeEnum = GetDSACType(database);
            rcpST.Value.CDASignatureRequested = cdaSigRequest;
            rcpST.UpdateDB();

            EMVGenerateACRequest request = new EMVGenerateACRequest(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2), null, rcpST);
            return request;
        }
    }
}


