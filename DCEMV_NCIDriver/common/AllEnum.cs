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

namespace DCEMV.CardReaders.NCIDriver
{
    public class EnumUtil
    {
        public static object GetEnum(Type enumtype, int identifier)
        {
            if (!Enum.IsDefined(enumtype, identifier))
                throw new Exception(enumtype + " enum not defined for:" + identifier);
            return Enum.ToObject(enumtype, identifier);
        }
    }

    public enum PacketTypeEnum
    {
        Data = 0x00, //000x
        ControlCommand = 0x02, //001x
        ControlResponse = 0x04, //010x
        ControlNotification = 0x06 //011x
    }

    public enum PacketBoundryFlagEnum
    {
        CompleteMessageOrLastSegment = 0x00, //0
        MessageSegment = 0x01, //1
    }

    public enum GroupIdentifierEnum
    {
        NCI_Core = 0x00,
        RFMANAGEMENT = 0x01,
        NFCEEMnanagement = 0x02,
        PROPRIETARY = 0x0F
    }

    public enum OpcodeCoreIdentifierEnum
    {
        CORE_RESET_CMD = 0x00,
        //CORE_RESET_RSP
        //CORE_RESET_NTF

        CORE_INIT_CMD = 0x01,
        //CORE_INIT_RSP

        CORE_SET_CONFIG_CMD = 0x02,
        //CORE_SET_CONFIG_RSP

        CORE_GET_CONFIG_CMD = 0x03,
        //CORE_GET_CONFIG_RSP

        CORE_CONN_CREATE_CMD = 0x04,
        //CORE_CONN_CREATE_RSP

        CORE_CONN_CLOSE_CMD = 0x05,
        //CORE_CONN_CLOSE_RSP

        CORE_CONN_CREDITS_NTF = 0x06,
        CORE_GENERIC_ERROR_NTF = 0x07,
        CORE_INTERFACE_ERROR_NTF = 0x08,
    }

    public enum OpcodeRFIdentifierEnum
    {
        RF_DISCOVER_MAP_CMD=0x00,
        //RF_DISCOVER_MAP_RSP
        RF_SET_LISTEN_MODE_ROUTING_CMD = 0x01,
        //RF_SET_LISTEN_MODE_ROUTING_RSP
        RF_GET_LISTEN_MODE_ROUTING_CMD = 0x02,
        //RF_GET_LISTEN_MODE_ROUTING_RSP
        //RF_GET_LISTEN_MODE_ROUTING_NTF
        RF_DISCOVER_CMD = 0x03,
        //RF_DISCOVER_RSP
        //RF_DISCOVER_NTF
        RF_DISCOVER_SELECT_CMD = 0x04,
        //RF_DISCOVER_SELECT_RSP
        RF_INTF_ACTIVATED_NTF = 0x05,
        RF_DEACTIVATE_CMD = 0x06,
        //RF_DEACTIVATE_RSP
        //RF_DEACTIVATE_NTF
        RF_FIELD_INFO_NTF = 0x07,
        RF_T3T_POLLING_CMD = 0x08,
        //RF_T3T_POLLING_RSP
        //RF_T3T_POLLING_NTF
        RF_NFCEE_ACTION_NTF = 0x09,
        RF_NFCEE_DISCOVERY_REQ_NTF = 0x0A,
        RF_PARAMETER_UPDATE_CMD = 0x0B,
        //RF_PARAMETER_UPDATE_RSP
    }

    public enum OpcodeNFCEEManagementEnum
    {
        NFCEE_DISCOVER_CMD = 0x00,
        //NFCEE_DISCOVER_RSP
        //NFCEE_DISCOVER_NTF
        NFCEE_MODE_SET_CMD = 0x01,
        //NFCEE_MODE_SET_RSP
    }

    public enum OpcodeProprietaryExtensionsEnum
    {
        NCI_PROPRIETARY_ACT_CMD = 0x02,
        NCI_PROPRIETARY_ISO_DEP_CHECK_CMD = 0x11
    }

