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
package dcemv.emvcard;

import javacard.framework.*;
import javacard.security.DESKey;
import javacard.security.Key;
import javacard.security.KeyBuilder;
import javacardx.crypto.Cipher;
import org.globalplatform.Application;

//use this when testing via the simulator
//public class EMVCard extends BaseApplet {
//use this when ready to install on card
public class EMVCard extends Applet implements Application {

    final static short Byte1 = 0;
    final static short Byte2 = 1;
    final static short Byte3 = 2;
    final static short Byte4 = 3;
    final static short Byte5 = 4;

    final static short Bit8 = 7;
    final static short Bit7 = 6;
    final static short Bit6 = 5;
    final static short Bit5 = 4;
    final static short Bit4 = 3;
    final static short Bit3 = 2;
    final static short Bit2 = 1;
    final static short Bit1 = 0;

    private final byte[] pdolDefaults = new byte[] {(byte)0x9F,0x38,0x09,(byte)0x9F,0x02,0x06,(byte)0x9F,0x37,0x04,(byte)0x9F,0x66,0x04};
    private final byte[] iapDefault = new byte[]{0x00,0x00};
    private final byte[] cardHolderNameDefault = new byte[]{0x20,0x2F};
    private final byte[] capDefault = new byte[]{0x00,0x60,(byte)0x80,0x00};

//    private final byte[] selectResponse = new byte[]{
//            0x6F,0x20,
//                (byte)0x84,0x07,0x54,0x65,0x73,0x74,0x41,0x70,0x70,
//                (byte)0xA5,0x15,
//                    0x50,0x07,0x54,0x65,0x73,0x74,0x41,0x70,0x70,
//                    (byte)0x9F,0x38,0x09,(byte)0x9F,0x02,0x06,(byte)0x9F,0x37,0x04,(byte)0x9F,0x66,0x04};

    private final byte[] selectResponse = new byte[]{
            0x6F,0x21,
            (byte)0x84,0x08, (byte)0xA0 , 0x00, 0x00, 0x00, 0x50 , 0x01, 0x01, 0x01,
            (byte)0xA5,0x15,
            0x50, 0x07, 0x54, 0x65, 0x73, 0x74, 0x41, 0x70, 0x70,
            (byte)0x9F,0x38,0x09,(byte)0x9F,0x02,0x06,(byte)0x9F,0x37,0x04,(byte)0x9F,0x66,0x04};

    private final static byte GPO = (byte) 0xA8;
    private final static byte SELECT_PPSE = (byte) 0xA4;

    private final byte[] tag_Template_77 = new byte[]{(byte)0x77};
    private final byte[] tag_AIP_82 = new byte[]{(byte)0x82};
    //private final byte[] tag_AFL_94 = new byte[]{(byte)0x94};
    private final byte[] tag_Track2_Equiv_57 = new byte[]{(byte)0x57};
    private final byte[] tag_CardholderName_5F20 = new byte[]{(byte)0x5F,(byte)0x20};
    private final byte[] tag_PSN_5F34 = new byte[]{(byte)0x5F,(byte)0x34};
    private final byte[] tag_IAD_9F10 = new byte[]{(byte)0x9F,(byte)0x10};
    private final byte[] tag_AC_9F26 = new byte[]{(byte)0x9F,(byte)0x26};
    private final byte[] tag_CID_9F27 = new byte[]{(byte)0x9F,(byte)0x27};
    private final byte[] tag_ATC_9F36 = new byte[]{(byte)0x9F,(byte)0x36};
    private final byte[] tag_SDAD_9F4B = new byte[]{(byte)0x9F,(byte)0x4B};
    private final byte[] tag_CTQ_9F6C = new byte[]{(byte)0x9F,(byte)0x6C};
    private final byte[] tag_FFI_9F6E = new byte[]{(byte)0x9F,(byte)0x6E};
    private final byte[] tag_CED_9F7C = new byte[]{(byte)0x9F,(byte)0x7C};
    private final byte[] tag_PAN_5A = new byte[]{(byte)0x5A};

    /*
     *Terminal Supplied fields
     */
    private final short MAX_TTQ_LENGTH = 4;
    private final short MAX_UNPRED_NUM__LENGTH = 4;
    private final short MAX_AMOUNT_AUTH__LENGTH = 6;

