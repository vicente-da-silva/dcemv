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
using DCEMV.ISO7816Protocol;
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_7_WaitingForInternalAuthenticate
    {
        public static SignalsEnum Execute(
            KernelDatabase database, 
            KernelQ qManager, 
            CardQ cardQManager,
            PublicKeyCertificateManager publicKeyCertificateManager,
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
                        throw new EMVProtocolException("Invalid Kernel1TerminalReaderServiceRequestEnum in State_7_WaitingForInternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), kernel1Request.KernelTerminalReaderServiceRequestEnum));
                }
            }
            else
            {
                CardResponse cardResponse = cardQManager.DequeueFromOutput(false);
                switch (cardResponse.CardInterfaceServiceResponseEnum)
                {
                    case CardInterfaceServiceResponseEnum.RA:
                        return EntryPointRA(database, cardResponse, qManager, cardQManager, sw, publicKeyCertificateManager, emvSelectApplicationResponse);

                    case CardInterfaceServiceResponseEnum.L1RSP:
                        return EntryPointL1RSP(database, cardResponse, qManager);

                    default:
                        throw new EMVProtocolException("Invalid Kernel1CardinterfaceServiceResponseEnum in State_7_WaitingForInternalAuthenticate:" + Enum.GetName(typeof(CardInterfaceServiceResponseEnum), cardResponse.CardInterfaceServiceResponseEnum));
                }
            }
        }

        private static SignalsEnum EntryPointRA(KernelDatabase database, CardResponse cardResponse, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager publicKeyCertificateManager, EMVSelectApplicationResponse emvSelectApplicationResponse)
        {
            if (!cardResponse.ApduResponse.Succeeded)
            {
                return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
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

            EMVInternalAuthenticateResponse response = cardResponse.ApduResponse as EMVInternalAuthenticateResponse;
          
            string aid = emvSelectApplicationResponse.GetDFName();
            string rid = aid.Substring(0, 10);
            RIDEnum ridEnum = (RIDEnum)Enum.Parse(typeof(RIDEnum), rid);
            CAPublicKeyCertificate capk = database.PublicKeyCertificateManager.GetCAPK(ridEnum, database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN).Value[0]);

            #region 3.8.1.1
            bool ddaPassed = DoDDA(database, qManager, capk, response.GetTLVSignedApplicationData());
            #endregion
            if (!ddaPassed)
            {
                TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                tvr.Value.DDAFailed = true;
                tvr.UpdateDB();
            }
            
            TLV cdol1 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN);
            CommonRoutines.PackRelatedDataTag(database, EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2, cdol1);

            REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 rcpST = new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(database);
            rcpST.Value.ACTypeEnum = database.ACType.Value.DSACTypeEnum;
            rcpST.Value.CDASignatureRequested = false;
            rcpST.UpdateDB();

            EMVGenerateACRequest request = new EMVGenerateACRequest(database.Get(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2), null, rcpST);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));

            return SignalsEnum.WAITING_FOR_GEN_AC_1;
        }

        private static SignalsEnum EntryPointL1RSP(KernelDatabase database, CardResponse cardResponse, KernelQ qManager)
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
                L2Enum.STATUS_BYTES,
                L3Enum.NOT_SET);
        }
        
        private static SignalsEnum EntryPointDET(KernelDatabase database, KernelRequest kernel1Request)
        {
            database.UpdateWithDETData(kernel1Request.InputData);
            return SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE;
        }
        private static SignalsEnum EntryPointSTOP(KernelDatabase database, KernelQ qManager)
        {
            CommonRoutines.CreateEMVDiscretionaryData(database);
            return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.END_APPLICATION, Kernel2StartEnum.N_A, L1Enum.NOT_SET, L2Enum.NOT_SET, L3Enum.STOP);
        }

        
        public static bool DoDDA(KernelDatabaseBase database, KernelQ qManager, CAPublicKeyCertificate capk, TLV sdadTLV)
        {
            try
            {
                TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
                tsi.Value.OfflineDataAuthenticationWasPerformed = true;
                tsi.UpdateDB();

                if (database.Get(EMVTagsEnum.CERTIFICATION_AUTHORITY_PUBLIC_KEY_INDEX_8F_KRN) == null)
                    return false;
                
                if (capk == null)
                    return false;

                TLV aip = database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag);
                int length = database.StaticDataToBeAuthenticated.Serialize().Length;
                if (aip != null && database.IsNotEmpty(EMVTagsEnum.STATIC_DATA_AUTHENTICATION_SDA_TAG_LIST_9F4A_KRN3.Tag))
                    if (2048 - length >= aip.Value.Length)
                        database.StaticDataToBeAuthenticated.AddToList(database.Get(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN));
                    else
                        return false;

                if (sdadTLV != null)
                {
                    ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.DYNAMIC_NUMBER_ONLY, database, capk, sdadTLV.Value);
                    if (iccdd == null) return false;

                    VerifySAD.AddSDADDataToDatabase(database, iccdd);
                    return true;
                }
                
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}
