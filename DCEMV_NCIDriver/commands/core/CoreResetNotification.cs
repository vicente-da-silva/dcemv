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
using System;
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class CoreResetNotification : NotificationPacket
    {
        private byte reasonCode;
        private ConfigStatusEnum configStatus;

        public CoreResetNotification() { }

        public CoreResetNotification(PacketBoundryFlagEnum pbf,byte resonCode) : base(pbf,GroupIdentifierEnum.NCI_Core,(byte)OpcodeCoreIdentifierEnum.CORE_RESET_CMD)
        {
            this.reasonCode = resonCode;
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            reasonCode = payLoad[0];
            configStatus = (ConfigStatusEnum)Enum.ToObject(typeof(ConfigStatusEnum), payLoad[1]);
        }

        public override byte[] serialize()
        {
            payLoad = new byte[2];
            payLoad[0] = reasonCode;
            payLoad[1] = (byte)configStatus;
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("ReasonCode: " + reasonCode);
            sb.AppendLine("ConfigStatus: " + configStatus);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
