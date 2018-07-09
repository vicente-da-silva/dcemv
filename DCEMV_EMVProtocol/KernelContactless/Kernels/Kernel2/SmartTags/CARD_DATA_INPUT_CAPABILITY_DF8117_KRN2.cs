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

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2 : SmartTag
    {
        public class CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2_VALUE : SmartValue
        {
            public CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool ManualKeyEntryCapable { get; set; }
            public bool MagneticStripeCapable { get; set; }
            public bool ICWithContactsCapable { get; set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], ManualKeyEntryCapable, 8);
                Formatting.SetBitPosition(ref Value[0], MagneticStripeCapable, 7);
                Formatting.SetBitPosition(ref Value[0], ICWithContactsCapable, 6);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                ManualKeyEntryCapable = Formatting.GetBitPosition(Value[0], 8);
                MagneticStripeCapable = Formatting.GetBitPosition(Value[0], 7);
                ICWithContactsCapable = Formatting.GetBitPosition(Value[0], 6);

                return pos;
            }
        }
        public new CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2_VALUE Value { get { return (CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2_VALUE)Val; } }
        public CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2, 
                   new CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2_VALUE(EMVTagsEnum.CARD_DATA_INPUT_CAPABILITY_DF8117_KRN2.DataFormatter))
        {
        }

        
    }
}
