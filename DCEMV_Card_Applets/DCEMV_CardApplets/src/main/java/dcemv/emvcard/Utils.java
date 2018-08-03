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

import javacard.framework.JCSystem;
import javacard.framework.Util;

public class Utils {

    public static boolean IsBitClear(byte val, short bit){
        return ((short)(val & (1 << bit))) == 0;
    }
    public static boolean IsBitSet(byte val, short bit){
        return ((short)(val & (1 << bit))) != 0;
    }
    public static byte SetBit(byte val, short bit){
        return  (byte)(val | (1 << bit));
    }
    public static byte ClearBit(byte val, short bit){
        return (byte)(val & ~(1 << bit));
    }
    public static byte[] Xor(byte[] op1, byte[] op2){
        byte[] result;
        // Use the smallest array
        if (op2.length > op1.length)
        {
            result = JCSystem.makeTransientByteArray((short)op1.length, JCSystem.CLEAR_ON_DESELECT);
        }
        else
        {
            result = JCSystem.makeTransientByteArray((short)op2.length, JCSystem.CLEAR_ON_DESELECT);
        }
        for (short i = 0; i < result.length; i++)
        {
            result[i] = (byte)(op1[i] ^ op2[i]);
        }
        return result;
    }

//    code for testing only, do not use in non test perso code
//    uncomment this code for running the applet in the emulator
    public static byte[] hexStringToByteArray(String s) {
        int len = s.length();
        byte[] data = new byte[len / 2];
        for (int i = 0; i < len; i += 2) {
            data[i / 2] = (byte) ((Character.digit(s.charAt(i), 16) << 4)
                    + Character.digit(s.charAt(i+1), 16));
        }
        return data;
    }
    public static byte[] bcdByteToByte(byte[] input) {
        int length = 0;
        if((input[input.length-1] & 0x0F) == 0x0F)
            length = (input.length*2)-1;
        else
            length = input.length*2;
        byte[] result = new byte[length];
        for(int i = 0;i<input.length;i++){
            result[i*2] = (byte)((input[i] & 0xF0) >> 4);
            if((input[i] & 0x0F) != 0x0F)
                result[(i*2)+1] = (byte)(input[i] & 0x0F);
        }
        return result;
    }
    public static byte[] byteToBCDByte(byte[] input) {
        int length = 0;
        if((input.length % 2) != 0)
            length = (input.length + 1) / 2;
        else
            length = input.length / 2;
        byte[] result = new byte[length];
        int counter = 0;
        for(int i = 0;i<input.length;i=i+2){
            if(i+1 <= input.length-1)
                result[counter] = (byte)(((input[i] & 0x0F) << 4) | input[i+1]);
            else
                result[counter] = (byte)(((input[i] & 0x0F) << 4) | 0x0F);
            counter++;
        }
        return result;
    }
    public static byte[] concatArrays(byte[] array1, byte[] array2,byte[] array3, byte[] array4){
        byte[] concatArray = new byte[(short)((short)array1.length + (short)array2.length + (short)array3.length + (short)array4.length)];
        Util.arrayCopy(array1, (short)0, concatArray, (short)0, (short)array1.length);
        Util.arrayCopy(array2, (short)0, concatArray, (short) array1.length, (short)array2.length);
        Util.arrayCopy(array3, (short)0, concatArray, (short)(array1.length + array2.length), (short)array3.length);
        Util.arrayCopy(array4, (short)0, concatArray, (short)(array1.length + array2.length + array3.length), (short)array4.length);
        return concatArray;
    }
}