    public enum ConfigParamTag
    {
        TOTAL_DURATION = 0x00,
        CON_DEVICES_LIMIT = 0x01,
        PA_BAIL_OUT = 0x08,
        PB_AFI = 0x10,
        PB_BAIL_OUT = 0x11,
        PB_ATTRIB_PARAM1 = 0x12,
        PB_SENSB_REQ_PARAM = 0x13,
        PF_BIT_RATE = 0x18,
        PF_RC_CODE = 0x19,
        PB_H_INFO = 0x20,
        PI_BIT_RATE = 0x21,
        PA_ADV_FEAT = 0x22,
        PN_NFC_DEP_SPEED = 0x28,
        PN_ATR_REQ_GEN_BYTES = 0x29,
        PN_ATR_REQ_CONFIG = 0x2A,
        LA_BIT_FRAME_SDD = 0x30,
        LA_PLATFORM_CONFIG = 0x31,
        LA_SEL_INFO = 0x32,
        LA_NFCID1 = 0x33,
        LB_SENSB_INFO = 0x38,
        LB_NFCID0 = 0x39,
        LB_APPLICATION_DATA = 0x3A,
        LB_SFGI = 0x3B,
        LB_ADC_FO = 0x3C,
        LF_T3T_IDENTIFIERS_1 = 0x40,
        LF_T3T_IDENTIFIERS_2 = 0x41,
        LF_T3T_IDENTIFIERS_3 = 0x42,
        LF_T3T_IDENTIFIERS_4 = 0x43,
        LF_T3T_IDENTIFIERS_5 = 0x44,
        LF_T3T_IDENTIFIERS_6 = 0x45,
        LF_T3T_IDENTIFIERS_7 = 0x46,
        LF_T3T_IDENTIFIERS_8 = 0x47,
        LF_T3T_IDENTIFIERS_9 = 0x48,
        LF_T3T_IDENTIFIERS_10 = 0x49,
        LF_T3T_IDENTIFIERS_11 = 0x4A,
        LF_T3T_IDENTIFIERS_12 = 0x4B,
        LF_T3T_IDENTIFIERS_13 = 0x4C,
        LF_T3T_IDENTIFIERS_14 = 0x4D,
        LF_T3T_IDENTIFIERS_15 = 0x4E,
        LF_T3T_IDENTIFIERS_16 = 0x4F,
        LF_PROTOCOL_TYPE = 0x50,
        LF_T3T_PMM = 0x51,
        LF_T3T_MAX = 0x52,
        LF_T3T_FLAGS = 0x53,
        LF_CON_BITR_F = 0x54,
        LF_ADV_FEAT = 0x55,
        LI_FWI = 0x58,
        LA_HIST_BY = 0x59,
        LB_H_INFO_RESP = 0x5A,
        LI_BIT_RATE = 0x5B,
        LN_WT = 0x60,
        LN_ATR_RES_GEN_BYTES = 0x61,
        LN_ATR_RES_CONFIG = 0x62,
        RF_FIELD_INFO = 0x80,
        RF_NFCEE_ACTION = 0x81,
        NFCDEP_OP = 0x82,

        RESERVED_START = 0xA0,
        RESERVED_END = 0xFE
    }

    public enum ReponseCode
    {
        STATUS_OK = 0x00,
        STATUS_REJECTED = 0x01,
        STATUS_RF_FRAME_CORRUPTED = 0x02,
        STATUS_FAILED = 0x03,
        STATUS_NOT_INITIALIZED = 0x04,
        STATUS_SYNTAX_ERROR = 0x05,
        STATUS_SEMANTIC_ERROR = 0x06,
        STATUS_INVALID_PARAM = 0x09,
        STATUS_MESSAGE_SIZE_EXCEEDED = 0x0A,
        DISCOVERY_ALREADY_STARTED = 0xA0,
        DISCOVERY_TARGET_ACTIVATION_FAILED = 0xA1,
        DISCOVERY_TEAR_DOWN = 0xA2,
        RF_TRANSMISSION_ERROR = 0xB0,
        RF_PROTOCOL_ERROR = 0xB1,
        RF_TIMEOUT_ERROR = 0xB2,
        NFCEE_INTERFACE_ACTIVATION_FAILED = 0xC0,
        NFCEE_TRANSMISSION_ERROR = 0xC1,
        NFCEE_PROTOCOL_ERROR = 0xC2,
        NFCEE_TIMEOUT_ERROR = 0xC3,
        PROPRIETARY_START = 0xE0,
        PROPRIETARY_END = 0xFF,
    }

    public enum SupportedInterfacesEnum
    {
        NFCEE_DIRECT_RF_INTERFACE = 0X00,
        FRAME_RF_INTERFACE = 0X01  ,
        ISO_DEP_RF_INTERFACE = 0X02  ,
        NFC_DEP_RF_INTERFACE = 0X03,
        PROPRIETARY_START = 0X80,
        PROPRIETARY_END = 0XFE,
    }

