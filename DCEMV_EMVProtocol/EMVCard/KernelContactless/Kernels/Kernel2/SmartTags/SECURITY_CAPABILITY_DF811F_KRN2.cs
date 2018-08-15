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
    public class SECURITY_CAPABILITY_DF811F_KRN2 : SmartTag
    {
        public class SECURITY_CAPABILITY_DF811F_KRN2_VALUE : SmartValue
        {
            public SECURITY_CAPABILITY_DF811F_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public bool SDACapable { get; set; }
            public bool DDACapable { get; set; }
            public bool CardCaptureCapable { get; set; }
            public bool RFUCapable { get; set; }
            public bool CDACapable { get; set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], SDACapable, 8);
                Formatting.SetBitPosition(ref Value[0], DDACapable, 7);
                Formatting.SetBitPosition(ref Value[0], CardCaptureCapable, 6);
                Formatting.SetBitPosition(ref Value[0], RFUCapable, 5);
                Formatting.SetBitPosition(ref Value[0], CDACapable, 4);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                SDACapable = Formatting.GetBitPosition(Value[0], 8);
                DDACapable = Formatting.GetBitPosition(Value[0], 7);
                CardCaptureCapable = Formatting.GetBitPosition(Value[0], 6);
                RFUCapable = Formatting.GetBitPosition(Value[0], 5);
                CDACapable = Formatting.GetBitPosition(Value[0], 4);

                return pos;
            }
        }

        public new SECURITY_CAPABILITY_DF811F_KRN2_VALUE Value { get { return (SECURITY_CAPABILITY_DF811F_KRN2_VALUE)Val; } }

        public SECURITY_CAPABILITY_DF811F_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.SECURITY_CAPABILITY_DF811F_KRN2, 
                  new SECURITY_CAPABILITY_DF811F_KRN2_VALUE(EMVTagsEnum.SECURITY_CAPABILITY_DF811F_KRN2.DataFormatter))
        {
        }

        
    }
}
