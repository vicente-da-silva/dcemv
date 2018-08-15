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
using System;
using System.Diagnostics;
using DCEMV.TLVProtocol;
using DCEMV.ISO7816Protocol;

namespace DCEMV.EMVProtocol.Kernels.K1
{
    public static class State_3_4_CommonProcessing
    {
        public static SignalsEnum DoCommonProcessing(string source, KernelDatabaseBase databaseIn, KernelQ qManager, CardQ cardQManager, Stopwatch sw, PublicKeyCertificateManager pkcm, CardExceptionManager cardExceptionManager)
        {
            Kernel1Database database = (Kernel1Database)databaseIn;
            if (database.NextCommandEnum == NextCommandEnum.READ_RECORD)
            {
                DoDEKIfNeeded(database, qManager);
                return SignalsEnum.WAITING_FOR_EMV_READ_RECORD_RESPONSE;
            }

            DoDEKIfNeeded(database, qManager);

            //ReaderContactlessTransactionLimit exceeded
            if (database.ProcessingIndicatorsForSelected.ContactlessApplicationNotAllowed)
            {
                CommonRoutines.CreateEMVDiscretionaryData(database);
                return CommonRoutines.PostOutcomeWithError(database, qManager, Kernel2OutcomeStatusEnum.SELECT_NEXT, Kernel2StartEnum.C, L1Enum.NOT_SET, L2Enum.MAX_LIMIT_EXCEEDED, L3Enum.NOT_SET);
            }

            bool goOnline = false;

            #region 3.2.1.1
            if (database.IsPresent(EMVTagsEnum.VLP_ISSUER_AUTHORISATION_CODE_9F74_KRN1.Tag))
            {
                if (database.ProcessingIndicatorsForSelected.ReaderContactlessFloorLimitExceeded)
                {
                    goOnline = true;
                }
                if (database.ProcessingIndicatorsForSelected.ReaderCVMRequiredLimitExceeded)
                {
                    goOnline = true;
                }
            }
            else
            {
                goOnline = true;
            }
            #endregion

            if (goOnline)
            {
                return DoOnlineProcess(database, cardQManager);
            }
            else
            {
                return DoOfflineProcess(database, cardQManager);
            }
        }

        public static SignalsEnum DoOfflineProcess(KernelDatabaseBase database, CardQ cardQManager)
        {
            #region 3.4.1.1 and 3.4.1.2
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
            EMVInternalAuthenticateRequest request = new EMVInternalAuthenticateRequest(ddolRelatedData);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            return SignalsEnum.WAITING_FOR_INTERNAL_AUTHENTICATE;
            #endregion
        }
        public static SignalsEnum DoOnlineProcess(KernelDatabaseBase database, CardQ cardQManager)
        {
            #region 3.5.1.1
            TLV tvr = database.Get(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN);
            if(tvr == null)
            {
                tvr = TLV.Create(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.Tag, new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00});
                database.AddToList(tvr);
            }
            TLV cdol1 = database.Get(EMVTagsEnum.CARD_RISK_MANAGEMENT_DATA_OBJECT_LIST_1_CDOL1_8C_KRN);
            byte[] cdol1RelatedData = CommonRoutines.PackRelatedDataTag(database, cdol1);

            EMVGenerateACRequest request = new EMVGenerateACRequest(cdol1RelatedData);
            cardQManager.EnqueueToInput(new CardRequest(request, CardinterfaceServiceRequestEnum.ADPU));
            return SignalsEnum.WAITING_FOR_GEN_AC_1;
            #endregion
        }

        private static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
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
