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
using DCEMV.Shared;
using DCEMV.FormattingUtils;
using System;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum CVMListIterationReturn
    {
        ConditionsMet,
        ConditionNotMet,
        Error,
    }
    public enum KernelCVMEnum
    {
        NO_CVM = 0x00,
        OBTAIN_SIGNATURE = 0x10,
        ONLINE_PIN = 0x20,
        CONFIRMATION_CODE_VERIFIED = 0x30,
        OFFLINE_PIN,
        COMBO,
        N_A = 0xF0,
    }
    public static class CVMSelection_7_5
    {
        public static Logger Logger = new Logger(typeof(CVMSelection_7_5));
        
        public static void CVMSelection(KernelDatabaseBase database, Func<bool> kernelSupportedCallback)
        {
            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvmResults = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);

            //#region support for Refunds pg 177
            //byte transactionType = database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0];
            //if (transactionType == (byte)TransactionTypeEnum.Refund)
            //{
            //    Do_CVM_14(database, cvmResults);
            //    return;
            //}
            //#endregion

            APPLICATION_INTERCHANGE_PROFILE_82_KRN aip = new APPLICATION_INTERCHANGE_PROFILE_82_KRN(database);
            
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);

            KernelCVMEnum cvmEnum = KernelCVMEnum.N_A;

            CardHolderVerificationRule cvrCurrentlySelected;
            
            if (aip.Value.OnDeviceCardholderVerificationIsSupported && kernelSupportedCallback.Invoke())
            {
                #region cvm.2
                long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);
                long rcvml = Formatting.BcdToLong(database.Get(EMVTagsEnum.READER_CVM_REQUIRED_LIMIT_DF8126_KRN2).Value);
                if (aa > rcvml)
                #endregion
                {
                    #region cvm.4
                    cvmEnum = KernelCVMEnum.CONFIRMATION_CODE_VERIFIED;
                    cvmResults.Value.CVMPerformed = 0x01;//on-device cardholder verification performed
                    cvmResults.Value.CVMCondition = 0x00;
                    cvmResults.Value.CVMResult = 0x02;//successful
                    CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
                    cvmResults.UpdateDB();
                    return;
                    #endregion
                }
                else
                {
                    #region cvm.3
                    cvmEnum = KernelCVMEnum.NO_CVM;
                    cvmResults.Value.CVMPerformed = 0x3F; //No CVM performed
                    cvmResults.Value.CVMCondition = 0x00;
                    cvmResults.Value.CVMResult = 0x02;//successful
                    CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
                    cvmResults.UpdateDB();
                    return;
                    #endregion
                }
            }

            #region cvm.5
            if (!aip.Value.CardholderVerificationIsSupported)
            #endregion
            {
                #region cvm.6
                cvmEnum = KernelCVMEnum.NO_CVM;
                cvmResults.Value.CVMPerformed = 0x3F; //No CVM performed
                cvmResults.Value.CVMCondition = 0x00;
                cvmResults.Value.CVMResult = 0x00;//unknown
                CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
                cvmResults.UpdateDB();
                return;
                #endregion
            }

            #region cvm.7
            if (database.IsNotPresent(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN.Tag) ||
                    database.IsEmpty(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN.Tag))
            #endregion
            {
                #region cvm.8
                cvmEnum = KernelCVMEnum.NO_CVM;
                cvmResults.Value.CVMPerformed = 0x3F; //No CVM performed
                cvmResults.Value.CVMCondition = 0x00;
                cvmResults.Value.CVMResult = 0x00;//unknown
                CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
                cvmResults.UpdateDB();
                tvr.Value.ICCDataMissing = true;
                tvr.UpdateDB();
                return;
                #endregion
            }

            #region cvm.9
            CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN cvl = new CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN(database);
            #endregion

            BackToIterate:
            CVMListIterationReturn result;
            do
            {
                cvrCurrentlySelected = cvl.Value.CardHolderVerificationRules[database.CVMCurrentlySelectedCounter];
                #region cvm.10 cvm.11
                result = IterateCVM(database, cvrCurrentlySelected, cvl.Value.AmountX, cvl.Value.AmountY);
                #endregion
                database.CVMCurrentlySelectedCounter++;
                #region cvm.12 cvm.13
            } while (database.CVMCurrentlySelectedCounter < cvl.Value.CardHolderVerificationRules.Count &&
                result != CVMListIterationReturn.ConditionsMet);
            #endregion

            if (result != CVMListIterationReturn.ConditionsMet)
            {
                Do_CVM_14(database, cvmResults);
                return;
            }

            #region cvm.15
            if (!cvrCurrentlySelected.IsCVMRecognised())
            #endregion
            {
                #region cvm.16
                tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
                tvr.Value.UnrecognisedCVM = true;
                tvr.UpdateDB();
                #endregion
            }
            else
            {
                #region cvm.17
                if (cvrCurrentlySelected.IsSupported(database) && cvrCurrentlySelected.GetCVMCode() != (byte)CVMCode.FailCVMProcessing)
                #endregion
                {
                    #region cvm.18 and from EMV Book 3 Section 10.5
                    switch ((byte)(cvrCurrentlySelected.CVMRule & 0x3F))
                    {
                        //Pin processing (U,X,Z) continues in PinProcessing Procedure after CVM Selection
                        case (byte)CVMCode.EncipheredPINVerifiedOnline:
                            cvmEnum = KernelCVMEnum.ONLINE_PIN;
                            cvmResults.Value.CVMResult = 0x00; //unknown
                            tvr.Value.OnlinePINEntered = true;
                            break;

                        case (byte)CVMCode.EncipheredPINVerificationPerformedByICC:
                        case (byte)CVMCode.PlaintextPINVerificationPerformedByICC:
                            cvmEnum = KernelCVMEnum.OFFLINE_PIN;
                            cvmResults.Value.CVMResult = 0x00; //unknown
                            break;

                        case (byte)CVMCode.EncipheredPINVerificationPerformedByICCAndSignature_Paper:
                        case (byte)CVMCode.PlaintextPINVerificationPerformedByICCAndSignature_Paper:
                            cvmEnum = KernelCVMEnum.COMBO;
                            cvmResults.Value.CVMResult = 0x00; //unknown
                            break;

                        //V in diagram
                        case (byte)CVMCode.Signature_Paper:
                            cvmEnum = KernelCVMEnum.OBTAIN_SIGNATURE;
                            cvmResults.Value.CVMResult = 0x00; //unknown
                            CommonRoutines.UpdateOutcomeParameterSet(database, true);
                            break;

                        //W in diagram
                        case (byte)CVMCode.NoCVMRequired:
                            cvmEnum = KernelCVMEnum.NO_CVM;
                            cvmResults.Value.CVMResult = 0x02; //successful
                            break;

                        case (byte)CVMCode.RFU:
                            cvmEnum = KernelCVMEnum.N_A;
                            tvr.Value.UnrecognisedCVM = true;
                            cvmResults.Value.CVMResult = 0x01; //failed
                            break;

                        default:
                            goto BackToIterate;
                    }
                    #endregion
                    cvmResults.Value.CVMPerformed = cvrCurrentlySelected.CVMRule;
                    cvmResults.Value.CVMCondition = (byte)cvrCurrentlySelected.CVMConditionCode;
                    cvmResults.UpdateDB();
                    int depth = 0;
                    Logger.Log(cvmResults.ToPrintString(ref depth));
                    CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
                    tvr.UpdateDB();
                    return;
                }
            }

            #region cvm.19
            if (((byte)cvrCurrentlySelected.CVMConditionCode & 0x40) == (byte)CVMFailureCondition.ApplySucceedingCVRuleIfThisCVMIsUnsuccessful)
            #endregion
            {
                #region cvm.20
                if (database.CVMCurrentlySelectedCounter < cvl.Value.CardHolderVerificationRules.Count)
                #endregion
                {
                    #region cvm.10
                    goto BackToIterate;
                    #endregion
                }
                else
                {
                    Do22_25(database, cvrCurrentlySelected);
                    return;
                }
            }
            else
            {
                Do22_25(database, cvrCurrentlySelected);
                return;
            }
        }

        private static void Do22_25(KernelDatabaseBase database, CardHolderVerificationRule cvrCurrentlySelected)
        {
            CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr = new CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN(database);
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            KernelCVMEnum cvmEnum = KernelCVMEnum.N_A;

            #region cvm.22
            cvmEnum = KernelCVMEnum.NO_CVM;
            tvr.Value.CardholderVerificationWasNotSuccessful = true;
            CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
            tvr.UpdateDB();
            #endregion

            #region cvm.23
            if (((byte)cvrCurrentlySelected.CVMConditionCode & 0x3F) == 0x00)
            #endregion
            {
                #region cvm.24
                cvr.Value.CVMPerformed = cvrCurrentlySelected.CVMRule;
                cvr.Value.CVMCondition = (byte)cvrCurrentlySelected.CVMConditionCode;
                cvr.Value.CVMResult = 0x01;//failed
                #endregion
            }
            else
            {
                #region cvm.25
                cvr.Value.CVMPerformed = 0x3F;
                cvr.Value.CVMCondition = 0x00;
                cvr.Value.CVMResult = 0x01;//failed
                #endregion
            }
            cvr.UpdateDB();
        }
        private static CVMListIterationReturn IterateCVM(KernelDatabaseBase database, CardHolderVerificationRule cvrCurrentlySelected, long amountX, long amountY)
        {
            #region cvm.10
            if (cvrCurrentlySelected.IsConditionCodeUnderstood())
            #endregion
            {
                #region cvm.11
                if (cvrCurrentlySelected.AllDataRequiredIsAvailable(database))
                #endregion
                {
                    #region cvm.12
                    if (cvrCurrentlySelected.ConditionsSatisfied(database, amountX, amountY))
                    #endregion
                    {
                        #region cvm.15
                        return CVMListIterationReturn.ConditionsMet;
                        #endregion
                    }
                    else
                    {
                        return CVMListIterationReturn.ConditionNotMet;
                    }
                }
                else
                {
                    return CVMListIterationReturn.Error;
                }

            }
            else
            {
                return CVMListIterationReturn.Error;
            }
        }
        private static void Do_CVM_14(KernelDatabaseBase database, CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr)
        {
            KernelCVMEnum cvmEnum = KernelCVMEnum.N_A;

            #region cvm.14
            cvmEnum = KernelCVMEnum.NO_CVM;
            cvr.Value.CVMPerformed = 0x3F; //No CVM performed
            cvr.Value.CVMCondition = 0x00;
            cvr.Value.CVMResult = 0x01;//failed
            CommonRoutines.UpdateOutcomeParameterSet(database, cvmEnum);
            cvr.UpdateDB();
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);
            tvr.Value.CardholderVerificationWasNotSuccessful = true;
            tvr.UpdateDB();
            #endregion
        }
    }
}
