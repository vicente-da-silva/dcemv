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
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class RFDeactivateNotification : RFManagementNotification
    {
        private DeactivationTypeEnum DeactivationType { get; set; }
        private DeactivationReasonEnum DeactivationReason { get; set; }

        public RFDeactivateNotification() { }

        public RFDeactivateNotification(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_DEACTIVATE_CMD) 
        {
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("DeactivationType: " + DeactivationType);
            sb.AppendLine("DeactivationReason: " + DeactivationReason);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            DeactivationType = (DeactivationTypeEnum)EnumUtil.GetEnum(typeof(DeactivationTypeEnum), payLoad[0]);
            DeactivationReason = (DeactivationReasonEnum)EnumUtil.GetEnum(typeof(DeactivationReasonEnum), payLoad[1]);
        }

        public override byte[] serialize()
        {
            payLoad = new byte[2];
            payLoad[0] = (byte)DeactivationType;
            payLoad[1] = (byte)DeactivationReason;
            return base.serialize();
        }
    }
}
