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
using System.Linq;

namespace DCEMV.EMVProtocol.Kernels
{
    public class SDAD
    {
        public byte DataHeader { get; protected set; }
        public byte SignedDataFormat { get; protected set; }
        public byte HashAlgorithmIndicator { get; protected set; }
        public byte ICCDynamicDataLength { get; protected set; }
        public byte[] ICCDynamicData { get; protected set; }
        public byte[] PadPattern { get; protected set; }
        public byte[] HashResult { get; protected set; }
        public byte RecoveredDataTrailer { get; protected set; }

        public byte[] Concat(byte[] unpredicatbleNumber)
        {
            List<byte[]> result = new List<byte[]>();

            result.Add(new byte[] { SignedDataFormat });
            result.Add(new byte[] { HashAlgorithmIndicator });
            result.Add(new byte[] { ICCDynamicDataLength });
            result.Add(ICCDynamicData);
            result.Add(PadPattern);
            result.Add(unpredicatbleNumber);

            return result.SelectMany(a => a).ToArray();
        }

        public SDAD(byte[] recoveredData)
        {
            deserialize(recoveredData, 0);
        }
        public int deserialize(byte[] recovered, int pos)
        {
            DataHeader = recovered[pos];
            pos++;
            SignedDataFormat = recovered[pos];
            pos++;
            HashAlgorithmIndicator = recovered[pos];
            pos++;
            ICCDynamicDataLength = recovered[pos];
            pos++;
            ICCDynamicData = new byte[ICCDynamicDataLength];
            Array.Copy(recovered, pos, ICCDynamicData, 0, ICCDynamicData.Length);
            pos = pos + ICCDynamicDataLength;
            int padPatterncount = recovered.Length - ICCDynamicDataLength - 25;
            PadPattern = new byte[padPatterncount];
            Array.Copy(recovered, pos, PadPattern, 0, padPatterncount);
            pos = pos + padPatterncount;
            HashResult = new byte[20];
            Array.Copy(recovered, pos, HashResult, 0, HashResult.Length);
            pos = pos + HashResult.Length;
            RecoveredDataTrailer = recovered[pos];
            pos++;
            return pos;
        }
    }
}
