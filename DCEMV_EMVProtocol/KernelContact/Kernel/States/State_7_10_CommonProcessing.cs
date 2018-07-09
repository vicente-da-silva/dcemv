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

namespace DCEMV.EMVProtocol.Kernels.K
{
    public static class State_7_10_CommonProcessing
    {
        public static SignalsEnum DoInvalidResponsePart_C(KernelDatabase database, KernelQ qManager, L1Enum l1Enum, L2Enum l2Enum, L3Enum l3Enum)
        {
            CommonRoutines.UpdateUserInterfaceRequestData(database, KernelMessageidentifierEnum.ERROR_OTHER_CARD, KernelStatusEnum.NOT_READY);
            CommonRoutines.CreateEMVDiscretionaryData(database);
            CommonRoutines.CreateEMVDataRecord(database);

            return CommonRoutines.PostOutcome(database, qManager,
                KernelMessageidentifierEnum.N_A,
                KernelStatusEnum.N_A,
                null,
                Kernel2OutcomeStatusEnum.END_APPLICATION,
                Kernel2StartEnum.N_A,
                true,
                KernelMessageidentifierEnum.ERROR_OTHER_CARD,
                l1Enum,
                null,
                l2Enum,
                l3Enum);
        }

        public static bool DoCDA(KernelDatabase database, KernelQ qManager, CAPublicKeyCertificate capk, CardQ cardQManager, CardResponse cardResponse, EMVSelectApplicationResponse emvSelectApplicationResponse, bool isFirstGenAC)
        {
            //#region 9_10.2
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            TRANSACTION_STATUS_INFORMATION_9B_KRN tsi = new TRANSACTION_STATUS_INFORMATION_9B_KRN(database);
            tsi.Value.OfflineDataAuthenticationWasPerformed = true;
            tsi.UpdateDB();

            bool cdaSucceeded = VerifySDAD_CDA(database, capk, cardResponse, isFirstGenAC);

            if (!cdaSucceeded)
            {
                tvr.Value.CDAFailed = true;
                tvr.UpdateDB();
                return false;
            }
            else
            {
                return true;
            }
        }

        public static SignalsEnum EndOnTCorAAC(KernelDatabase database, KernelQ qManager, CardQ cardQManager)
        {
            CommonRoutines.CreateEMVDataRecord(database);

            Kernel2OutcomeStatusEnum k2OutcomeStatus = Kernel2OutcomeStatusEnum.N_A;
            Kernel2StartEnum k2StartStatus = Kernel2StartEnum.N_A;
            KernelStatusEnum k2Status = KernelStatusEnum.N_A;
            KernelMessageidentifierEnum k2MessageIdentifier = KernelMessageidentifierEnum.N_A;
            byte[] holdTime = new byte[] { 0x00, 0x00, 0x00 };
            ValueQualifierEnum valueQualifierEnum = ValueQualifierEnum.NONE;
            KernelCVMEnum cvmEnum = new OUTCOME_PARAMETER_SET_DF8129_KRN2(database).Value.CVM;
            byte[] currencyCode = new byte[2];
            byte[] valueQualifier = new byte[6];
            bool uiRequestOnOutcomePresent;

            string tt = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value);
            if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40)
            {
                k2OutcomeStatus = Kernel2OutcomeStatusEnum.APPROVED;
            }
            else
            {
                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80)
                {
                    throw new EMVProtocolException("Invalid state in EndOnTCorAAC");
                }
                else
                {
                    k2OutcomeStatus = Kernel2OutcomeStatusEnum.DECLINED;
                }
            }

            k2Status = KernelStatusEnum.NOT_READY;

            if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x40)
            {
                holdTime = database.GetDefault(EMVTagsEnum.MESSAGE_HOLD_TIME_DF812D_KRN2).Value;

                if (cvmEnum == KernelCVMEnum.OBTAIN_SIGNATURE)
                {
                    k2MessageIdentifier = KernelMessageidentifierEnum.APPROVED_SIGN;
                }
                else
                {
                    k2MessageIdentifier = KernelMessageidentifierEnum.APPROVED;
                }
            }
            else
            {
                if ((database.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN).Value[0] & 0xC0) == 0x80)
                {
                    throw new EMVProtocolException("Invalid state in EndOnTCorAAC");
                }
                else
                {
                    k2MessageIdentifier = KernelMessageidentifierEnum.DECLINED;
                }
            }

            CommonRoutines.CreateEMVDiscretionaryData(database);

            uiRequestOnOutcomePresent = true;

            return CommonRoutines.PostOutcome(database, qManager,
                k2MessageIdentifier,
                k2Status,
                holdTime,
                k2OutcomeStatus,
                k2StartStatus,
                uiRequestOnOutcomePresent,
                KernelMessageidentifierEnum.N_A,
                L1Enum.NOT_SET,
                null,
                L2Enum.NOT_SET,
                L3Enum.NOT_SET,
                valueQualifierEnum,
                valueQualifier,
                currencyCode,
                false,
                cvmEnum);
        }

        private static bool VerifySDAD_CDA(KernelDatabase database, CAPublicKeyCertificate capk, CardResponse cardResponse, bool isFirstGenAC)
        {
            ICCDynamicData iccdd = VerifySAD.VerifySDAD(ICCDynamicDataType.NO_IDS_OR_RRP, isFirstGenAC, database, database.StaticDataToBeAuthenticated, capk, cardResponse);
            if (iccdd == null) return false;

            VerifySAD.AddSDADDataToDatabase(database, iccdd);
            return true;
        }
    }
}