    private final byte[] tm_TTQ_9F66;//Terminal Transaction Qualifiers
    private final byte[] tm_Amount_9F02;//Amount Authorized
    private final byte[] tm_UnpredicableNumber_9F37;
    /*
     * PERSO fields
     */
    private final short MAX_PDOL_LENGTH = 12;
    private final short MAX_CAP_LENGTH = 4;
    //private final short MAX_IDD_LENGTH = 15;
    private final short MAX_IAP_LENGTH = 2;
    //private final short MAX_CVN_LENGTH = 1;
    private final short MAX_PSN_LENGTH = 1;
    private final short MAX_ICC_AC_MASTER_KEY_LENGTH = 16;
    private final short MAX_PAN_LENGTH = 10;
    private final short MAX_CARDHOLDER_NAME_LENGTH = 2;
    //private final short MAX_EXPIRY_DATE_LENGTH = 2;

    private final byte[] tm_PDOL_9F38;//Processing Options Data Object List
    private final byte[] tm_CAP_9F68;//Card Additional Processes
    //private final byte[] tm_IDD;//Issuer Discretionary Data (part of IAD)
    private final byte[] tm_IAP_82;//Issuer Application Profile
    //private final byte[] tm_CVN;//Cryptogram Version Number
    private final byte[] tm_ICCACMasterKey; //AC Master Key
    private final byte[] tm_PAN_5A; //Primary Account Number
    //private final byte[] tm_ExpiryDate_YYMM; //Primary Account Number
    private final byte[] tm_Cardholder_Name_5F20; //Cardholder Name
    private final byte[] tm_PanSequenceNumber_5F34;//Pan Sequence Number

    private final byte[] tm_PAN_5A_Length;//Pan Sequence Number Actual Length
    private final byte[] tm_Track2_Equivalent_57_Length;
    /*
     * GPO Calculated Response fields
     */
    //private final short MAX_AIP_LENGTH = 2;
    private final short MAX_AFL_LENGTH = 2;
    private final short MAX_TRACK2_LENGTH = 19;
    private final short MAX_IAD_LENGTH = 32;
    private final short MAX_CVR_LENGTH = 4;
    private final short MAX_AC_LENGTH = 8;
    private final short MAX_CID_LENGTH = 1;
    private final short MAX_ATC_LENGTH = 2;
    private final short MAX_SDAD_LENGTH = 15;
    private final short MAX_CTQ_LENGTH = 2;
    private final short MAX_FFI_LENGTH = 4;
    private final short MAX_CED_LENGTH = 32;

    //private final byte[] tm_AIP_82;//Application Interchange Profile
    //private final byte[] tm_AFL_94;//Application File Locator
    private final byte[] tm_Track2_Equivalent_57; //Track 2 Equivalent Data
    private final byte[] tm_IAD_9F10;//Issuer Application Data (Only option 0 is implemented, since offline is not implemented and Issuer Update Processing is not supported)
    private final byte[] tm_CVR;//Card Verification Results
    private final byte[] tm_AC_9F26;//Application Cryptogram
    private final byte[] tm_CID_9F27;//Cryptogram Information Data
    private final byte[] tm_ATC_9F36;//Application Transaction Counter
    private final byte[] tm_SDAD_9F4B;//Cryptogram Information Data
    private final byte[] tm_CTQ_9F6C;//Card Transaction Qualifiers
    private final byte[] tm_FFI_9F6E;//Form Factor Indicator
    private final byte[] tm_CED_9F7C;//Customer Exclusive Data

    private final static boolean isCapPersonalized = true;

    /**
     * Variables used in local methods
     */
    private final byte[] cryptoInput;
    private final byte[] gpoResponse;
    private final byte[] lengthArray;
    private final byte[] tagBuffer;

