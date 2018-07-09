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

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class DS_ID_9F5E_KRN2 : SmartTag
    {
        public class DS_ID_9F5E_KRN2_VALUE : SmartValue
        {
            public DS_ID_9F5E_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public override byte[] Serialize()
            {
                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);
                return pos;
            }
        }

        public new DS_ID_9F5E_KRN2_VALUE Value { get { return (DS_ID_9F5E_KRN2_VALUE)Val; } }
        public DS_ID_9F5E_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.DS_ID_9F5E_KRN2, 
                  new DS_ID_9F5E_KRN2_VALUE(EMVTagsEnum.DS_ID_9F5E_KRN2.DataFormatter))
        {
        }
    }
}
