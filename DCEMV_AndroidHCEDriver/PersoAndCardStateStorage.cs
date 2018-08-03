using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;

namespace DCEMV_AndroidHCEDriver
{
    public static class PersoAndCardStateStorage
    {
        public static TLV APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN { get; set; }
        public static TLV APPLICATION_INTERCHANGE_PROFILE_82_KRN { get; set; }
        public static TLV TRACK_2_EQUIVALENT_DATA_57_KRN { get; set; }
        public static TLV CARDHOLDER_NAME_5F20_KRN { get; set; }
        public static TLV APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN { get; set; }
        public static TLV APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN { get; set; }
        public static TLV FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3 { get; set; }
        public static TLV CUSTOMER_EXCLUSIVE_DATA_CED_9F7C_KRN3 { get; set; }
        public static TLV CARD_ADDITIONAL_PROCESSES_9F68_KRN { get; set; }
        public static TLV ISSUER_APPLICATION_DATA_9F10_KRN { get; set; }

        public static byte[] ICC_MK { get; set; }

        static PersoAndCardStateStorage()
        {
            APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN = TLV.Create(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag, Formatting.HexStringToByteArray("0001"));
            APPLICATION_INTERCHANGE_PROFILE_82_KRN = TLV.Create(EMVTagsEnum.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Tag, Formatting.HexStringToByteArray("0000"));
            TRACK_2_EQUIVALENT_DATA_57_KRN = TLV.Create(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag, calcTrack2());
            CARDHOLDER_NAME_5F20_KRN = TLV.Create(EMVTagsEnum.CARDHOLDER_NAME_5F20_KRN.Tag, Formatting.HexStringToByteArray("202F"));
            APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN = TLV.Create(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag, Formatting.HexStringToByteArray("1234567890123456"));
            APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN = TLV.Create(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag, Formatting.HexStringToByteArray("01"));
            FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3 = TLV.Create(EMVTagsEnum.FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3.Tag, Formatting.HexStringToByteArray("00000000"));
            CUSTOMER_EXCLUSIVE_DATA_CED_9F7C_KRN3 = TLV.Create(EMVTagsEnum.CUSTOMER_EXCLUSIVE_DATA_CED_9F7C_KRN3.Tag, Formatting.HexStringToByteArray("0000000000000000000000000000000000000000000000000000000000000000"));
            CARD_ADDITIONAL_PROCESSES_9F68_KRN = TLV.Create(EMVTagsEnum.CARD_ADDITIONAL_PROCESSES_9F68_KRN.Tag, new byte[] { 0x00, 0x60, (byte)0x80, 0x00 });

            ISSUER_APPLICATION_DATA_9F10_KRN = TLV.Create(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
            ISSUER_APPLICATION_DATA_9F10_KRN.Val.PackValue(32);
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[0] = 0x06;//format
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[1] = 0x00;//kdi
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[2] = 0x11;//cvn , crypto 17

            ISSUER_APPLICATION_DATA_9F10_KRN.Value[3] = 0x00;//cvr byte 1
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[4] = 0x00;//cvr byte 2
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[5] = 0x00;//cvr byte 3
            ISSUER_APPLICATION_DATA_9F10_KRN.Value[6] = 0x00;//cvr byte 4

            //with emv demo app which uses the hsm
            ICC_MK = Formatting.HexStringToByteArray("8CB9F7D54362B0A240D0AE626780A86B");
            //with simulated provider, standard master ac test key -> 2315208C9110AD402315208C9110AD40 -> derives this key for this pan and this pan sequence number
            //ICC_MK = Formatting.HexStringToByteArray("3DAD707C897F49895D6D62526297497A");
        }

        private static byte[] calcTrack2()
        {
            //1234567890123456D2512201
            byte[] source = Formatting.StringToBcd("1234567890123456" + "D" + "2512" + "201", false);
            return source;
        }
    }
}