    public EMVCard(byte[] bArray, short bOffset, byte bLength) {
        tagBuffer = JCSystem.makeTransientByteArray((short)4,JCSystem.CLEAR_ON_RESET);//buffers called by store data cannot be clear on deselect

        short cryptoInputLenghth = (short)(MAX_AMOUNT_AUTH__LENGTH + MAX_UNPRED_NUM__LENGTH + MAX_ATC_LENGTH + (short)1);
        cryptoInputLenghth = (short)(cryptoInputLenghth + (8 - (cryptoInputLenghth % 8)));

        cryptoInput = JCSystem.makeTransientByteArray(cryptoInputLenghth,JCSystem.CLEAR_ON_DESELECT);

        short responseLengthMax =
                (short)(
                        (short)4 + //0x77 and a max length of length of 3
                                (short)tag_AIP_82.length + (short)1 + MAX_IAP_LENGTH +
                                (short)tag_Track2_Equiv_57.length + (short)1 + MAX_TRACK2_LENGTH +
                                (short)tag_CardholderName_5F20.length + (short)1 + MAX_CARDHOLDER_NAME_LENGTH +
                                (short)tag_PAN_5A.length + (short)1 + MAX_PAN_LENGTH +
                                (short)tag_PSN_5F34.length + (short)1 + MAX_PSN_LENGTH +
                                (short)tag_IAD_9F10.length + (short)1 + MAX_IAD_LENGTH +
                                (short)tag_AC_9F26.length + (short)1 + MAX_AC_LENGTH +
                                (short)tag_CID_9F27.length + (short)1 + MAX_CID_LENGTH +
                                (short)tag_ATC_9F36.length + (short)1 + MAX_ATC_LENGTH +
                                (short)tag_SDAD_9F4B.length + (short)1 + MAX_SDAD_LENGTH +
                                (short)tag_CTQ_9F6C.length + (short)1 + MAX_CTQ_LENGTH +
                                (short)tag_FFI_9F6E.length + (short)1 + MAX_FFI_LENGTH +
                                (short)tag_CED_9F7C.length + (short)1 + MAX_CED_LENGTH
                );
        gpoResponse = JCSystem.makeTransientByteArray(responseLengthMax , JCSystem.CLEAR_ON_DESELECT);
        lengthArray = JCSystem.makeTransientByteArray((short)3, JCSystem.CLEAR_ON_DESELECT);

        //calculated, transient
        //tm_IDD = JCSystem.makeTransientByteArray(MAX_IDD_LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_IAD_9F10 = JCSystem.makeTransientByteArray(MAX_IAD_LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_CVR = JCSystem.makeTransientByteArray(MAX_CVR_LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_CID_9F27 = JCSystem.makeTransientByteArray(MAX_CID_LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_AC_9F26 = JCSystem.makeTransientByteArray(MAX_AC_LENGTH, JCSystem.CLEAR_ON_DESELECT);

        //terminal provided, transient
        tm_Amount_9F02 = JCSystem.makeTransientByteArray(MAX_AMOUNT_AUTH__LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_UnpredicableNumber_9F37 = JCSystem.makeTransientByteArray(MAX_UNPRED_NUM__LENGTH, JCSystem.CLEAR_ON_DESELECT);
        tm_TTQ_9F66 = JCSystem.makeTransientByteArray(MAX_TTQ_LENGTH, JCSystem.CLEAR_ON_DESELECT);

        //calculated, not transient
        tm_ATC_9F36 = makePersistentArray(MAX_ATC_LENGTH);

        //hardcoded perso arrays, not transient
        //tm_AFL_94 = makePersistentArray(MAX_AFL_LENGTH);
        tm_PDOL_9F38 = makePersistentArray(MAX_PDOL_LENGTH);
        tm_IAP_82 = makePersistentArray(MAX_IAP_LENGTH);
        tm_Cardholder_Name_5F20= makePersistentArray(MAX_CARDHOLDER_NAME_LENGTH);
        tm_CTQ_9F6C = makePersistentArray(MAX_CTQ_LENGTH);
        tm_SDAD_9F4B = makePersistentArray(MAX_SDAD_LENGTH);
        tm_CAP_9F68 = makePersistentArray(MAX_CAP_LENGTH);
        tm_FFI_9F6E = makePersistentArray(MAX_FFI_LENGTH);
        tm_CED_9F7C = makePersistentArray(MAX_CED_LENGTH);

        //perso arrays, not transient
        tm_ICCACMasterKey = makePersistentArray(MAX_ICC_AC_MASTER_KEY_LENGTH);
        tm_PAN_5A = makePersistentArray(MAX_PAN_LENGTH);
        tm_PanSequenceNumber_5F34 = makePersistentArray(MAX_PSN_LENGTH);
        tm_Track2_Equivalent_57= makePersistentArray(MAX_TRACK2_LENGTH);

        tm_Track2_Equivalent_57_Length = makePersistentArray((short)1);
        tm_PAN_5A_Length = makePersistentArray((short)1);
        register();

        doHardPerso();
        //comment the following line of code out for cap building
        //its only needed for running the applet in the emulator
        doTestPerso();
    }

    private byte[] makePersistentArray(short length){
        return new byte[length];
    }

    public static void install(byte[] bArray, short bOffset, byte bLength) throws ISOException {
        new EMVCard(bArray, bOffset, bLength);
    }

    /**
     * This method is called each time the applet receives an APDU.
     */
    public void process(APDU apdu) {
        apdu.setIncomingAndReceive();
        // Now determine the requested instruction:
        switch (apdu.getBuffer()[ISO7816.OFFSET_INS]) {

            case SELECT_PPSE:
                doSelectApp(apdu);
                return;

            case GPO:
                doProcessADPU(apdu);
                return;

            default:
                // We do not support any other INS values
                ISOException.throwIt(ISO7816.SW_INS_NOT_SUPPORTED);
        }
    }

    private void doStoreData(byte[] baBuffer, short sOffset, short sLength) {
        //MAC not supported
        if(baBuffer[(short)(0+sOffset)] == (byte)0x80 && baBuffer[(short)(1+sOffset)] == (byte)0x01){//data is key
            //data is DES key
            Util.arrayCopyNonAtomic(baBuffer,(short)(sOffset + (short)3),tm_ICCACMasterKey,(short)0,MAX_ICC_AC_MASTER_KEY_LENGTH);
        }
        else if(baBuffer[(short)(0+sOffset)] == (byte)0x01 && baBuffer[(short)(1+sOffset)] == (byte)0x01){//data is TLV List
            //PAN, PSN
            TLV.findTagValue(baBuffer,(short)(sOffset + (short)3),tag_PSN_5F34,tm_PanSequenceNumber_5F34,tagBuffer, null);
            TLV.findTagValue(baBuffer,(short)(sOffset + (short)3),tag_PAN_5A,tm_PAN_5A,tagBuffer, tm_PAN_5A_Length);
        }
        else if(baBuffer[(short)(0+sOffset)] == (byte)0x01 && baBuffer[(short)(1+sOffset)] == (byte)0x02){//data is TLV List
            //Track 2 Equivalent Data
            TLV.findTagValue(baBuffer,(short)(sOffset + (short)3),tag_Track2_Equiv_57,tm_Track2_Equivalent_57,tagBuffer,tm_Track2_Equivalent_57_Length);
        }
        else
            ISOException.throwIt(ISO7816.SW_FILE_NOT_FOUND);
    }

    private void doHardPerso(){
        //TODO: this will be moved to perso
        //Cryptogram version number, will not be persoed as the card currently only supports 17
        //Util.arrayCopyNonAtomic(Utils.hexStringToByteArray("11"),(short)0,tm_CVN,(short)0,MAX_CVN_LENGTH);
        Util.arrayCopyNonAtomic(pdolDefaults,(short)0,tm_PDOL_9F38,(short)0,MAX_PDOL_LENGTH);
        Util.arrayCopyNonAtomic(iapDefault,(short)0,tm_IAP_82,(short)0,MAX_IAP_LENGTH);//no msd, dda not supported
        Util.arrayCopyNonAtomic(cardHolderNameDefault,(short)0,tm_Cardholder_Name_5F20,(short)0,MAX_CARDHOLDER_NAME_LENGTH);
        //will be used for card serial number
        //Util.arrayCopyNonAtomic(Utils.hexStringToByteArray("112233445566778899001122334455"),(short)0,tm_IDD,(short)0,MAX_IDD_LENGTH);
        tm_CTQ_9F6C[Byte1] = Utils.SetBit(tm_CTQ_9F6C[Byte1],Bit4); //go online if app expired
        tm_CTQ_9F6C[Byte1] = Utils.SetBit(tm_CTQ_9F6C[Byte1],Bit6); //go online id oda fails
        Util.arrayCopyNonAtomic(capDefault,(short)0,tm_CAP_9F68,(short)0,MAX_CAP_LENGTH);
    }

    //do not use this kind of memory allocation code for card apps processing as it allocates memory on the card
    //and eventually the card will run out of memory
    private void doTestPerso(){
        //with emv demo app which uses the hsm
        //8CB9F7D54362B0A240D0AE626780A86B
        //with simulated provider, standard master ac test key -> 2315208C9110AD402315208C9110AD40 -> derives this key for this pan and this pan sequence number
        //3DAD707C897F49895D6D62526297497A
        Util.arrayCopyNonAtomic(Utils.hexStringToByteArray("3DAD707C897F49895D6D62526297497A"),(short)0,tm_ICCACMasterKey,(short)0,MAX_ICC_AC_MASTER_KEY_LENGTH);
        byte[] panBytes = Utils.hexStringToByteArray("1234567890123456");
        tm_PAN_5A_Length[0] = (byte)panBytes.length;
        Util.arrayCopyNonAtomic(panBytes,(short)0, tm_PAN_5A,(short)0,tm_PAN_5A_Length[0]);
        Util.arrayCopyNonAtomic(Utils.hexStringToByteArray("01"),(short)0,tm_PanSequenceNumber_5F34,(short)0,MAX_PSN_LENGTH);
        byte[] t2 = calcTrack2("2501");
        tm_Track2_Equivalent_57_Length[0] = (byte)t2.length;
        Util.arrayCopyNonAtomic(t2,(short)0,tm_Track2_Equivalent_57,(short)0,(short)tm_Track2_Equivalent_57_Length[0]);
    }
    private byte[] calcTrack2(String expDate){
        byte[] separator = new byte[]{'D'};
        byte[] serviceCode = new byte[] {0x02 , 0x00, 0x01};
        byte[] panHex = Utils.bcdByteToByte(tm_PAN_5A);
        byte[] expiryHex = Utils.bcdByteToByte(Utils.hexStringToByteArray(expDate));
        byte[] source = Utils.byteToBCDByte(Utils.concatArrays(panHex,separator,expiryHex,serviceCode));
        Util.arrayCopy(source,(short)0,tm_Track2_Equivalent_57,(short)0,(short)source.length);
        return tm_Track2_Equivalent_57;
    }

    private void doSelectApp(APDU apdu) {
        apdu.setOutgoing();
        apdu.setOutgoingLength((short)selectResponse.length);
        apdu.sendBytesLong(selectResponse, (short) 0, (short)selectResponse.length);
    }

    private void doProcessADPU(APDU apdu) {
        byte[] buffer = apdu.getBuffer();

        tm_IAD_9F10[0] = 0x06;//format
        tm_IAD_9F10[1] = 0x00;//kdi
        tm_IAD_9F10[2] = 0x11;//cvn , crypto 17

        tm_IAD_9F10[3] = 0x00;//cvr byte 1
        tm_IAD_9F10[4] = 0x00;//cvr byte 2
        tm_IAD_9F10[5] = 0x00;//cvr byte 3
        tm_IAD_9F10[6] = 0x00;//cvr byte 4

        short pos = ISO7816.OFFSET_CDATA + 2;
        Util.arrayCopyNonAtomic(buffer, pos, tm_Amount_9F02, (short) 0, MAX_AMOUNT_AUTH__LENGTH);
        pos = (short)(pos + MAX_AMOUNT_AUTH__LENGTH);
        Util.arrayCopyNonAtomic(buffer, pos, tm_UnpredicableNumber_9F37, (short) 0, MAX_UNPRED_NUM__LENGTH);
        pos = (short)(pos + MAX_UNPRED_NUM__LENGTH);
        Util.arrayCopyNonAtomic(buffer, pos, tm_TTQ_9F66, (short) 0, MAX_TTQ_LENGTH);
        //pos = pos + MAX_TTQ_LENGTH;

        /*
         * supported by card and reader (TTQ byte 1 bit 6 set to 1b)
         * return 6985 if not
         */
        if(!Utils.IsBitSet(tm_TTQ_9F66[Byte1], Bit6)) {
            ISOException.throwIt((short) 0x6985);
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
        tm_CTQ_9F6C[Byte1] = Utils.ClearBit(tm_CTQ_9F6C[Byte1],Bit8);
        tm_CTQ_9F6C[Byte1] = Utils.ClearBit(tm_CTQ_9F6C[Byte1],Bit7);
        //Req H.3 (Initialization of Issuer Application Data)
        //The card shall set the CVR to '03 80 00 00' (indicating Second GENERATE AC not requested)
        tm_CVR[Byte1] = 0x03;
        tm_CVR[Byte2] = (byte)0x80;
        tm_CVR[Byte3] = 0x00;
        tm_CVR[Byte4] = 0x00;
        //Req H.4 (Initialization of Cryptogram Information Data)
        //The card shall reset the Cryptogram Information Data (CID) to '00'.
        tm_CID_9F27[Byte1] = 0x00;

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
        if(Utils.IsBitSet(tm_TTQ_9F66[Byte2], Bit7)) {
            //Req H.7 (Determine Common CVM)
            //If a CVM is required for the transaction, then the card shall attempt to select a
            //common CVM supported by both itself and the reader, as defined in this requirement.
            //If there is more than one CVM supported by both the card and the reader, the
            //selected CVM is chosen based on the following defined CVM hierarchy: 1) Online
            //PIN, 2) Signature.
            if (isCapPersonalized) {
                //If CVM Required by reader (TTQ byte 2 bit 7 is 1b), then a CVM is required for the
                //transaction, and the card shall determine the common CVM to be performed.
                if (Utils.IsBitSet(tm_TTQ_9F66[Byte2], Bit7)) {
                    //Online PIN supported by reader (TTQ byte 1 bit 3 is 1b) and either
                    //Online PIN supported by card for domestic transactions (CAP byte 3 bit 8 is 1b)
                    //or Online PIN supported by card for international transactions (CAP byte 3 bit 7 is 1b)
                    if (Utils.IsBitSet(tm_TTQ_9F66[Byte1], Bit3) && (Utils.IsBitSet(tm_CAP_9F68[Byte3], Bit8) || Utils.IsBitSet(tm_CAP_9F68[Byte3], Bit7))) {
                        //Then the card shall indicate Online PIN Required (set CTQ byte 1 bit 8 to 1b)
                        tm_CTQ_9F6C[Byte1] = Utils.SetBit(tm_CTQ_9F6C[Byte1], Bit8);
                        //Else, if both of the following are true
                    } else if (Utils.IsBitSet(tm_TTQ_9F66[Byte1], Bit2) && Utils.IsBitSet(tm_CAP_9F68[Byte3], Bit5)) {
                        Utils.SetBit(tm_CTQ_9F6C[Byte1], Bit7);
                    } else {
                        ISOException.throwIt((short) 0x6984);
                    }
                }
            } else {
                //if Signature is not supported by the reader (TTQ byte 1 bit 2 is 0b), then the card
                //shall discontinue processing and respond to the GPO command with SW1 SW2 = '6984'
                if (Utils.IsBitClear(tm_TTQ_9F66[Byte1], Bit2)) {
                    ISOException.throwIt((short) 0x6984);
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
        tm_CVR[Byte2] = Utils.SetBit(tm_CVR[Byte2],Bit6);
        tm_CVR[Byte2] = Utils.ClearBit(tm_CVR[Byte2],Bit5);
        tm_CID_9F27[Byte1] = Utils.SetBit(tm_CID_9F27[Byte1],Bit8);
        tm_CID_9F27[Byte1] = Utils.ClearBit(tm_CID_9F27[Byte1],Bit7);

        //Increment the Application Transaction Counter (ATC) by one. The ATC shall be
        //incremented prior to the performance of any cryptographic operations.
        //If incrementing the ATC results in the ATC reaching its maximum value, then the
        //application shall be permanently blocked, Req 6.7 (Application Permanently
        //Blocked), and shall respond to the GPO command with error SW1 SW2 = '6985'

        //Increment the Application Transaction Counter (ATC) by one. The ATC shall be
        //incremented prior to the performance of any cryptographic operations
        if(tm_ATC_9F36[0] == 0xFF){
            ISOException.throwIt((short) 0x6985);
        }
        else{
            if(tm_ATC_9F36[1] == 0xFF){
                tm_ATC_9F36[0] = (byte)(tm_ATC_9F36[0] + 1);
                tm_ATC_9F36[1] = 0x00;
            }else{
                tm_ATC_9F36[1] = (byte)(tm_ATC_9F36[1] + 1);
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
        if(Utils.IsBitSet(tm_IAP_82[Byte1],Bit6) && Utils.IsBitClear(tm_CAP_9F68[Byte2],Bit6) && Utils.IsBitSet(tm_TTQ_9F66[Byte1],Bit1)){
            //fdda not supported
            ISOException.throwIt((short) 0x6985);
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
        tm_IAD_9F10[Byte5] = tm_CVR[Byte2];

        //9F02 Amount, Authorized from terminal via pdol
        //9F37 Unpredictable Number from terminal via pdol
        //9F36 ATC from card
        //9F10 IAD Byte 5 (IAD byte 5 is CVR byte 2) from card
        short posCrypto = 0;
        posCrypto = Util.arrayCopy(tm_Amount_9F02,(short)0, cryptoInput,posCrypto,MAX_AMOUNT_AUTH__LENGTH);
        posCrypto = Util.arrayCopy(tm_UnpredicableNumber_9F37,(short)0, cryptoInput,posCrypto,MAX_UNPRED_NUM__LENGTH);
        posCrypto = Util.arrayCopy(tm_ATC_9F36,(short)0, cryptoInput,posCrypto,MAX_ATC_LENGTH);
        cryptoInput[posCrypto] = tm_IAD_9F10[Byte5];
        generateCryptogram17(tm_ICCACMasterKey,cryptoInput);

        //endProcessing(apdu,new byte[]{(byte)0x80,0x02,tm_ATC_9F36[0],tm_ATC_9F36[1]});
        //return;

        //If ODA for Online Authorizations supported by reader (TTQ byte 1 bit 1 is 1b), then
        //construct and send the GPO response in [EMV] Format 2 with the data shown in
        //Table 6-2 using Condition column "Online (with ODA)".
        short responseLength = 0;
        if(Utils.IsBitSet(tm_TTQ_9F66[Byte1],Bit1)){
            //only diff is to add empty 9F4B SDAD
            responseLength = genGPOResponse(true);
        }
        //Else (TTQ byte 1 bit 1 is 0b), construct and send the GPO response in [EMV]
        //Format 2 with the data shown in Table 6-2 using Condition column "Online and
        //Decline (without ODA)".
        else{
            responseLength = genGPOResponse(false);
        }
        //Note: The Available Offline Spending Amount (tag '9F5D') is not supported

        endProcessing(apdu,responseLength);
    }

    private void endProcessing(APDU apdu, short responseLength){
        apdu.setOutgoing();
        apdu.setOutgoingLength(responseLength);
        apdu.sendBytesLong(gpoResponse, (short) 0, responseLength);
    }

    private short genGPOResponse(boolean includeSDAD){
        //Format 2: The data object returned in the response message is a constructed
        //data object with tag equal to '77'. The value field may contain several BERTLV coded objects,
        // but shall always include the AIP and the AFL.

        //assumption here that none of these tags are greater than 1 byte lengths
        short sdadLength = includeSDAD ? (short)((short)tag_SDAD_9F4B.length + (short)1 + (short)tm_SDAD_9F4B.length) : 0;
        short responseLength =
                (short)(
                        (short)tag_AIP_82.length + (short)1 + (short)tm_IAP_82.length +
                                (short)tag_Track2_Equiv_57.length + (short)1 + tm_Track2_Equivalent_57_Length[0] +
                                (short)tag_CardholderName_5F20.length + (short)1 + tm_Cardholder_Name_5F20.length +
                                (short)tag_PAN_5A.length+ (short)1 + tm_PAN_5A_Length[0] +
                                (short)tag_PSN_5F34.length+ (short)1 + tm_PanSequenceNumber_5F34.length +
                                (short)tag_IAD_9F10.length+ (short)1 + tm_IAD_9F10.length +
                                (short)tag_AC_9F26.length+ (short)1 + tm_AC_9F26.length +
                                (short)tag_CID_9F27.length+ (short)1 + tm_CID_9F27.length +
                                (short)tag_ATC_9F36.length+ (short)1 + tm_ATC_9F36.length +
                                sdadLength +
                                (short)tag_CTQ_9F6C.length+ (short)1 + tm_CTQ_9F6C.length +
                                (short)tag_FFI_9F6E.length+ (short)1 + tm_FFI_9F6E.length +
                                (short)tag_CED_9F7C.length+ (short)1 + tm_CED_9F7C.length
                );

        short pos = 0;
        short lengthArraySize = TLV.makeLength(responseLength, lengthArray);
        responseLength = (short)(responseLength + lengthArraySize + (short)1); //0x77

        gpoResponse[0] = 0x77;
        pos++;
        Util.arrayCopy(lengthArray,(short)0,gpoResponse,pos,lengthArraySize);
        pos = (short)(pos + lengthArraySize);

        pos = TLV.tlv(tag_AIP_82,tm_IAP_82,null,lengthArray,gpoResponse,pos) ;
        TLV.tlv(tag_Track2_Equiv_57,tm_Track2_Equivalent_57,tm_Track2_Equivalent_57_Length,lengthArray,gpoResponse,pos) ;
        pos = (short)((short)1+ (short)1 + (short)(pos + tm_Track2_Equivalent_57_Length[0]));
        pos = TLV.tlv(tag_CardholderName_5F20,tm_Cardholder_Name_5F20,null,lengthArray,gpoResponse,pos) ;
        TLV.tlv(tag_PAN_5A,tm_PAN_5A,tm_PAN_5A_Length,lengthArray,gpoResponse,pos) ;
        pos = (short)((short)1+ (short)1 + (short)(pos + tm_PAN_5A_Length[0]));
        pos = TLV.tlv(tag_PSN_5F34,tm_PanSequenceNumber_5F34,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_IAD_9F10,tm_IAD_9F10,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_AC_9F26,tm_AC_9F26,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_CID_9F27,tm_CID_9F27,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_ATC_9F36,tm_ATC_9F36,null,lengthArray,gpoResponse,pos) ;
        if(includeSDAD)
            pos = TLV.tlv(tag_SDAD_9F4B,tm_SDAD_9F4B,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_CTQ_9F6C,tm_CTQ_9F6C,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_FFI_9F6E,tm_FFI_9F6E,null,lengthArray,gpoResponse,pos) ;
        pos = TLV.tlv(tag_CED_9F7C,tm_CED_9F7C,null,lengthArray,gpoResponse,pos) ;

        return responseLength;
    }

    public void generateCryptogram17(byte[] iccACMasterKey, byte[] d){
        iso9797Alg3NoMethod2Pad(iccACMasterKey,d);
    }

    public void iso9797Alg3NoMethod2Pad(byte[] iccACMasterKey, byte[] d){
        //already padded with 00 during buffer allocation
        Cipher engine = Cipher.getInstance(Cipher.ALG_DES_ECB_NOPAD, false);
        Key left = KeyBuilder.buildKey(KeyBuilder.TYPE_DES,KeyBuilder.LENGTH_DES,false);
        Key right = KeyBuilder.buildKey(KeyBuilder.TYPE_DES,KeyBuilder.LENGTH_DES,false);

        byte[] leftBytes = JCSystem.makeTransientByteArray((short)8, JCSystem.CLEAR_ON_DESELECT);
        byte[] rightBytes = JCSystem.makeTransientByteArray((short)8, JCSystem.CLEAR_ON_DESELECT);

        Util.arrayCopy(iccACMasterKey,(short)0,leftBytes,(short)0,(short)8);
        Util.arrayCopy(iccACMasterKey,(short)8,rightBytes,(short)0,(short)8);

        ((DESKey)left).setKey(leftBytes,(short)0);
        ((DESKey)right).setKey(rightBytes,(short)0);

        byte[] yi = JCSystem.makeTransientByteArray((short)8, JCSystem.CLEAR_ON_DESELECT);
        byte[] y_i = JCSystem.makeTransientByteArray((short)8, JCSystem.CLEAR_ON_DESELECT);
        engine.init(left,Cipher.MODE_ENCRYPT);
        for (short i = 0; i < d.length; i += 8)
        {
            Util.arrayCopy(d, i, yi, (short)0, (short)yi.length);
            engine.doFinal(Utils.Xor(yi, y_i),(short)0,(short)8,y_i,(short)0);
        }
        engine.init(right,Cipher.MODE_DECRYPT);
        engine.doFinal(y_i, (short)0,(short)8,y_i,(short)0);
        engine.init(left,Cipher.MODE_ENCRYPT);
        engine.doFinal(y_i, (short)0,(short)8,y_i, (short)0);
        Util.arrayCopy(y_i,(short)0,tm_AC_9F26,(short)0,(short)8);
    }

//    public void iso9797Alg3(byte[] iccACMasterKey, byte[] data){
//        Key key = KeyBuilder.buildKey(
//                KeyBuilder.TYPE_DES,
//                //JCSystem.MEMORY_TYPE_PERSISTENT,
//                KeyBuilder.LENGTH_DES3_2KEY,
//                false);
//        ((DESKey)key).setKey(iccACMasterKey,(short)0);
//        Signature engine = Signature.getInstance(Signature.ALG_DES_MAC8_ISO9797_1_M2_ALG3, false);
//        signData(engine, key, null, data);
//    }
//    private void signData(Signature engine, Key key, byte[] iv, byte[] msg) {
//        if (iv == null) {
//            engine.init(key, Signature.MODE_SIGN);
//        } else {
//            engine.init(key, Signature.MODE_SIGN, iv, (short) 0, (short) iv.length);
//        }
//        engine.getPaddingAlgorithm();
//        engine.sign(msg, (short) 0, (short) msg.length, tm_AC_9F26, (short) 0);
//    }

    public void processData(byte[] baBuffer, short sOffset, short sLength) {
        doStoreData(baBuffer, sOffset, sLength);
    }

}