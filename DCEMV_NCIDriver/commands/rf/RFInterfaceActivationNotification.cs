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
    public class RFInterfaceActivationNotification : RFManagementNotification
    {
        public RFInterfaceEnum RFInterface { get; set; }
        public byte MaxDataPacketPayloadSize { get; set; }
        public byte InitialNoOfCredits { get; set; }
        public RFTechnologiesAndModeEnum DataExchangeRFTechAndMode { get; set; }
        public byte DataExchangeTransmitBitRate { get; set; }
        public byte DataExchangeReceiveBitRate { get; set; }
        public ActivationParameterBase ActivationParameter { get; set; }

        public RFInterfaceActivationNotification() { }

        public RFInterfaceActivationNotification(PacketBoundryFlagEnum pbf) : base(pbf,OpcodeRFIdentifierEnum.RF_INTF_ACTIVATED_NTF)
        {
            
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            sb.AppendLine("RFInterface: " + RFInterface);
            sb.AppendLine("MaxDataPacketPayloadSize: " + MaxDataPacketPayloadSize);
            sb.AppendLine("InitialNoOfCredits: " + InitialNoOfCredits);
            sb.AppendLine("DataExchangeRFTechAndMode: " + DataExchangeRFTechAndMode);
            sb.AppendLine("DataExchangeTransmitBitRate: " + DataExchangeTransmitBitRate);
            sb.AppendLine("DataExchangeReceiveBitRate: " + DataExchangeReceiveBitRate);
            sb.AppendLine("---ActivationParameter---");
            if (ActivationParameter!=null)
                sb.AppendLine(ActivationParameter.ToString());
            else
                sb.AppendLine("NULL");
            sb.AppendLine("---");
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);
            RFDiscoveryId = payLoad[0];
            RFInterface = (RFInterfaceEnum)EnumUtil.GetEnum(typeof(RFInterfaceEnum), payLoad[1]);
            RFProtocol = (RFProtocolEnum)EnumUtil.GetEnum(typeof(RFProtocolEnum), payLoad[2]);
            RFTechnologiesAndMode = (RFTechnologiesAndModeEnum)EnumUtil.GetEnum(typeof(RFTechnologiesAndModeEnum), payLoad[3]);
            MaxDataPacketPayloadSize = payLoad[4];
            InitialNoOfCredits = payLoad[5];

            byte lengthOfTechModeParam = payLoad[6];
            byte pos = 7;
            
            if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCAPollMode();
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_LISTEN_MODE)
                TechSpecificParam = null;
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_B_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCBPollMode();
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_B_PASSIVE_LISTEN_MODE)
                TechSpecificParam = null;
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_F_PASSIVE_POLL_MODE)
                TechSpecificParam = new TechSpecificParamsNFCFPollMode();
            else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_F_PASSIVE_LISTEN_MODE)
                TechSpecificParam = new TechSpecificParamsNFCFListenMode();
            else if ((byte)RFTechnologiesAndMode >= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_START_LISTEN &&
                     (byte)RFTechnologiesAndMode <= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_END_LISTEN)
                TechSpecificParam = null;
            else
                throw new Exception("Invalid RFTechnologiesAndMode found in RFDiscoverNotification");

            if(TechSpecificParam != null)
                pos = TechSpecificParam.deserialize(payLoad, pos);
            else
                pos = (byte)(pos + lengthOfTechModeParam);

            DataExchangeRFTechAndMode = (RFTechnologiesAndModeEnum)EnumUtil.GetEnum(typeof(RFTechnologiesAndModeEnum), payLoad[pos]);
            pos++;
            DataExchangeTransmitBitRate = payLoad[pos];
            pos++;
            DataExchangeReceiveBitRate = payLoad[pos];
            pos++;

            if (RFInterface == RFInterfaceEnum.INTF_ISODEP)
            {
                if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_POLL_MODE)
                    ActivationParameter = new ActivationParameterNFCA_ISODEP_POLL();
                else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_B_PASSIVE_POLL_MODE)
                    ActivationParameter = new ActivationParameterNFCB_ISODEP_POLL();
                else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_LISTEN_MODE)
                    ActivationParameter = new ActivationParameterNFCA_ISODEP_LISTEN();
                else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_B_PASSIVE_LISTEN_MODE)
                    ActivationParameter = new ActivationParameterNFCB_ISODEP_LISTEN();
                else if ((byte)RFTechnologiesAndMode >= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_START_LISTEN &&
                         (byte)RFTechnologiesAndMode <= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_END_LISTEN)
                    ActivationParameter = null;
                else
                    throw new Exception("Invalid RFTechnologiesAndMode found in RFDiscoverNotification");
            }
            else if (RFInterface == RFInterfaceEnum.INTF_NFCDEP)
            {
                if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_POLL_MODE ||  //NFC-DEP Poll Mode
                    RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_F_PASSIVE_POLL_MODE)
                    ActivationParameter = new ActivationParameterNFCA_NFCF__DEP_POLL();
                else if (RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_A_PASSIVE_LISTEN_MODE || // NFC-DEP Listen Mode
                         RFTechnologiesAndMode == RFTechnologiesAndModeEnum.NFC_F_PASSIVE_LISTEN_MODE)
                    ActivationParameter = new ActivationParameterNFCA_NFCF__DEP_LISTEN();
                else if ((byte)RFTechnologiesAndMode >= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_START_LISTEN &&
                         (byte)RFTechnologiesAndMode <= (byte)RFTechnologiesAndModeEnum.PROPRIETARY_END_LISTEN)
                    ActivationParameter = null;
                else
                    throw new Exception("Invalid RFTechnologiesAndMode found in RFDiscoverNotification");
            }

            if (ActivationParameter != null)
                pos = ActivationParameter.deserialize(payLoad, pos);
            else
            {
                byte lengthOfActivationParameter = payLoad[pos];
                pos = (byte)(pos + 1 + lengthOfActivationParameter);
            }
        }

        public override byte[] serialize()
        {
            byte pos = 7;
            byte[] ser = new byte[0];
            byte[] ser2 = new byte[0];
            if (TechSpecificParam!=null)
                ser = TechSpecificParam.serialize();
            if (ActivationParameter != null)
                ser2 = ActivationParameter.serialize();

            payLoad = new byte[pos + ser.Length + 3 + ser2.Length];

            payLoad[0] = RFDiscoveryId;
            payLoad[1] = (byte)RFInterface;
            payLoad[2] = (byte)RFProtocol;
            payLoad[3] = (byte)RFTechnologiesAndMode;
            payLoad[4] = MaxDataPacketPayloadSize;
            payLoad[5] = InitialNoOfCredits;
            payLoad[6] = (byte)ser.Length; 
            Array.Copy(ser, 0, payLoad, pos, ser.Length);
            pos = (byte)(pos + ser.Length);
            payLoad[pos] = (byte)DataExchangeRFTechAndMode;
            pos++;
            payLoad[pos] = DataExchangeTransmitBitRate;
            pos++;
            payLoad[pos] = DataExchangeReceiveBitRate;
            pos++;
            Array.Copy(ser2, 0, payLoad, pos, ser2.Length);
            pos = (byte)(pos + ser2.Length);
            return base.serialize();
        }
    }
}
