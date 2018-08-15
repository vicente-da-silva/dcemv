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
using DataFormatters;
using System.Collections.Generic;
using System.Linq;

namespace DCEMV.EMVProtocol.Kernels
{
    public class FILE_LOCATOR_ENTRY
    {
        public byte SFI { get; set; }
        public byte FirstRecordNumber { get; set; }
        public byte LastRecordNumber { get; set; }
        public byte OfflineDataAuthenticationRecordLength { get; set; }

        public FILE_LOCATOR_ENTRY()
        {
           
        }

        public byte[] serialize()
        {
            byte[] aflBytes = new byte[4];
            aflBytes[0] = (byte)(SFI << 3);
            aflBytes[1] = FirstRecordNumber;
            aflBytes[2] = LastRecordNumber;
            aflBytes[3] = OfflineDataAuthenticationRecordLength;
            return aflBytes;
        }
        public int deserialize(byte[] rawTlv, int pos)
        {
            SFI = (byte)(rawTlv[pos] >> 3);
            FirstRecordNumber = rawTlv[pos+1];
            LastRecordNumber = rawTlv[pos+2];
            OfflineDataAuthenticationRecordLength = rawTlv[pos+3];
            pos = pos + 4;
            return pos;
        }
    }

    public class APPLICATION_FILE_LOCATOR_AFL_94_KRN : SmartTag
    {
        
        public class APPLICATION_FILE_LOCATOR_AFL_94_KRN_VALUE : SmartValue
        {
            public List<FILE_LOCATOR_ENTRY> Entries { get; }

            public APPLICATION_FILE_LOCATOR_AFL_94_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                Entries = new List<FILE_LOCATOR_ENTRY>();
            }

            public override byte[] Serialize()
            {
                List<byte[]> result = new List<byte[]>();

                foreach(FILE_LOCATOR_ENTRY fle in Entries)
                    result.Add(fle.serialize());

                Value = result.SelectMany(a => a).ToArray();

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                for(int i = 0; i < Value.Length;)//dont increment i
                {
                    FILE_LOCATOR_ENTRY fle = new FILE_LOCATOR_ENTRY();
                    i = fle.deserialize(Value,i);
                    Entries.Add(fle);
                }

                List<FILE_LOCATOR_ENTRY> newList = new List<FILE_LOCATOR_ENTRY>();
                foreach (FILE_LOCATOR_ENTRY fle in Entries)
                {
                    if (fle.FirstRecordNumber != fle.LastRecordNumber)
                    {
                        int offlineCounter = fle.OfflineDataAuthenticationRecordLength;
                        for (int j = fle.FirstRecordNumber; j <= fle.LastRecordNumber; j++)
                        {
                            FILE_LOCATOR_ENTRY fleNew = new FILE_LOCATOR_ENTRY();
                            fleNew.FirstRecordNumber = (byte)j;
                            fleNew.LastRecordNumber = (byte)j;
                            fleNew.SFI = fle.SFI;

                            if (offlineCounter > 0)
                                fleNew.OfflineDataAuthenticationRecordLength = 0x01;
                            else
                                fleNew.OfflineDataAuthenticationRecordLength = 0x00;

                            if (offlineCounter > 0)
                                offlineCounter--;
                            //fleNew.OfflineDataAuthenticationRecordLength = fle.OfflineDataAuthenticationRecordLength;

                            newList.Add(fleNew);
                        }

                    }
                    else
                        newList.Add(fle);
                }

                Entries.Clear();
                Entries.AddRange(newList);

                return pos;
            }
        }

        public new APPLICATION_FILE_LOCATOR_AFL_94_KRN_VALUE Value { get { return (APPLICATION_FILE_LOCATOR_AFL_94_KRN_VALUE)Val; } }

        public APPLICATION_FILE_LOCATOR_AFL_94_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN, 
                  new APPLICATION_FILE_LOCATOR_AFL_94_KRN_VALUE(EMVTagsEnum.APPLICATION_FILE_LOCATOR_AFL_94_KRN.DataFormatter))
        {
        }
    }
}
