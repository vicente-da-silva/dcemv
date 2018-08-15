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
using DataFormatters;
using DCEMV.FormattingUtils;
using DCEMV_QRDEProtocol;
using System;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.EMVQRCode
{
    public class EMVQRMetaDataSource : IQRMetaDataSource
    {
        public DataFormatterBase GetFormatter(TagId tagLabel, TagId tagLabelParent)
        {
            return EMVQRTagsEnum.GetFormatter(tagLabel, tagLabelParent);
        }

        public bool IsTemplate(TagId tagLabel, TagId tagLabelParent)
        {
            return EMVQRTagsEnum.IsTemplate(tagLabel, tagLabelParent);
        }

        public string GetName(TagId tagLabel, TagId tagLabelParent)
        {
            string meta = EMVQRTagsEnum.GetName(tagLabel, tagLabelParent);
            if (meta == null)
                return null;
            else
                return meta;
        }
    }
    public class EMVQRTagsEnum
    {
        private static List<EMVQRTagMeta> EnumList = new List<EMVQRTagMeta>();

        public static EMVQRTagMeta PAYLOAD_FORMAT_INDICATOR_00 = new EMVQRTagMeta(TagId._00, "Payload Format Indicator", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");
        public static EMVQRTagMeta POINT_OF_INITIATION_METHOD_01 = new EMVQRTagMeta(TagId._01, "Point of Initiation Method", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");

        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_02 = new EMVQRTagMeta(TagId._02, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_03 = new EMVQRTagMeta(TagId._03, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_04 = new EMVQRTagMeta(TagId._04, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_05 = new EMVQRTagMeta(TagId._05, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_06 = new EMVQRTagMeta(TagId._06, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_07 = new EMVQRTagMeta(TagId._07, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_08 = new EMVQRTagMeta(TagId._08, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_09 = new EMVQRTagMeta(TagId._09, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_10 = new EMVQRTagMeta(TagId._10, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_11 = new EMVQRTagMeta(TagId._11, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_12 = new EMVQRTagMeta(TagId._12, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_13 = new EMVQRTagMeta(TagId._13, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_14 = new EMVQRTagMeta(TagId._14, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_15 = new EMVQRTagMeta(TagId._15, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_16 = new EMVQRTagMeta(TagId._16, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_17 = new EMVQRTagMeta(TagId._17, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_18 = new EMVQRTagMeta(TagId._18, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_19 = new EMVQRTagMeta(TagId._19, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_20 = new EMVQRTagMeta(TagId._20, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_21 = new EMVQRTagMeta(TagId._21, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_22 = new EMVQRTagMeta(TagId._22, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_23 = new EMVQRTagMeta(TagId._23, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_24 = new EMVQRTagMeta(TagId._24, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_25 = new EMVQRTagMeta(TagId._25, "Merchant Account Information Primitive", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");

        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_26 = new EMVQRTagMeta(TagId._26, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_27 = new EMVQRTagMeta(TagId._27, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_28 = new EMVQRTagMeta(TagId._28, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_29 = new EMVQRTagMeta(TagId._29, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_30 = new EMVQRTagMeta(TagId._30, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_31 = new EMVQRTagMeta(TagId._31, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_32 = new EMVQRTagMeta(TagId._32, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_33 = new EMVQRTagMeta(TagId._33, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_34 = new EMVQRTagMeta(TagId._34, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_35 = new EMVQRTagMeta(TagId._35, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_36 = new EMVQRTagMeta(TagId._36, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_37 = new EMVQRTagMeta(TagId._37, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_38 = new EMVQRTagMeta(TagId._38, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_39 = new EMVQRTagMeta(TagId._39, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_40 = new EMVQRTagMeta(TagId._40, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_41 = new EMVQRTagMeta(TagId._41, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_42 = new EMVQRTagMeta(TagId._42, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_43 = new EMVQRTagMeta(TagId._43, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_44 = new EMVQRTagMeta(TagId._44, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_45 = new EMVQRTagMeta(TagId._45, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_46 = new EMVQRTagMeta(TagId._46, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_47 = new EMVQRTagMeta(TagId._47, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_48 = new EMVQRTagMeta(TagId._48, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_49 = new EMVQRTagMeta(TagId._49, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_50 = new EMVQRTagMeta(TagId._50, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_51 = new EMVQRTagMeta(TagId._51, "Merchant Account Information Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);

        public static EMVQRTagMeta MERCHANT_CATEGORY_CODE_52 = new EMVQRTagMeta(TagId._52, "Merchant Category Code", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 4), "");
        public static EMVQRTagMeta TRANSACTION_CURRENCY_53 = new EMVQRTagMeta(TagId._53, "Transaction Currency", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 3), "");
        public static EMVQRTagMeta TRANSACTION_AMOUNT_54 = new EMVQRTagMeta(TagId._54, "Transaction Amount", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 13), "");
        public static EMVQRTagMeta TIP_OR_CONVENIENCE_INDICATOR_55 = new EMVQRTagMeta(TagId._55, "Tip or Convenience Indicator", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");
        public static EMVQRTagMeta VALUE_OF_CONVENIENCE_FEE_FIXED_56 = new EMVQRTagMeta(TagId._56, "Value of Convenience Fee Fixed", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 5), "");
        public static EMVQRTagMeta VALUE_OF_CONVENIENCE_FEE_PERCENTAGE_57 = new EMVQRTagMeta(TagId._57, "Value of Convenience Fee Percentage", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 13), "");
        public static EMVQRTagMeta COUNTRY_CODE_58 = new EMVQRTagMeta(TagId._58, "Country Code", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");
        public static EMVQRTagMeta MERCHANT_NAME_59 = new EMVQRTagMeta(TagId._59, "Merchant Name", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta MERCHANT_CITY_60 = new EMVQRTagMeta(TagId._60, "Merchant City", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 15), "");
        public static EMVQRTagMeta POSTAL_CODE_61 = new EMVQRTagMeta(TagId._61, "Postal Code", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 10), "");
        public static EMVQRTagMeta ADDITIONAL_DATA_FIELD_TEMPLATE_62 = new EMVQRTagMeta(TagId._62, "Additional Data Field Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta CRC_63 = new EMVQRTagMeta(TagId._63, "CRC", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 4), "");
        public static EMVQRTagMeta MERCHANT_INFORMATION_LANGUAGE_TEMPLATE_64 = new EMVQRTagMeta(TagId._64, "Merchant Information—Language Template", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);

        //65-79 public static EMVQRTagMeta RFU_FOR_EMVCO_ = new EMVQRTagMeta(TagId._00, "RFU for EMVCo", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");

        public static EMVQRTagMeta UNRESERVED_TEMPLATES_80 = new EMVQRTagMeta(TagId._80, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_81 = new EMVQRTagMeta(TagId._81, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_82 = new EMVQRTagMeta(TagId._82, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_83 = new EMVQRTagMeta(TagId._83, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_84 = new EMVQRTagMeta(TagId._84, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_85 = new EMVQRTagMeta(TagId._85, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_xx = new EMVQRTagMeta(TagId._86, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_87 = new EMVQRTagMeta(TagId._87, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_88 = new EMVQRTagMeta(TagId._88, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_89 = new EMVQRTagMeta(TagId._89, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_90 = new EMVQRTagMeta(TagId._90, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_91 = new EMVQRTagMeta(TagId._91, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_92 = new EMVQRTagMeta(TagId._92, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_93 = new EMVQRTagMeta(TagId._93, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_94 = new EMVQRTagMeta(TagId._94, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_95 = new EMVQRTagMeta(TagId._95, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_96 = new EMVQRTagMeta(TagId._96, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_97 = new EMVQRTagMeta(TagId._97, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_98 = new EMVQRTagMeta(TagId._98, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta UNRESERVED_TEMPLATES_99 = new EMVQRTagMeta(TagId._99, "Unreserved Templates", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);

        public static EMVQRTagMeta BILL_NUMBER_01 = new EMVQRTagMeta(TagId._01, "Bill Number", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta MOBILE_NUMBER_02 = new EMVQRTagMeta(TagId._02, "Mobile Number", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta STORE_LABEL_03 = new EMVQRTagMeta(TagId._03, "Store Label", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta LOYALTY_NUMBER_04 = new EMVQRTagMeta(TagId._04, "Loyalty Number", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta REFERENCE_LABEL_05 = new EMVQRTagMeta(TagId._05, "Reference Label", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta CUSTOMER_LABEL_06 = new EMVQRTagMeta(TagId._06, "Customer Label", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta TERMINAL_LABEL_07 = new EMVQRTagMeta(TagId._07, "Terminal Label", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta PURPOSE_OF_TRANSACTION_08 = new EMVQRTagMeta(TagId._08, "Purpose of Transaction", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 25), "");
        public static EMVQRTagMeta ADDITIONAL_CONSUMER_DATA_REQUEST_09 = new EMVQRTagMeta(TagId._09, "Additional Consumer Data Request", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 3), "");

        //10-49 public static EMVQRTagMeta RFU_FOR_EMVCO_ = new EMVQRTagMeta(TagId._00, "RFU for EMVCo", new List<EMVQRTagMeta>(), new DataFormatterLengthFixed(DataFormats._NUMERIC, 2), "");

        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_50 = new EMVQRTagMeta(TagId._50, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_51 = new EMVQRTagMeta(TagId._51, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_52 = new EMVQRTagMeta(TagId._52, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_53 = new EMVQRTagMeta(TagId._53, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_54 = new EMVQRTagMeta(TagId._54, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_55 = new EMVQRTagMeta(TagId._55, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_56 = new EMVQRTagMeta(TagId._56, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_57 = new EMVQRTagMeta(TagId._57, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_58 = new EMVQRTagMeta(TagId._58, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_59 = new EMVQRTagMeta(TagId._59, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_60 = new EMVQRTagMeta(TagId._60, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_61 = new EMVQRTagMeta(TagId._61, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_62 = new EMVQRTagMeta(TagId._62, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_63 = new EMVQRTagMeta(TagId._63, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_64 = new EMVQRTagMeta(TagId._64, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_65 = new EMVQRTagMeta(TagId._65, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_66 = new EMVQRTagMeta(TagId._66, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_67 = new EMVQRTagMeta(TagId._67, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_68 = new EMVQRTagMeta(TagId._68, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_69 = new EMVQRTagMeta(TagId._69, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_70 = new EMVQRTagMeta(TagId._70, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_71 = new EMVQRTagMeta(TagId._71, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_72 = new EMVQRTagMeta(TagId._72, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_73 = new EMVQRTagMeta(TagId._73, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_74 = new EMVQRTagMeta(TagId._74, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_75 = new EMVQRTagMeta(TagId._75, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_76 = new EMVQRTagMeta(TagId._76, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_77 = new EMVQRTagMeta(TagId._77, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_78 = new EMVQRTagMeta(TagId._78, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_79 = new EMVQRTagMeta(TagId._79, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_80 = new EMVQRTagMeta(TagId._80, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_81 = new EMVQRTagMeta(TagId._81, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_82 = new EMVQRTagMeta(TagId._82, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_83 = new EMVQRTagMeta(TagId._83, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_84 = new EMVQRTagMeta(TagId._84, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_85 = new EMVQRTagMeta(TagId._85, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_86 = new EMVQRTagMeta(TagId._86, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_87 = new EMVQRTagMeta(TagId._87, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_88 = new EMVQRTagMeta(TagId._88, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_89 = new EMVQRTagMeta(TagId._89, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_90 = new EMVQRTagMeta(TagId._90, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_91 = new EMVQRTagMeta(TagId._91, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_92 = new EMVQRTagMeta(TagId._92, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_93 = new EMVQRTagMeta(TagId._93, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_94 = new EMVQRTagMeta(TagId._94, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_95 = new EMVQRTagMeta(TagId._95, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_96 = new EMVQRTagMeta(TagId._96, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_97 = new EMVQRTagMeta(TagId._97, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_98 = new EMVQRTagMeta(TagId._98, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);
        public static EMVQRTagMeta PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_99 = new EMVQRTagMeta(TagId._99, "Payment System specific Templates", new List<EMVQRTagMeta>() { ADDITIONAL_DATA_FIELD_TEMPLATE_62 }, new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 99), "", true);

        public static EMVQRTagMeta LANGUAGE_PREFERENCE_00 = new EMVQRTagMeta(TagId._00, "Language Preference", new List<EMVQRTagMeta>() { MERCHANT_INFORMATION_LANGUAGE_TEMPLATE_64 }, new DataFormatterLengthFixed(DataFormats._ALPHA_NUMERIC_SPECIAL, 2), "");
        public static EMVQRTagMeta MERCHANT_NAME_ALTERNATE_LANGUAGE_01 = new EMVQRTagMeta(TagId._01, "Merchant Name—Alternate Language", new List<EMVQRTagMeta>() { MERCHANT_INFORMATION_LANGUAGE_TEMPLATE_64 }, new DataFormatterLengthRange(DataFormats._ALPHA, 0, 25), "");
        public static EMVQRTagMeta MERCHANT_CITY_ALTERNATE_LANGUAGE_02 = new EMVQRTagMeta(TagId._02, "Merchant City—Alternate Language", new List<EMVQRTagMeta>() { MERCHANT_INFORMATION_LANGUAGE_TEMPLATE_64 }, new DataFormatterLengthRange(DataFormats._ALPHA, 0, 32), "");

        public static EMVQRTagMeta UNKNOWN_TAG = new EMVQRTagMeta(TagId._00, "Invalid tag", new List<EMVQRTagMeta>(), new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, -1, -1), "Invalid Tag");

        public static EMVQRTagMeta GLOBALLY_UNIQUE_IDENTIFIER_00 = new EMVQRTagMeta(TagId._00, "Globally Unique Identifier",
            new List<EMVQRTagMeta>() {
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_26,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_27,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_28,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_29,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_30,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_31,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_32,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_33,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_34,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_35,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_36,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_37,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_38,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_39,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_40,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_41,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_42,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_43,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_44,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_45,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_46,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_47,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_48,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_49,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_50,
                MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_51,

                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_50,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_51,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_52,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_53,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_54,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_55,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_56,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_57,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_58,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_59,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_60,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_61,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_62,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_63,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_64,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_65,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_66,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_67,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_68,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_69,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_70,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_71,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_72,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_73,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_74,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_75,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_76,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_77,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_78,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_79,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_80,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_81,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_82,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_83,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_84,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_85,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_86,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_87,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_88,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_89,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_90,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_91,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_92,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_93,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_94,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_95,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_96,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_97,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_98,
                PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_99,

                UNRESERVED_TEMPLATES_80,
                UNRESERVED_TEMPLATES_81,
                UNRESERVED_TEMPLATES_82,
                UNRESERVED_TEMPLATES_83,
                UNRESERVED_TEMPLATES_84,
                UNRESERVED_TEMPLATES_85,
                UNRESERVED_TEMPLATES_xx,
                UNRESERVED_TEMPLATES_87,
                UNRESERVED_TEMPLATES_88,
                UNRESERVED_TEMPLATES_89,
                UNRESERVED_TEMPLATES_90,
                UNRESERVED_TEMPLATES_91,
                UNRESERVED_TEMPLATES_92,
                UNRESERVED_TEMPLATES_93,
                UNRESERVED_TEMPLATES_94,
                UNRESERVED_TEMPLATES_95,
                UNRESERVED_TEMPLATES_96,
                UNRESERVED_TEMPLATES_97,
                UNRESERVED_TEMPLATES_98,
                UNRESERVED_TEMPLATES_99
            },
            new DataFormatterLengthRange(DataFormats._ALPHA_NUMERIC_SPECIAL, 0, 32), "");


        static EMVQRTagsEnum()
        {
            EnumList.Add(PAYLOAD_FORMAT_INDICATOR_00);
            EnumList.Add(POINT_OF_INITIATION_METHOD_01);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_02);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_03);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_04);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_05);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_06);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_07);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_08);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_09);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_10);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_11);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_12);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_13);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_14);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_15);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_16);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_17);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_18);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_19);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_20);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_21);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_22);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_23);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_24);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_PRIMITIVE_25);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_26);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_27);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_28);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_29);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_30);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_31);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_32);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_33);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_34);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_35);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_36);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_37);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_38);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_39);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_40);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_41);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_42);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_43);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_44);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_45);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_46);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_47);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_48);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_49);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_50);
            EnumList.Add(MERCHANT_ACCOUNT_INFORMATION_TEMPLATE_51);
            EnumList.Add(MERCHANT_CATEGORY_CODE_52);
            EnumList.Add(TRANSACTION_CURRENCY_53);
            EnumList.Add(TRANSACTION_AMOUNT_54);
            EnumList.Add(TIP_OR_CONVENIENCE_INDICATOR_55);
            EnumList.Add(VALUE_OF_CONVENIENCE_FEE_FIXED_56);
            EnumList.Add(VALUE_OF_CONVENIENCE_FEE_PERCENTAGE_57);
            EnumList.Add(COUNTRY_CODE_58);
            EnumList.Add(MERCHANT_NAME_59);
            EnumList.Add(MERCHANT_CITY_60);
            EnumList.Add(POSTAL_CODE_61);
            EnumList.Add(ADDITIONAL_DATA_FIELD_TEMPLATE_62);
            EnumList.Add(CRC_63);
            EnumList.Add(MERCHANT_INFORMATION_LANGUAGE_TEMPLATE_64);
            EnumList.Add(UNRESERVED_TEMPLATES_80);
            EnumList.Add(UNRESERVED_TEMPLATES_81);
            EnumList.Add(UNRESERVED_TEMPLATES_82);
            EnumList.Add(UNRESERVED_TEMPLATES_83);
            EnumList.Add(UNRESERVED_TEMPLATES_84);
            EnumList.Add(UNRESERVED_TEMPLATES_85);
            EnumList.Add(UNRESERVED_TEMPLATES_xx);
            EnumList.Add(UNRESERVED_TEMPLATES_87);
            EnumList.Add(UNRESERVED_TEMPLATES_88);
            EnumList.Add(UNRESERVED_TEMPLATES_89);
            EnumList.Add(UNRESERVED_TEMPLATES_90);
            EnumList.Add(UNRESERVED_TEMPLATES_91);
            EnumList.Add(UNRESERVED_TEMPLATES_92);
            EnumList.Add(UNRESERVED_TEMPLATES_93);
            EnumList.Add(UNRESERVED_TEMPLATES_94);
            EnumList.Add(UNRESERVED_TEMPLATES_95);
            EnumList.Add(UNRESERVED_TEMPLATES_96);
            EnumList.Add(UNRESERVED_TEMPLATES_97);
            EnumList.Add(UNRESERVED_TEMPLATES_98);
            EnumList.Add(UNRESERVED_TEMPLATES_99);
            EnumList.Add(BILL_NUMBER_01);
            EnumList.Add(MOBILE_NUMBER_02);
            EnumList.Add(STORE_LABEL_03);
            EnumList.Add(LOYALTY_NUMBER_04);
            EnumList.Add(REFERENCE_LABEL_05);
            EnumList.Add(CUSTOMER_LABEL_06);
            EnumList.Add(TERMINAL_LABEL_07);
            EnumList.Add(PURPOSE_OF_TRANSACTION_08);
            EnumList.Add(ADDITIONAL_CONSUMER_DATA_REQUEST_09);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_50);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_51);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_52);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_53);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_54);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_55);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_56);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_57);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_58);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_59);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_60);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_61);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_62);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_63);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_64);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_65);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_66);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_67);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_68);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_69);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_70);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_71);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_72);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_73);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_74);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_75);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_76);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_77);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_78);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_79);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_80);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_81);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_82);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_83);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_84);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_85);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_86);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_87);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_88);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_89);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_90);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_91);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_92);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_93);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_94);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_95);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_96);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_97);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_98);
            EnumList.Add(PAYMENT_SYSTEM_SPECIFIC_TEMPLATES_99);
            EnumList.Add(LANGUAGE_PREFERENCE_00);
            EnumList.Add(MERCHANT_NAME_ALTERNATE_LANGUAGE_01);
            EnumList.Add(MERCHANT_CITY_ALTERNATE_LANGUAGE_02);
            EnumList.Add(GLOBALLY_UNIQUE_IDENTIFIER_00);

        }

        public static QRDE CreateUnknown(TagId tagId, string value, QRDE parent = null)
        {
            EMVQRTagMeta newEnum = new EMVQRTagMeta(tagId, UNKNOWN_TAG.Name, new List<EMVQRTagMeta>(), UNKNOWN_TAG.DataFormatter, UNKNOWN_TAG.Description, UNKNOWN_TAG.IsTemplate);
            QRDE qde = QRDE.Create(newEnum, value, parent);
            return qde;
        }
        private static EMVQRTagMeta GetMeta(TagId tagLabel, TagId tagLabelParent)
        {
            EMVQRTagMeta meta;
            if (tagLabelParent == TagId.None)
                meta = EnumList.Find(x => x.Tag == tagLabel && x.TagParents.Count == 0);
            else
                meta = EnumList.Find(x => x.Tag == tagLabel && x.TagParents.Exists(y=>y.Tag == tagLabelParent));

            if (meta == null)
                return UNKNOWN_TAG;// throw new EMVProtocolException("GetMeta, could not find meta class for:" + tagLabel);

            return meta;
        }
        public static DataFormatterBase GetFormatter(TagId tagLabel, TagId tagLabelParent)
        {
            return GetMeta(tagLabel, tagLabelParent).DataFormatter;
        }
        public static string GetName(TagId tagLabel, TagId tagLabelParent)
        {
            return GetMeta(tagLabel, tagLabelParent).Name;
        }
        public static bool IsTemplate(TagId tagLabel, TagId tagLabelParent)
        {
            return GetMeta(tagLabel, tagLabelParent).IsTemplate;
        }
    }
}
