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
package dcemv.gpsim;

import javacard.framework.*;

public class GlobalPlatform extends Applet {
//public class GlobalPlatform extends BaseApplet {

    private final static byte SELECT = (byte) 0xA4;
    private final static byte INSTALL = (byte) 0xE6;
    private final static byte INIT_UPDATE = (byte) 0x50;
    private final static byte EX_AUTH = (byte) 0x82;
    private final static byte STORE_DATA = (byte) 0xE2;

    private byte[] initParamsBytes;
    private final byte[] response = new byte[]{0x00,0x00,0x15,0x16,0x00,0x07,0x00,0x55,0x00,0x3E,(byte)0xFF,0x02,0x00,0x49,0x3B,(byte)0xE0,0x52,(byte)0xB9,(byte)0xC8,(byte)0xE5,0x40,0x03,0x66,(byte)0xA1,0x1C,(byte)0xC5,(byte)0xC9,0x5C};

    protected GlobalPlatform(byte[] bArray, short bOffset, byte bLength) {
        if (bLength > 0) {
            byte iLen = bArray[bOffset]; // aid length
            bOffset = (short) (bOffset + iLen + 1);
            byte cLen = bArray[bOffset]; // info length
            bOffset = (short) (bOffset + 3);
            byte aLen = bArray[bOffset]; // applet data length
            initParamsBytes = new byte[aLen];
            Util.arrayCopyNonAtomic(bArray, (short) (bOffset + 1), initParamsBytes, (short) 0, aLen);
        }
        register();
    }

    public static void install(byte[] bArray, short bOffset, byte bLength)
            throws ISOException {
        new GlobalPlatform(bArray, bOffset, bLength);
    }

    public void process(APDU apdu) {
        byte[] buffer = apdu.getBuffer();
        // Now determine the requested instruction:
        switch (buffer[ISO7816.OFFSET_INS]) {
            case SELECT:
                return;

            case INSTALL:
                return;

            case STORE_DATA:
                return;

            case INIT_UPDATE:
                apdu.setOutgoing();
                apdu.setOutgoingLength((short)response.length);
                apdu.sendBytesLong(response, (short) 0, (short)response.length);
                return;

            case EX_AUTH:
                return;

            default:
                // We do not support any other INS values
                ISOException.throwIt(ISO7816.SW_INS_NOT_SUPPORTED);
        }
    }
}