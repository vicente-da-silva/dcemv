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
    public class RFDiscoverNotification : RFManagementNotification
    {
        public DiscoverNotificationTypeEnum DiscoverNotificationType { get; set; }

        public RFDiscoverNotification() { }

        public RFDiscoverNotification(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_DISCOVER_CMD)
        {
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            RFDiscoveryId = payLoad[0];
            RFProtocol = (RFProtocolEnum)EnumUtil.GetEnum(typeof(RFProtocolEnum), payLoad[1]);
            RFTechnologiesAndMode = (RFTechnologiesAndModeEnum)EnumUtil.GetEnum(typeof(RFTechnologiesAndModeEnum), payLoad[2]);
            byte techModeLength = payLoad[3];
            byte pos = 4;

            if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCAPollMode();
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_B_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCBPollMode();
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_F_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCFPollMode();
            else
                throw new Exception("Invalid RFTechnologiesAndMode found in RFDiscoverNotification");

            pos = TechSpecificParam.deserialize(payLoad, pos);
            
            DiscoverNotificationType = (DiscoverNotificationTypeEnum)EnumUtil.GetEnum(typeof(DiscoverNotificationTypeEnum), payLoad[pos]);
        }

        public override byte[] serialize()
        {
            byte pos = 4;
            byte[] ser = TechSpecificParam.serialize();
            payLoad = new byte[pos + ser.Length + 1];

            payLoad[0] = RFDiscoveryId;
            payLoad[1] = (byte)RFProtocol;
            payLoad[2] = (byte)RFTechnologiesAndMode;
            payLoad[3] = (byte)(ser.Length + 1);
            Array.Copy(ser, 0,payLoad, pos, ser.Length);
            pos = (byte)(pos + ser.Length);
            payLoad[pos] = (byte)DiscoverNotificationType;

            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("DiscoverNotificationType:" + DiscoverNotificationType);
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
