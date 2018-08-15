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
    public class DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2 : SmartTag
    {
        public class DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2_VALUE : SmartValue
        {
            public DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {

            }
            public bool PermanentSlotType { get; set;}
            public bool VolatileSlotType { get; set; }
            public bool LowVolatility { get; set; }
            public bool LockedSlot { get; set; }
            public bool DeactivatedSlot { get; set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], PermanentSlotType, 8);
                Formatting.SetBitPosition(ref Value[0], VolatileSlotType, 7);
                Formatting.SetBitPosition(ref Value[0], LowVolatility, 6);
                Formatting.SetBitPosition(ref Value[0], LockedSlot, 5);
                Formatting.SetBitPosition(ref Value[0], DeactivatedSlot, 1);

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                PermanentSlotType = Formatting.GetBitPosition(Value[0], 8);
                VolatileSlotType = Formatting.GetBitPosition(Value[0], 7);
                LowVolatility = Formatting.GetBitPosition(Value[0], 6);
                LockedSlot = Formatting.GetBitPosition(Value[0], 5);
                DeactivatedSlot = Formatting.GetBitPosition(Value[0], 1);

                return pos;
            }
        }

        public new DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2_VALUE Value { get { return (DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2_VALUE)Val; } }
        public DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2, 
                  new DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2_VALUE(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2.DataFormatter))
        {
        }
    }
}
