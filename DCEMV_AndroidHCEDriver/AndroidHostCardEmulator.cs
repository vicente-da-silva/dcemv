using System;
using Android.Nfc.CardEmulators;
using Android.OS;
using Android.Runtime;
using DCEMV.Shared;
using System.Threading.Tasks;
using Android.App;
using DCEMV.ISO7816Protocol;
using DCEMV.EMVProtocol;
using DCEMV.FormattingUtils;
using DCEMV.TLVProtocol;
using DCEMV.EMVProtocol.Kernels;
using DCEMV.EMVSecurity;

namespace DCEMV_AndroidHCEDriver
{
    [Service(Exported = true, Enabled = true, Permission = "android.permission.BIND_NFC_SERVICE"),
        IntentFilter(new[] { "android.nfc.cardemulation.action.HOST_APDU_SERVICE" },
        Categories = new[] { "android.intent.category.DEFAULT" }),
        MetaData("android.nfc.cardemulation.host_apdu_service", Resource = "@xml/aid_list")]
    public class AndroidHostCardEmulator : HostApduService
    {
        public static Logger Logger = new Logger(typeof(AndroidHostCardEmulator));

        //2PAY.SYS.DDF01
        private const string PPSE_NAME = "325041592E5359532E4444463031";
        private static byte[] ppseSelectResponse = new byte[] {0x6F,0x21,(byte)0x84,0x0e,0x32,0x50,0x41,0x59,0x2E,
            0x53,0x59,0x53,0x2E,0x44,0x44,0x46,0x30,0x31,(byte)0xA5,0x0f,(byte)0xBF,0x0C,
            0x0c,0x61,0x0a,0x4F,0x08,(byte)0xA0,0x00,0x00,0x00,0x50,0x01,0x01,0x01,0x90,0x00};

        private const string APPLET_AID = "A000000050010101";
        private static byte[] appletSelectResponse = new byte[]{
            0x6F,0x21,
                (byte)0x84,0x08, 0xA0 , 0x00, 0x00, 0x00, 0x50 , 0x01, 0x01, 0x01, 
                    //0x84,0x07, 0x54,0x65,0x73,0x74,0x41,0x70,0x70,
                (byte)0xA5,0x15,
                            0x50, 0x07, 0x54, 0x65, 0x73, 0x74, 0x41, 0x70, 0x70,
                            (byte)0x9F,0x38,0x09,(byte)0x9F,0x02,0x06,(byte)0x9F,0x37,0x04,(byte)0x9F,0x66,0x04,
            0x90,0x00};

        const short Byte1 = 0;
        const short Byte2 = 1;
        const short Byte3 = 2;
        const short Byte4 = 3;
        const short Byte5 = 4;

        const short Bit8 = 7;
        const short Bit7 = 6;
        const short Bit6 = 5;
        const short Bit5 = 4;
        const short Bit4 = 3;
        const short Bit3 = 2;
        const short Bit2 = 1;
        const short Bit1 = 0;

        private static bool isCapPersonalized = true;

        private static short MAX_TTQ_LENGTH = 4;
        private static short MAX_UNPRED_NUM__LENGTH = 4;
        private static short MAX_AMOUNT_AUTH__LENGTH = 6;

        public AndroidHostCardEmulator()
        {
            TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();
        }

        public override void OnDeactivated([GeneratedEnum] DeactivationReason reason)
        {

        }

