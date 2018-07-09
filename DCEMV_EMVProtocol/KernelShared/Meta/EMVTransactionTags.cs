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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public class EMVTransactionTags
    {
        public static TLVList EMVTransactionTagsList = new TLVList();

        static EMVTransactionTags()
        {
            CreateEntry(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN, Formatting.HexStringToByteArray("00000000"));
            CreateEntry(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN, Formatting.HexStringToByteArray("000000001000"));
            CreateEntry(EMVTagsEnum.AMOUNT_OTHER_NUMERIC_9F03_KRN, Formatting.HexStringToByteArray("000000000000"));
            CreateEntry(EMVTagsEnum.TRANSACTION_DATE_9A_KRN, Formatting.HexStringToByteArray("161118"));
            CreateEntry(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN, Formatting.HexStringToByteArray("00"));
            CreateEntry(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN, Formatting.HexStringToByteArray("0000000000"));
        }

        private static void CreateEntry(EMVTagMeta emvTagMeta, byte[] value)
        {
            TLV tlv = TLV.Create(emvTagMeta.Tag);
            tlv.Value = value;
            EMVTransactionTagsList.AddToList(tlv);
        }
    }
}
