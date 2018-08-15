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
using System;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class ProcessingRestrictions_7_7
    {
        public static void ProcessingRestrictions(KernelDatabaseBase database)
        {
            TERMINAL_VERIFICATION_RESULTS_95_KRN tvr = new TERMINAL_VERIFICATION_RESULTS_95_KRN(database);

            if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_VERSION_NUMBER_CARD_9F08_KRN.Tag))
            {
                #region pre.2
                string pvnCard = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_VERSION_NUMBER_CARD_9F08_KRN).Value);
                string pvnTerm = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_VERSION_NUMBER_TERMINAL_9F09_KRN).Value);
                if (pvnCard != pvnTerm)
                #endregion
                {
                    #region pre.3
                    tvr.Value.ICCAndTerminalHaveDifferentApplicationVersions = true;
                    tvr.UpdateDB();
                    #endregion
                }
            }

            DateTime transactionDate = EMVTagsEnum.TRANSACTION_DATE_9A_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.TRANSACTION_DATE_9A_KRN).Value);
            DateTime appExpiryDate = EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.APPLICATION_EXPIRATION_DATE_5F24_KRN).Value);

            #region pre.4
            if (database.IsNotEmpty(EMVTagsEnum.APPLICATION_EFFECTIVE_DATE_5F25_KRN.Tag))
            #endregion
            {
                DateTime appEffectiveDate = EMVTagsEnum.APPLICATION_EFFECTIVE_DATE_5F25_KRN.FormatAsDateTime(database.Get(EMVTagsEnum.APPLICATION_EFFECTIVE_DATE_5F25_KRN).Value);
                #region pre.5
                if (transactionDate < appEffectiveDate)
                #endregion
                {
                    #region pre.6
                    tvr.Value.ApplicationNotYetEffective = true;
                    tvr.UpdateDB();
                    #endregion
                }
            }

            #region pre.7
            if (transactionDate > appExpiryDate)
            #endregion
            {
                #region pre.8

                tvr.Value.ExpiredApplication = true;
                tvr.UpdateDB();
                #endregion
            }

            #region pre.9
            if (database.IsEmpty(EMVTagsEnum.APPLICATION_USAGE_CONTROL_9F07_KRN.Tag))
                return;
            #endregion

            #region pre.10
            TERMINAL_TYPE_9F35_KRN tt = new TERMINAL_TYPE_9F35_KRN(database);
            ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN atc = new ADDITIONAL_TERMINAL_CAPABILITIES_9F40_KRN(database);
            APPLICATION_USAGE_CONTROL_9F07_KRN auc = new APPLICATION_USAGE_CONTROL_9F07_KRN(database);

            if ((tt.Value.TerminalType.Code == 0x14 || tt.Value.TerminalType.Code == 0x15 || tt.Value.TerminalType.Code == 0x16)
                && atc.Value.IsCash)
            #endregion
            {
                #region pre.12
                if (!auc.Value.IsValidAtATMs)
                #endregion
                {
                    #region pre.13
                    tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                    tvr.UpdateDB();
                    return;
                    #endregion
                }
            }
            else
            {
                #region pre.11
                if (!auc.Value.IsValidAtTerminalsOtherThanATMs)
                #endregion
                {
                    #region pre.13
                    tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                    tvr.UpdateDB();
                    return;
                    #endregion
                }
            }

            #region pre.14
            if (database.IsEmpty(EMVTagsEnum.ISSUER_COUNTRY_CODE_5F28_KRN.Tag))
            {
                tvr.UpdateDB();
                return;
            }
            #endregion

            string tcc = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN).Value);
            string icc = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.ISSUER_COUNTRY_CODE_5F28_KRN).Value);

            byte transactionType = database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0];
            

            #region pre.15
            if (transactionType == (byte)TransactionTypeEnum.CashWithdrawal ||
                transactionType == (byte)TransactionTypeEnum.CashDisbursement)
            #endregion
            {
                #region pre.16
                if (tcc == icc)
                #endregion
                {
                    #region pre.17
                    if (!auc.Value.IsValidForDomesticCashTransactions)
                    #endregion
                    {
                        #region pre.19
                        tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                        #endregion
                    }
                }
                else
                {
                    #region pre.18
                    if (!auc.Value.IsValidForInternationalCashTransactions)
                    #endregion
                    {
                        #region pre.19
                        tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                        #endregion
                    }
                }
            }

            #region pre.20
            if (transactionType == (byte)TransactionTypeEnum.PurchaseGoodsAndServices ||
                transactionType == (byte)TransactionTypeEnum.PurchaseWithCashback)
            #endregion
            {
                #region pre.21
                if (tcc == icc)
                #endregion
                {
                    #region pre.22
                    if (!(auc.Value.IsValidForDomesticGoods || auc.Value.IsValidForDomesticServices))
                    #endregion
                    {
                        #region pre.24
                        tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                        #endregion

                    }
                }
                else
                {
                    #region pre.23
                    if (!(auc.Value.IsValidForInternationalGoods || auc.Value.IsValidForInternationalServices))
                    #endregion
                    {
                        #region pre.24
                        tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
                        #endregion

                    }
                }
            }

            #region pre.25
            if (database.IsNotPresent(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN.Tag))
            {
                tvr.UpdateDB();
                return;
            }

            if (Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN).Value) == 0)
            {
                tvr.UpdateDB();
                return;
            }
            #endregion

            #region pre.26
            if (tcc == icc)
            #endregion
            {
                #region pre.27
                if (auc.Value.IsDomesticCashbackAllowed)
                {
                    tvr.UpdateDB();
                    return;
                }
                #endregion
            }
            else
            {
                #region pre.28
                if (auc.Value.IsInternationalCashbackAllowed)
                {
                    tvr.UpdateDB();
                    return;
                }
                #endregion
            }

            #region pre.29
            tvr.Value.RequestedServiceNotAllowedForCardProduct = true;
            tvr.UpdateDB();
            #endregion
        }
    }
}
