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
package dcemv.pse;

import javacard.framework.APDU;
import javacard.framework.Applet;
import javacard.framework.ISO7816;
import javacard.framework.ISOException;

//public class PPSE extends BaseApplet {
public class PPSE extends Applet {

    private final static byte SELECT_PPSE = (byte) 0xA4;

    private final byte[] response = new byte[] {0x6F,0x21,(byte)0x84,0x0e,0x32,0x50,0x41,0x59,0x2E,
            0x53,0x59,0x53,0x2E,0x44,0x44,0x46,0x30,0x31,(byte)0xA5,0x0f,(byte)0xBF,0x0C,
            0x0c,0x61,0x0a,0x4F,0x08,(byte)0xA0,0x00,0x00,0x00,0x50,0x01,0x01,0x01};


    protected PPSE(byte[] bArray, short bOffset, byte bLength) {
        register();
    }


    public static void install(byte[] bArray, short bOffset, byte bLength)
            throws ISOException {
        new PPSE(bArray, bOffset, bLength);
    }

    public void process(APDU apdu) {
        byte[] buffer = apdu.getBuffer();
        // Now determine the requested instruction:
        switch (buffer[ISO7816.OFFSET_INS]) {
            case SELECT_PPSE:
                apdu.setIncomingAndReceive();
                doSelectPPSE(apdu);//select ppse command
                return;

            default:
                // We do not support any other INS values
                ISOException.throwIt(ISO7816.SW_INS_NOT_SUPPORTED);
        }
    }

    private void doSelectPPSE(APDU apdu) {
        apdu.setOutgoing();
        apdu.setOutgoingLength((short)response.length);
        apdu.sendBytesLong(response, (short) 0, (short)response.length);
    }
}