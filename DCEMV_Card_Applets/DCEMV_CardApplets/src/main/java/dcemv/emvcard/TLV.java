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

import javacard.framework.Util;

public class TLV {
//    public static byte[] tlv(byte[] tag, byte[] value){
//        byte[] lengthArray;
//        if(value.length > 127) {
//            lengthArray = new byte[2];
//            Util.setShort(lengthArray,(short)0,(short)value.length);
//            lengthArray = Utils.concatArrays(new byte[]{(byte)(0x80 | 0x02)},lengthArray);
//        }
//        else {
//            lengthArray = new byte[1];
//            lengthArray[0] = (byte) value.length;
//        }
//        return Utils.concatArrays(tag,lengthArray,value);
//    }

    public static short makeLength(short length, byte[] lengthArray){
        if(length > 127) {
            Util.setShort(lengthArray,(short)1,length);
            lengthArray[0] = (byte)(0x80 | 0x02);
            return 3;
        }
        else {
            lengthArray[0] = (byte) length;
            return 1;
        }
    }

    public static short tlv(byte[] tag, byte[] value, byte[] valueLength, byte[] lengthArray, byte[] buffer,short offset){
        short lengthArraySize;
        if(valueLength!=null)
            lengthArraySize = makeLength((short)valueLength[0], lengthArray);
        else
            lengthArraySize = makeLength((short)value.length, lengthArray);
        Util.arrayCopy(tag,(short)0,buffer,offset,(short)tag.length);
        offset = (short)(offset + (short)tag.length);
        Util.arrayCopy(lengthArray,(short)0,buffer,offset,lengthArraySize);
        offset = (short)(offset + lengthArraySize);
        Util.arrayCopy(value,(short)0,buffer,offset,(short)value.length);
        offset = (short)(offset + (short)value.length);
        return offset;
    }

    private static short getTagBytesCount(byte[] aBuf, short aOffset) {
        if((aBuf[aOffset] & 0x1F) == 0x1F) { // see subsequent bytes
            short len = 2;
            for(short i=(short)(aOffset+1); i<(short)(aOffset+10); i++) {
                if( (aBuf[i] & 0x80) != 0x80) {
                    break;
                }
                len++;
            }
            return len;
        } else {
            return 1;
        }
    }
    private static short getLengthBytesCount(byte aBuf[], short aOffset) {
        short len = (short)(aBuf[aOffset] & 0xff);
        if( (len & 0x80) == 0x80) {
            return (short)(1 + (len & 0x7f));
        } else {
            return 1;
        }
    }
    private static short getDataLength(byte[] aBuf, short aOffset) {
        short length = (short)(aBuf[aOffset] & 0xff);
        if((length & 0x80) == 0x80) {
            short numberOfBytes = (short)(length & 0x7f);
            if(numberOfBytes>3) {
                return -1;
                //throw new RuntimeException("data length format error");
            }
            length = 0;
            for(short i=(short)(aOffset+1); i<(short)(aOffset+1+numberOfBytes); i++) {
                length = (short)(length * 0x100 + (aBuf[i] & 0xff));
            }
        }
        return length;
    }
    private static boolean isConstructed(byte firstTagByte) {
        return (firstTagByte & 0x20) != 0;
    }

    public static short findTagValue(byte[] aBuf, short aOffset, byte[] tagToFind, byte[] valueFound, byte[] tagBuffer, byte[] valueFoundLength) {
        while(aOffset < aBuf.length) {
            // tag
            short tagBytesCount = getTagBytesCount(aBuf, aOffset);
            Util.arrayCopyNonAtomic(aBuf,  aOffset, tagBuffer, (short) 0,  tagBytesCount);
            aOffset = (short)(aOffset + tagBytesCount);
            // length
            short lengthBytesCount = getLengthBytesCount(aBuf, aOffset);
            short valueLength = getDataLength(aBuf, aOffset);
            aOffset = (short)(aOffset + lengthBytesCount);

            if (tagToFind.length == tagBytesCount && Util.arrayCompare(tagToFind, (short) 0, tagBuffer, (short) 0, tagBytesCount) == (byte) 0x00) {
                Util.arrayCopyNonAtomic(aBuf,  (aOffset), valueFound, (short) 0,  valueLength);
                aOffset = (short)(aOffset + valueLength);
                if(valueFoundLength!=null){
                    //TODO: this assumes the length is not greater than 255, is this ok?
                    valueFoundLength[0] = (byte)valueLength;
                }
                return aOffset;
            }
            else{
                if (isConstructed(tagBuffer[0])) {
                    short aOffsetStart = aOffset;
                    while (aOffset < (short)(aOffsetStart + valueLength)) {
                        aOffset = findTagValue(aBuf, aOffset, tagToFind, valueFound,tagBuffer,valueFoundLength);
                    }
                }
                else
                    return (short)(aOffset + valueLength);
            }
        }
        return aOffset;
    }
}