        public override byte[] ProcessCommandApdu(byte[] commandApdu, Bundle extras)
        {
            try
            {
                ApduCommand command = new ApduCommand();
                command.Deserialize(commandApdu);
                Logger.Log(command.ToString());

                switch (command.INS)
                {
                    case (byte)EMVInstructionEnum.Select:
                        //PPSE or applet select
                        if (Formatting.ByteArrayToHexString(command.CommandData) == PPSE_NAME)
                            return ppseSelectResponse;
                        else if (Formatting.ByteArrayToHexString(command.CommandData) == APPLET_AID)
                            return appletSelectResponse;
                        else
                            return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_INS_NOT_SUPPORTED);

                    case (byte)EMVInstructionEnum.GetProcessingOptions:
                        return DoGPO(commandApdu);


                    default:
                        return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_INS_NOT_SUPPORTED);
                }
            }
            catch(Exception ex)
            {
                return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_INS_NOT_SUPPORTED);
            }
        }

        private byte[] DoGPO(byte[] adpu)
        {
            TLVList db = new TLVList();
            
            EMVGetProcessingOptionsRequest request = new EMVGetProcessingOptionsRequest();
            request.Deserialize(adpu);

            TLV _83 = TLV.Create(EMVTagsEnum.COMMAND_TEMPLATE_83_KRN.Tag);
            _83.Deserialize(request.CommandData, 0);

            int pos = 0;
            byte[] amount = Formatting.copyOfRange(_83.Value, pos, pos + MAX_AMOUNT_AUTH__LENGTH);
            pos = pos + MAX_AMOUNT_AUTH__LENGTH;
            byte[] upn = Formatting.copyOfRange(_83.Value, pos, pos + MAX_UNPRED_NUM__LENGTH);
            pos = pos + MAX_UNPRED_NUM__LENGTH;
            byte[] ttq = Formatting.copyOfRange(_83.Value, pos, pos + MAX_TTQ_LENGTH);
            //pos = pos + MAX_TTQ_LENGTH;

            db.AddToList(TLV.Create(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag, ttq));
            /*
            * supported by card and reader (TTQ byte 1 bit 6 set to 1b)
            * return 6985 if not
            */
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttqST = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(db.Get(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag));
            if (!ttqST.Value.EMVModeSupported)
            {
                return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_CONDITIONS_OF_USE_NOT_SATISFIED);
            }

            /*
            * Card action analysis
            */

            /*
             * Card Risk Management Processing
             */

            /*
             * Initialize data
             */
            //Req H.2 (Initialization of Card Transaction Qualifiers)
            //The card shall reset CTQ byte 1 bits 8-7 to 00b (indicating Online PIN Not Required and Signature Not Required).
            CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3 ctq = new CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3();
            ctq.Value.OnlinePINRequired = false;
            ctq.Value.SignatureRequired = false;

            ctq.Value.GoOnlineIfApplicationExpired = true; //hardcoded perso
            ctq.Value.GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable = true;//hardcoded perso

            //Req H.3 (Initialization of Issuer Application Data)
            //The card shall set the CVR to '03 80 00 00' (indicating Second GENERATE AC not requested)
            byte[] cvr = new byte[4];
            cvr[Byte1] = 0x03;
            cvr[Byte2] = (byte)0x80;
            cvr[Byte3] = 0x00;
            cvr[Byte4] = 0x00;

            //Req H.4 (Initialization of Cryptogram Information Data)
            //The card shall reset the Cryptogram Information Data (CID) to '00'.
            byte[] cid_9F27 = new byte[1];
            cid_9F27[Byte1] = 0x00;

            /*
            * Application Block Check
            */
            //Req H.5 (Application Blocked Check)
            //If the application is blocked, then the card shall discontinue processing the command and shall respond with SW1 SW2 = '6985'

            /*
             * PIN tries exceeded check
             */

            /*
             * Refunds and Credits Check
             */

            /*
             * Reader Indicators Check
             */

            /*
             * Cardholder Verification Method Check
             */

            //Req H.6 (CVM Required Check)
            //If CVM Required by reader (TTQ byte 2 bit 7 is 1b), then a CVM is required for the
            //transaction, and the card shall determine the common CVM to be performed
            byte[] capDefault = PersoAndCardStateStorage.CARD_ADDITIONAL_PROCESSES_9F68_KRN.Value;
            if (ttqST.Value.CVMRequired)
            {
                //Req H.7 (Determine Common CVM)
                //If a CVM is required for the transaction, then the card shall attempt to select a
                //common CVM supported by both itself and the reader, as defined in this requirement.
                //If there is more than one CVM supported by both the card and the reader, the
                //selected CVM is chosen based on the following defined CVM hierarchy: 1) Online
                //PIN, 2) Signature.
                if (isCapPersonalized)
                {
                    //If CVM Required by reader (TTQ byte 2 bit 7 is 1b), then a CVM is required for the
                    //transaction, and the card shall determine the common CVM to be performed.
                    if (ttqST.Value.CVMRequired)
                    {
                        //Online PIN supported by reader (TTQ byte 1 bit 3 is 1b) and either
                        //Online PIN supported by card for domestic transactions (CAP byte 3 bit 8 is 1b)
                        //or Online PIN supported by card for international transactions (CAP byte 3 bit 7 is 1b)
                        if (ttqST.Value.OnlinePINSupported && Formatting.GetBitPosition(capDefault[Byte3], Bit8+1) || Formatting.GetBitPosition(capDefault[Byte3], Bit7+1))
                        {
                            //Then the card shall indicate Online PIN Required (set CTQ byte 1 bit 8 to 1b)
                            ctq.Value.OnlinePINRequired = true;
                            //Else, if both of the following are true
                        }
                        else if (ttqST.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported && Formatting.GetBitPosition(capDefault[Byte3], Bit5+1))
                        {
                            ctq.Value.GoOnlineIfOfflineDataAuthenticationFailsAndReaderIsOnlineCapable = true;
                        }
                        else
                        {
                            return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_DATA_INVALID);
                        }
                    }
                }
                else
                {
                    //if Signature is not supported by the reader (TTQ byte 1 bit 2 is 0b), then the card
                    //shall discontinue processing and respond to the GPO command with SW1 SW2 = '6984'
                    if (!ttqST.Value.SignatureSupported)
                    {
                        return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_DATA_INVALID);
                    }
                }
            }

            /*
            * Domestic Velocity Checking
            */

            /*
             * International Velocity Checking
             */

            /*
             * Contactless Transaction Counter Velocity Checking
             */

            /*
             * Transaction Disposition
             */
            //Req H.8 (Online)
            //Indicate Authorization Request Cryptogram returned (set CVR byte 2 bits 6-5
            //to 10b and set CID bits 8-7 to 10b).
            Formatting.SetBitPosition(ref cvr[Byte2], true, Bit6+1);
            Formatting.SetBitPosition(ref cvr[Byte2], false, Bit5+1);
            Formatting.SetBitPosition(ref cid_9F27[Byte1], true, Bit8+1);
            Formatting.SetBitPosition(ref cid_9F27[Byte1], false, Bit7+1);

            //Increment the Application Transaction Counter (ATC) by one. The ATC shall be
            //incremented prior to the performance of any cryptographic operations.
            //If incrementing the ATC results in the ATC reaching its maximum value, then the
            //application shall be permanently blocked, Req 6.7 (Application Permanently
            //Blocked), and shall respond to the GPO command with error SW1 SW2 = '6985'

            //Increment the Application Transaction Counter (ATC) by one. The ATC shall be
            //incremented prior to the performance of any cryptographic operations
            TLV atc_9F36 = PersoAndCardStateStorage.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN;
            if (atc_9F36.Value[0] == 0xFF)
            {
                return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_CONDITIONS_OF_USE_NOT_SATISFIED);
            }
            else
            {
                if (atc_9F36.Value[1] == 0xFF)
                {
                    atc_9F36.Value[0] = (byte)(atc_9F36.Value[0] + 1);
                    atc_9F36.Value[1] = 0x00;
                }
                else
                {
                    atc_9F36.Value[1] = (byte)(atc_9F36.Value[1] + 1);
                }
            }

            //Construct the Issuer Application Data. If an Issuer Discretionary Data Option (IDD
            //Option) is supported (see Appendix E), it shall be constructed and the MAC
            //generated (if applicable).
            //Only Option 0 supported, IDD already included during perso

            //If the card is capable of performing fDDA and all of the following are true:
            //the card supports fDDA for Online Authorizations (AIP byte 1 bit 6 is 1b for the
            //"Online (with ODA)" GPO response)
            //and ODA for Online Authorizations supported by card (CAP byte 2 bit 6 is 0b)
            //and ODA for Online Authorizations supported by reader (TTQ byte 1 bit 1 is 1b)
            //Then the card shall construct the Card Authentication Related Data and generate
            //the Signed Dynamic Application Data (tag '9F4B'). The Signed Dynamic
            //Application Data shall be generated as defined in Appendix A.
            byte[] iapDefault = PersoAndCardStateStorage.APPLICATION_INTERCHANGE_PROFILE_82_KRN.Value;
            if (Formatting.GetBitPosition(iapDefault[Byte1], Bit6+1) && !Formatting.GetBitPosition(capDefault[Byte2], Bit6+1) &&
                ttqST.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported)
            {
                //fdda not supported
                return BitConverter.GetBytes((int)ISO7816ReturnCodes.SW_CONDITIONS_OF_USE_NOT_SATISFIED);
            }

            //Generate the Application Cryptogram
            //The path shall implement support for Cryptogram Version Number 10 and
            //Cryptogram Version Number 17, and may implement support for Cryptogram Version
            //Number 18 at implementer discretion. The Cryptogram Version to be used for
            //cryptogram generation shall be issuer configurable, and indicated by the issuer in the
            //Cryptogram Version Number of the Issuer Application Data returned for
            //transactions.
            //We use only 17 at this stage
            //if(tm_CVN[Byte1] != 0x11)
            //    ISOException.throwIt((short) 0x6984);

            //9F10 IAD Byte 5 (IAD byte 5 is CVR byte 2) from card
            TLV iad_9F10 = PersoAndCardStateStorage.ISSUER_APPLICATION_DATA_9F10_KRN;
            iad_9F10.Value[Byte5] = cvr[Byte2];

            //9F02 Amount, Authorized from terminal via pdol
            //9F37 Unpredictable Number from terminal via pdol
            //9F36 ATC from card
            //9F10 IAD Byte 5 (IAD byte 5 is CVR byte 2) from card
            byte[] crytogram_9F26 = generateCryptogram17(PersoAndCardStateStorage.ICC_MK,
                Formatting.ConcatArrays(amount, upn, atc_9F36.Value, new byte[] { iad_9F10.Value[Byte5] }));

            //If ODA for Online Authorizations supported by reader (TTQ byte 1 bit 1 is 1b), then
            //construct and send the GPO response in [EMV] Format 2 with the data shown in
            //Table 6-2 using Condition column "Online (with ODA)".
            EMVGetProcessingOptionsResponse response;
            //pdol sent back during select is 9F38 09 -> 9F02 06 9F37 04 9F66 04
            ctq.Serialize();
            db.AddToList(TLV.Create(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag, amount));
            db.AddToList(TLV.Create(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag, upn));
            db.AddToList(TLV.Create(EMVTagsEnum.TERMINAL_TRANSACTION_QUALIFIERS_TTQ_9F66_KRN.Tag, ttq));
            db.AddToList(TLV.Create(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag, ctq.Value.Value));
            db.AddToList(TLV.Create(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag, cid_9F27));
            db.AddToList(TLV.Create(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag, crytogram_9F26));
            TLV sdad = TLV.Create(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag);
            sdad.Val.PackValue(15);
            db.AddToList(sdad);
            db.AddToList(atc_9F36);
            db.AddToList(iad_9F10);

            if (ttqST.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported)
            {
                //only diff is to add empty 9F4B SDAD
                response = genGPOResponse(true, db);
            }
            //Else (TTQ byte 1 bit 1 is 0b), construct and send the GPO response in [EMV]
            //Format 2 with the data shown in Table 6-2 using Condition column "Online and
            //Decline (without ODA)".
            else
            {
                response = genGPOResponse(false, db);
            }
            //Note: The Available Offline Spending Amount (tag '9F5D') is not supported

            return response.Serialize();
        }

        public static byte[] generateCryptogram17(byte[] iccACMasterKey, byte[] data)
        {
            IKey mcAC = JCEHandler.FormDESKey(SMAdapter.LENGTH_DES3_2KEY, iccACMasterKey);
            return EMVDESSecurity.CalculateMACISO9797Alg3(mcAC, data);
        }

        private static EMVGetProcessingOptionsResponse genGPOResponse(bool includeSDAD, TLVList db)
        {
            TLVList response = new TLVList();
            //from perso
            response.AddToList(PersoAndCardStateStorage.APPLICATION_INTERCHANGE_PROFILE_82_KRN);
            response.AddToList(PersoAndCardStateStorage.TRACK_2_EQUIVALENT_DATA_57_KRN);
            response.AddToList(PersoAndCardStateStorage.CARDHOLDER_NAME_5F20_KRN);
            response.AddToList(PersoAndCardStateStorage.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN);
            response.AddToList(PersoAndCardStateStorage.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN);
            response.AddToList(PersoAndCardStateStorage.FORM_FACTOR_INDICATOR_FFI_9F6E_KRN3);
            response.AddToList(PersoAndCardStateStorage.CUSTOMER_EXCLUSIVE_DATA_CED_9F7C_KRN3);

            //from storage and calculated
            response.AddToList(db.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag));

            //calculated
            response.AddToList(db.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag));
            response.AddToList(db.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag));
            response.AddToList(db.Get(EMVTagsEnum.CRYPTOGRAM_INFORMATION_DATA_9F27_KRN.Tag));
            response.AddToList(db.Get(EMVTagsEnum.CARD_TRANSACTION_QUALIFIERS_CTQ_9F6C_KRN3.Tag));
            if (includeSDAD)
                response.AddToList(db.Get(EMVTagsEnum.SIGNED_DYNAMIC_APPLICATION_DATA_9F4B_KRN.Tag));

            TLV _77 = TLV.Create(EMVTagsEnum.RESPONSE_MESSAGE_TEMPLATE_FORMAT_2_77_KRN.Tag, response.Serialize());

            EMVGetProcessingOptionsResponse resp = new EMVGetProcessingOptionsResponse();
            resp.SW1 = 0x90;
            resp.SW2 = 0x00;
            resp.ResponseData = _77.Serialize();
            return resp;
        }
    }
}
