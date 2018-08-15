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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public class EMVTerminalTagDefaults
    {
        public static TLVList EMVTagDefaultsList = new TLVList();

        static EMVTerminalTagDefaults()
        {
            CreateEntry(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN, Formatting.HexStringToByteArray("86A00000"));
            CreateEntry(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN, Formatting.HexStringToByteArray(Convert.ToString(ISOCurrencyCodesEnum.ZAR.Code).PadLeft(4, '0')));
            CreateEntry(EMVTagsEnum.TERMINAL_COUNTRY_CODE_9F1A_KRN, Formatting.HexStringToByteArray(Convert.ToString(ISOCountryCodesEnum.ZAF.Code).PadLeft(4, '0')));
        }

        private static void CreateEntry(EMVTagMeta emvTagMeta, byte[] value)
        {
            TLV tlv = TLV.Create(emvTagMeta.Tag);
            tlv.Value = value;
            EMVTagDefaultsList.AddToList(tlv);
        }
    }
}
