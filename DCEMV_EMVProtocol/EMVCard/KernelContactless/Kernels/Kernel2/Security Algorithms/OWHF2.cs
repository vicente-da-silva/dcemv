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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public static class OWHF2
    {
        public static byte[] OWHF2_8_2(KernelDatabaseBase database, byte[] pd)
        {
            int pl = database.Get(EMVTagsEnum.DS_ID_9F5E_KRN2).Value.Length;
            byte[] dsid = database.Get(EMVTagsEnum.DS_ID_9F5E_KRN2).Value;

            byte[] dspkl = new byte[6];
            byte[] dspkr = new byte[6];

            for(int i = 0; i < 6; i++)
            {
                dspkl[i] = (byte)(((dsid[i] / 16) * 10 + (dsid[i] % 16)) * 2);
                dspkr[i] = (byte)(((dsid[pl - 6 + i] / 16) * 10 + (dsid[pl - 6 + i] % 16)) * 2);
            }

            byte[] oid = new byte[8];
            if(database.IsNotEmpty(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2.Tag) &&
                (database.Get(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2).Value[0] & 0x80) == 0x80 && // Permanent slot type
 
                 (database.Get(EMVTagsEnum.DS_ODS_INFO_DF62_KRN2).Value[0] & 0x40) == 0x40  //Volatile slot type
                )
            {
                oid = new byte[] {0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            }
            else
            {
                oid = database.Get(EMVTagsEnum.DS_REQUESTED_OPERATOR_ID_9F5C_KRN2).Value;
            }

            byte[] kl = new byte[8];
            byte[] kr = new byte[8];
            for (int i = 0; i < 6; i++)
            {
                kl[i] = dspkl[i];
                kr[i] = dspkr[i];

            }
            for (int i = 6; i < 8; i++)
            {
                kl[i] = oid[i - 2];
                kr[i] = oid[i];
            }

            TripleDES des = TripleDES.Create();

            List<byte[]> keyComps = new List<byte[]>();
            keyComps.Add(kl);
            keyComps.Add(kr);
            keyComps.Add(kl);
            byte[] key = keyComps.SelectMany(x => x).ToArray();

            des.Key = key;
            des.Mode = CipherMode.CBC;
            des.Padding = PaddingMode.None;

            byte[] exOrData = XOR(oid, pd);

            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(exOrData, 0, exOrData.Length);
            }
            byte[] r = ms.ToArray();
            ms.Dispose();
            return XOR(r,pd); 
        }
        public static byte[] XOR(byte[] buffer1, byte[] buffer2)
        {
            if (buffer1.Length != buffer2.Length)
                throw new EMVProtocolException("XOR input buffers have different lengths");

            byte[] result = new byte[buffer1.Length];
            for(int i = 0; i< buffer1.Length; i++)
            {
                result[i] = (byte)(buffer1[i] ^ buffer2[i]);
            }
            return result;
        }
        public static byte[] OWHF2AES_8_3(KernelDatabaseBase database, byte[] c)
        {
            byte[] oid = new byte[8];
            if (database.IsNotEmpty(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2.Tag) &&
                (database.Get(EMVTagsEnum.DS_SLOT_MANAGEMENT_CONTROL_9F6F_KRN2).Value[0] & 0x80) == 0x80 && // Permanent slot type

                 (database.Get(EMVTagsEnum.DS_ODS_INFO_DF62_KRN2).Value[0] & 0x40) == 0x40  //Volatile slot type
                )
            {
                oid = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            }
            else
            {
                oid = database.Get(EMVTagsEnum.DS_REQUESTED_OPERATOR_ID_9F5C_KRN2).Value;
            }

            List<byte[]> keyComps = new List<byte[]>();
            keyComps.Add(c);
            keyComps.Add(oid);
            byte[] m = keyComps.SelectMany(x => x).ToArray();

            byte[] dsid = database.Get(EMVTagsEnum.DS_ID_9F5E_KRN2).Value;

            byte[] y = new byte[11];
            Array.Copy(dsid, 0, y, 0, y.Length);//this will be padded

            keyComps = new List<byte[]>();
            keyComps.Add(y);
            keyComps.Add(new byte[] { oid[5], oid[6], oid[7], oid[8] });
            keyComps.Add(new byte[] { 0x3F });
            byte[] k = keyComps.SelectMany(x => x).ToArray();

            Aes aes = Aes.Create();
            aes.Key = k;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;

            MemoryStream ms = new MemoryStream();
            using (CryptoStream cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(m, 0, m.Length);
            }
            byte[] t = ms.ToArray();
            ms.Dispose();
            t = XOR(t, m);

            byte[] r = new byte[8];
            Array.Copy(t, 0, r, 0, 8);
            return r;
        }
    }
}
