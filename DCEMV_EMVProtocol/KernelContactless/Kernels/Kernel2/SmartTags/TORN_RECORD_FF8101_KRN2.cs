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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class TORN_RECORD_FF8101_KRN2 : SmartTag
    {
        public TORN_RECORD_FF8101_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TORN_RECORD_FF8101_KRN2, new SmartValue(EMVTagsEnum.TORN_RECORD_FF8101_KRN2.DataFormatter))
        {
        }

        public void AddTornTransactionLog(KernelDatabaseBase database)
        {
            AddTTL(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN, database);
            AddTTL(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN, database);
            AddTTL(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN, database);
            AddTTL(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN, database);
            AddTTL(EMVTagsEnum.BALANCE_READ_BEFORE_GEN_AC_DF8104_KRN2, database);
            AddTTL(EMVTagsEnum.CDOL1_RELATED_DATA_DF8107_KRN2, database);
            AddTTL(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN, database);
            AddTTL(EMVTagsEnum.DRDOL_RELATED_DATA_DF8113_KRN2, database);
            AddTTL(EMVTagsEnum.DS_SUMMARY_1_9F7D_KRN2, database);
            AddTTL(EMVTagsEnum.IDS_STATUS_DF8128_KRN2, database);
            AddTTL(EMVTagsEnum.INTERFACE_DEVICE_IFD_SERIAL_NUMBER_9F1E_KRN, database);
            AddTTL(EMVTagsEnum.PDOL_RELATED_DATA_DF8111_KRN2, database);
            AddTTL(EMVTagsEnum.REFERENCE_CONTROL_PARAMETER_DF8114_KRN2, database);
            AddTTL(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN, database);
            AddTTL(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN, database);
            AddTTL(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN, database);
            AddTTL(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN, database);
            AddTTL(EMVTagsEnum.TRANSACTION_CATEGORY_CODE_9F53_KRN2, database);
            AddTTL(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN, database);
            AddTTL(EMVTagsEnum.TRANSACTION_DATE_9A_KRN, database);
            AddTTL(EMVTagsEnum.TRANSACTION_TIME_9F21_KRN, database);
            AddTTL(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN, database);
            AddTTL(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN, database);
            AddTTL(EMVTagsEnum.TERMINAL_RELAY_RESISTANCE_ENTROPY_DF8301_KRN2, database);
            AddTTL(EMVTagsEnum.DEVICE_RELAY_RESISTANCE_ENTROPY_DF8302_KRN2, database);
            AddTTL(EMVTagsEnum.MIN_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8303_KRN2, database);
            AddTTL(EMVTagsEnum.MAX_TIME_FOR_PROCESSING_RELAY_RESISTANCE_APDU_DF8304_KRN2, database);
            AddTTL(EMVTagsEnum.DEVICE_ESTIMATED_TRANSMISSION_TIME_FOR_RELAY_RESISTANCE_RAPDU_DF8305_KRN2, database);
            AddTTL(EMVTagsEnum.MEASURED_RELAY_RESISTANCE_PROCESSING_TIME_DF8306_KRN2, database);
            AddTTL(EMVTagsEnum.RRP_COUNTER_DF8307_KRN2, database);
        }

        private void AddTTL(EMVTagMeta tag, KernelDatabaseBase database)
        {
            if (database.IsNotEmpty(tag.Tag))
            {
                Children.AddToList(TLV.Create(tag.Tag, database.Get(tag).Value));
            }
        }
    }
}
