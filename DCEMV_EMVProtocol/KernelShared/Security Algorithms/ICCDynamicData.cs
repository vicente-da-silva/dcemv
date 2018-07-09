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

namespace DCEMV.EMVProtocol.Kernels
{
    public enum ICCDynamicDataType
    {
        NO_IDS_OR_RRP,
        RRP,
        IDS,
        IDS_AND_RRP,
        DYNAMIC_NUMBER_ONLY
    }
    public class ICCDynamicData
    {
        public byte ICCDynamicNumberLength { get; protected set; }
        public byte[] ICCDynamicNumber { get; protected set; }
        public byte CryptogramInformationData { get; protected set; }
        public byte[] ApplicationCryptogram { get; protected set; }
        public byte[] TransactionDataHashCode { get; protected set; }
        public ICCDynamicDataType IccDynamicDataType { get; protected set; }

        //ids
        public byte[] DSSummary2 { get; protected set; }
        public byte[] DSSummary3 { get; protected set; }

        //rrp
        public byte[] Terminal_Relay_Resistance_Entropy { get; protected set; }
        public byte[] Device_Relay_Resistance_Entropy { get; protected set; }
        public byte[] Min_Time_For_Processing_Relay_Resistance_APDU { get; protected set; }
        public byte[] Max_Time_For_Processing_Relay_Resistance_APDU { get; protected set; }
        public byte[] Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU { get; protected set; }


        public ICCDynamicData(KernelDatabaseBase database, byte[] value, ICCDynamicDataType IccDynamicDataType)
        {
            this.IccDynamicDataType = IccDynamicDataType;
            deserialize(database, value, 0);
        }

        public int deserialize(KernelDatabaseBase database,byte[] iccDynamicData, int pos)
        {
            ICCDynamicNumberLength = iccDynamicData[pos];
            pos++;
            ICCDynamicNumber = new byte[ICCDynamicNumberLength];
            Array.Copy(iccDynamicData, pos, ICCDynamicNumber, 0, ICCDynamicNumber.Length);
            pos = pos + ICCDynamicNumber.Length;

            if (IccDynamicDataType != ICCDynamicDataType.DYNAMIC_NUMBER_ONLY)
            {
                CryptogramInformationData = iccDynamicData[pos];
                pos++;
                ApplicationCryptogram = new byte[8];
                Array.Copy(iccDynamicData, pos, ApplicationCryptogram, 0, ApplicationCryptogram.Length);
                pos = pos + ApplicationCryptogram.Length;
                TransactionDataHashCode = new byte[20];
                Array.Copy(iccDynamicData, pos, TransactionDataHashCode, 0, TransactionDataHashCode.Length);
                pos = pos + TransactionDataHashCode.Length;
            }

            APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci;
            switch (IccDynamicDataType)
            {
                case ICCDynamicDataType.NO_IDS_OR_RRP:
                    break;

                case ICCDynamicDataType.IDS_AND_RRP:
                    aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                    pos = deserializeIDS(aci, iccDynamicData, pos);
                    pos = deserializeRRP(iccDynamicData, pos);
                    break;

                case ICCDynamicDataType.IDS:
                    aci = new APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2(database);
                    pos = deserializeIDS(aci, iccDynamicData, pos);
                    break;

                case ICCDynamicDataType.RRP:
                    pos = deserializeRRP(iccDynamicData, pos);
                    break;
            }

            return pos;
        }

        private int deserializeIDS(APPLICATION_CAPABILITIES_INFORMATION_9F5D_KRN2 aci, byte[] iccDynamicData, int pos)
        {
            int summaryLength;
            switch (aci.Value.DataStorageVersionNumberEnum)
            {
                case DataStorageVersionNumberEnum.VERSION_1:
                    summaryLength = 8;
                    break;

                case DataStorageVersionNumberEnum.VERSION_2:
                    summaryLength = 16;
                    break;

                default:
                    throw new EMVProtocolException("Invalid DataStorageVersionNumberEnum in ICCDynamicData");
            }

            if (pos + summaryLength <= iccDynamicData.Length)
            {
                DSSummary2 = new byte[summaryLength];
                Array.Copy(iccDynamicData, pos, DSSummary2, 0, DSSummary2.Length);
                pos = pos + DSSummary2.Length;
            }
            else
                DSSummary2 = null;

            if (pos + summaryLength <= iccDynamicData.Length)
            {
                DSSummary3 = new byte[summaryLength];
                Array.Copy(iccDynamicData, pos, DSSummary3, 0, DSSummary3.Length);
                pos = pos + DSSummary3.Length;
            }
            else
                DSSummary3 = null;

            return pos;
        }
        private int deserializeRRP(byte[] iccDynamicData, int pos)
        {
            Terminal_Relay_Resistance_Entropy = new byte[4];
            Array.Copy(iccDynamicData, pos, Terminal_Relay_Resistance_Entropy, 0, Terminal_Relay_Resistance_Entropy.Length);
            pos = pos + Terminal_Relay_Resistance_Entropy.Length;
            Device_Relay_Resistance_Entropy = new byte[4];
            Array.Copy(iccDynamicData, pos, Device_Relay_Resistance_Entropy, 0, Device_Relay_Resistance_Entropy.Length);
            pos = pos + Device_Relay_Resistance_Entropy.Length;
            Min_Time_For_Processing_Relay_Resistance_APDU = new byte[2];
            Array.Copy(iccDynamicData, pos, Min_Time_For_Processing_Relay_Resistance_APDU, 0, Min_Time_For_Processing_Relay_Resistance_APDU.Length);
            pos = pos + Min_Time_For_Processing_Relay_Resistance_APDU.Length;
            Max_Time_For_Processing_Relay_Resistance_APDU = new byte[2];
            Array.Copy(iccDynamicData, pos, Max_Time_For_Processing_Relay_Resistance_APDU, 0, Max_Time_For_Processing_Relay_Resistance_APDU.Length);
            pos = pos + Max_Time_For_Processing_Relay_Resistance_APDU.Length;
            Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU = new byte[2];
            Array.Copy(iccDynamicData, pos, Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU, 0, Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU.Length);
            pos = pos + Device_Estimated_Transmission_Time_For_Relay_Resistance_R_APDU.Length;

            return pos;
        }
    }
}
