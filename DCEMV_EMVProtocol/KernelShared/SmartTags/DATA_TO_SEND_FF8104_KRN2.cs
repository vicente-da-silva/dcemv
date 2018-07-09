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
using System.Collections.Generic;
using System.Linq;

namespace DCEMV.EMVProtocol.Kernels
{
    public class DATA_TO_SEND_FF8104_KRN2 : SmartTag
    {
        public class DATA_TO_SEND_FF8104_KRN2_VALUE : SmartValue
        {
            private List<string> Tags;
            public DATA_TO_SEND_FF8104_KRN2_VALUE(DataFormatterBase dataFormatter)
                : base(dataFormatter)
            {
                Tags = new List<string>();
            }

            public void AddTag(EMVTagMeta tag)
            {
                Tags.Add(tag.Tag);
            }

            public override byte[] Serialize()
            {
                if (Tags.Count == 0)
                    return new byte[0];

                Value = Formatting.HexStringToByteArray(Tags.Aggregate((x, y) => x + y));
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                int posInV = 0;
                while (posInV < Value.Length)
                    Tags.Add(T.Create(Value, ref posInV).TagLable);

                return pos;
            }
        }

        public new DATA_TO_SEND_FF8104_KRN2_VALUE Value { get { return (DATA_TO_SEND_FF8104_KRN2_VALUE)Val; } }

        public DATA_TO_SEND_FF8104_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2, new DATA_TO_SEND_FF8104_KRN2_VALUE(EMVTagsEnum.DATA_TO_SEND_FF8104_KRN2.DataFormatter))
        {
        }
    }
}