    public enum NCIVersionEnum
    {
        NCIVERSION1 = 0x10,
        NCIVERSION2 = 0x20,
    }

    public enum ConfigStatusEnum
    {
        CONFIG_KEPT = 0x00,
        CONFIG_RESET = 0x01,
    }

    public enum ResetCommandTypeEnum
    {
        KEEP_CONFIG = 0x00,
        RESET_CONFIG = 0x01,
    }

    public enum NxpNciRWOperationEnum
    {
        READ_NDEF,
        WRITE_NDEF,
        PRESENCE_CHECK,
    }

    public enum DeviceModeEnum
    {
        NXPNCI_MODE_CARDEMU = 1 << 1,
        NXPNCI_MODE_P2P = 1 << 2,
        NXPNCI_MODE_RW = 1 << 3,
    }

    public enum RFModeEnum
    {
        MODE_POLL = 0x01,
        MODE_LISTEN = 0x02,
    }

    public enum RFModeMaskEnum
    {
        MODE_MASK = 0xF0,
        MODE_POLL = 0x00,
        MODE_LISTEN = 0x80,
    }

    public enum RFInterfaceEnum
    {
        INTF_UNDETERMINED = 0x0,
        INTF_FRAME = 0x1,
        INTF_ISODEP = 0x2,
        INTF_NFCDEP = 0x3,
        PROPRIETARY_START = 0X80,
        PROPRIETARY_END = 0XFE,

    }
    public enum RFTechnologiesAndModeEnum
    {
        NFC_A_PASSIVE_POLL_MODE = 0x00,
        NFC_B_PASSIVE_POLL_MODE = 0x01,
        NFC_F_PASSIVE_POLL_MODE = 0x02,
        NFC_A_ACTIVE_POLL_MODE_RFU = 0x03,
        NFC_F_ACTIVE_POLL_MODE_RFU = 0x05,
        NFC_15693_PASSIVE_POLL_MODE_RFU = 0x06,
        PROPRIETARY_START_POLL = 0X70,
        PROPRIETARY_END_POLL = 0X7F,
        NFC_A_PASSIVE_LISTEN_MODE = 0x80,
        NFC_B_PASSIVE_LISTEN_MODE = 0x81,
        NFC_F_PASSIVE_LISTEN_MODE = 0x82,
        NFC_A_ACTIVE_LISTEN_MODE_RFU = 0x83,
        NFC_F_ACTIVE_LISTEN_MODE_RFU = 0x85,
        NFC_15693_PASSIVE_LISTEN_MODE_RFU = 0x86,
        PROPRIETARY_START_LISTEN = 0XF0,
        PROPRIETARY_END_LISTEN = 0XFF,
    }
    public enum RFTechnologiesEnum
    {
        NFC_RF_TECHNOLOGY_A = 0x00,
        NFC_RF_TECHNOLOGY_B = 0x01,
        NFC_RF_TECHNOLOGY_F = 0x02,
        NFC_RF_TECHNOLOGY_15693 = 0x03,
        PROPRIETARY_START = 0X80,
        PROPRIETARY_END = 0XFE,
    }
    public enum RFProtocolEnum
    {
        PROT_UNDETERMINED = 0x0,
        PROT_T1T = 0x1,
        PROT_T2T = 0x2,
        PROT_T3T = 0x3,
        PROT_ISODEP = 0x4,
        PROT_NFCDEP = 0x5,
        PROT_MIFARE = 0x80,
        PROPRIETARY_START = 0X80,
        PROPRIETARY_END = 0XFE,
    }

    public enum DiscoverNotificationTypeEnum
    {
        LAST = 0x00,
        LAST_NFCC_LIMIT = 0x01,
        MORE_TO_COME = 0x02,
    }

    public enum DeactivationTypeEnum
    {
        IDLE_MODE = 0x00,
        SLEEP_MODE = 0x01,
        SLEEP_AF_MODE = 0x02,
        DISCOVERY = 0x03,
    }

    public enum DeactivationReasonEnum
    {
        DH_REQUEST = 0x00,
        ENDPOINT_REQUEST = 0x01,
        RF_LINK_LOSS = 0x02,
        NFC_B_BAD_AFI = 0x03,
    }

    
}
