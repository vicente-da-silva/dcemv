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

import com.licel.jcardsim.smartcardio.CardSimulator;
import com.licel.jcardsim.utils.AIDUtil;
import javacard.framework.AID;
import org.junit.Test;


import javax.smartcardio.CommandAPDU;
import javax.smartcardio.ResponseAPDU;

import static junit.framework.Assert.assertEquals;

/**
 * Created by vicen on 2017/10/31.
 */
public class EMVCardTest {
    @Test
    public void Test(){
        // 1. create simulator
        CardSimulator simulator = new CardSimulator();

        // 2. install applet
        AID appletAID = AIDUtil.create("A000000003");
        simulator.installApplet(appletAID, EMVCard.class);

        // 3. select applet
        simulator.selectApplet(appletAID);

        // 4. send APDU
        //CommandAPDU commandAPDU = new CommandAPDU(0x00, 0x01, 0x00, 0x00);
        byte[] test = hexStringToByteArray("80A8000010830E00000000010046C1D93F2700400000");
        CommandAPDU commandAPDU = new CommandAPDU(test);
        ResponseAPDU response = simulator.transmitCommand(commandAPDU);

        // 5. check response
        assertEquals(0x9000, response.getSW());
    }

    public static byte[] hexStringToByteArray(String s) {
        int len = s.length();
        byte[] data = new byte[len / 2];
        for (int i = 0; i < len; i += 2) {
            data[i / 2] = (byte) ((Character.digit(s.charAt(i), 16) << 4)
                    + Character.digit(s.charAt(i+1), 16));
        }
        return data;
    }

    @Test
    public void TestBCD(){
        byte[] bcd = hexStringToByteArray("1234567F");
        byte[] hex = bcdByteToByte(bcd);

        bcd = hexStringToByteArray("12345678");
        hex = bcdByteToByte(bcd);

        hex = hexStringToByteArray("01020304");
        bcd = byteToBCDByte(hex);

        hex = hexStringToByteArray("010203");
        bcd = byteToBCDByte(hex);

        bcd = hexStringToByteArray("1234567890123456789F");
        hex = bcdByteToByte(bcd);

    }
    public static String byteArrayToHexString(byte[] b) {
        String result = "";
        for (int i=0; i < b.length; i++) {
            result +=
                    Integer.toString( ( b[i] & 0xff ) + 0x100, 16).substring( 1 );
        }
        return result;
    }
        public static byte[] byteToBCDByte(byte[] input) {
        short length = 0;
        if((input.length % 2) != 0)
            length = (short)((short)((input.length + 1)) / 2);
        else
            length = (short)(input.length / 2);
        byte[] result = new byte[length];
        short counter = 0;
        for(short i = 0;i<input.length;i=(short)(i+2)){
            if((short)(i+1) <= (short)(input.length-1))
                result[counter] = (byte)((short)((((short)(input[i] & 0x0F) << 4)) | (short)(input[(short)(i+1)])));
            else
                result[counter] = (byte)((short)(((short)((short)((input[i] & 0x0F)) << 4)) | 0x0F));
            counter++;
        }
        return result;
    }
    public static byte[] bcdByteToByte(byte[] input) {
        short length = 0;
        if((input[(short)(input.length-1)] & 0x0F) == 0x0F)
            length = (short)((input.length*2)-1);
        else
            length = (short)(input.length*2);
        byte[] result = new byte[length];
        for(short i = 0;i<input.length;i++){
            result[(short)(i*2)] = (byte)((input[i] & 0xF0) >> 4);
            if((input[i] & 0x0F) != 0x0F)
                result[(short)((i*2)+1)] = (byte)(input[i] & 0x0F);
        }
        return result;
    }
//    @Test
//    public void TestEnc(){
//
////        byte[] c = EMVCard.generateCryptogram17(
////                EMVCard.hexStringToByteArray("B1891A49B2EA69F21245D4A51DD132E24F247FAC6D97F007"),
////                EMVCard.hexStringToByteArray("0000000000000000"));
//
//        EMVCard c = new EMVCard(null,0,0);
//       c.generateCryptogram17(
//                hexStringToByteArray("B5982683AB324FB367C8E5D69B08ECBAB5982683AB324FB3"),
//                hexStringToByteArray("000000000100" + "9264B836" + "0001" + "A0"));
//
//       // String crypto = byteArrayToHexString(c);
//    }

//    @Test
//    public void TestStoreData() {
//        EMVCard e = new EMVCard(null,(short)0,(byte)0);
//        e.doStoreData(hexStringToByteArray("80E2880213" + "010210700E570C1234567890123456D2512201"),
//                (short)5,(short)19);
//
//    }

    @Test
    public void GenPPSEResponse() {
        String aid = "A000000050010101";
        String adfName = "4F" + getLength(aid) + aid;
        String dirEntry = "61" + getLength(adfName) +  adfName;
        String FCIIDD = "BF0C" + getLength(dirEntry) + dirEntry;
        String FCIProp = "A5" + getLength(FCIIDD) + FCIIDD;
        String DFN = "325041592E5359532E4444463031";//2PAY.SYS.DDF01
        String DFNAme = "84" + getLength(DFN) + DFN;
        String FCI = "6F" + getLength(DFNAme + FCIProp) + DFNAme + FCIProp;
        byte[] response = hexStringToByteArray(FCI);
    }

    @Test
    public void GenEMVCardResponse() {
        String df = "54657374417070";
        String DFNAme = "84" + getLength(df) + df;
        String appLabelName = "54657374417070";
        String appLabel = "50" + getLength(appLabelName) + appLabelName;//TestApp
        String pdol = "9F3809" + "9F0206" + "9F3704" + "9F6604";//12
        String FCIProp = "A5" + getLength(appLabel + pdol) + appLabel + pdol;
        String FCI = "6F" + getLength(DFNAme + FCIProp) + DFNAme + FCIProp;

        byte[] response = hexStringToByteArray(FCI);
    }



    private String getLength(String data){
        return byteArrayToHexString(new byte[]{(byte)(data.length()/2)});
    }
}
