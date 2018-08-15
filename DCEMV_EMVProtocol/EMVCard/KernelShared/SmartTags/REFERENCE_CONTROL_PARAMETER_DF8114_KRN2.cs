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

namespace DCEMV.EMVProtocol.Kernels
{
    public class REFERENCE_CONTROL_PARAMETER_DF8114_KRN2 : SmartTag
    {
        public class REFERENCE_CONTROL_PARAMETER_DF8114_KRN2_VALUE : SmartValue
        {
            public REFERENCE_CONTROL_PARAMETER_DF8114_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }

            public ACTypeEnum ACTypeEnum { get; set; }
            public bool CDASignatureRequested { get; set; }

            public override byte[] Serialize()
            {
                Value[0] = 0x00;
                Formatting.SetBitPosition(ref Value[0], CDASignatureRequested, 5);
                Value[0] = (byte)(Value[0] | ((byte)ACTypeEnum << 6));

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                CDASignatureRequested = Formatting.GetBitPosition(Value[0], 5);
                ACTypeEnum = (ACTypeEnum)GetEnum(typeof(ACTypeEnum), Value[0] >> 6);

                return pos;
            }
        }

        public new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2_VALUE Value { get { return (REFERENCE_CONTROL_PARAMETER_DF8114_KRN2_VALUE)Val; } }

        public REFERENCE_CONTROL_PARAMETER_DF8114_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.REFERENCE_CONTROL_PARAMETER_DF8114_KRN2, 
                  new REFERENCE_CONTROL_PARAMETER_DF8114_KRN2_VALUE(EMVTagsEnum.REFERENCE_CONTROL_PARAMETER_DF8114_KRN2.DataFormatter))
        {
        }
    }
}
