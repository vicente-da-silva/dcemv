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
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Pcsc.Common
{
    public class AtrInfo
    {
        public const int MAXIMUM_ATR_CODES = 4;
        /// <summary>
        /// Helper class that holds information about the ICC Answer-To-Reset (ATR) information such
        /// as interface character and offset and length of the historical bytes. It also hold info 
        /// about the validity of the TCK byte.
        /// </summary>
        public AtrInfo()
        {
            ProtocolInterfaceA = new byte[MAXIMUM_ATR_CODES] { 0, 0, 0, 0 };
            ProtocolInterfaceB = new byte[MAXIMUM_ATR_CODES] { 0, 0, 0, 0 };
            ProtocolInterfaceC = new byte[MAXIMUM_ATR_CODES] { 0, 0, 0, 0 };
            ProtocolInterfaceD = new byte[MAXIMUM_ATR_CODES] { 0, 0, 0, 0 };

            HistoricalBytes = null;
        }
        /// <summary>
        /// Protocol interface characters TAi
        /// </summary>
        public byte[] ProtocolInterfaceA { set; get; }
        /// <summary>
        /// Protocol interface characters TBi
        /// </summary>
        public byte[] ProtocolInterfaceB { set; get; }
        /// <summary>
        /// Protocol interface characters TCi
        /// </summary>
        public byte[] ProtocolInterfaceC { set; get; }
        /// <summary>
        /// Protocol interface characters TDi
        /// </summary>
        public byte[] ProtocolInterfaceD { set; get; }
        /// <summary>
        /// Historical bytes if present
        /// </summary>
        public IBuffer HistoricalBytes { set; get; }
        /// <summary>
        /// Check Byte valid
        /// </summary>
        public bool? TckValid { set; get; }
    }
    /// <summary>
    /// Helper class that parses the ATR and populate the AtrInfo class
    /// </summary>
    class AtrParser
    {
        enum AtrHeader : byte
        {
            InitialHeader = 0x3B,
        }
        /// <summary>
        /// Main parser method that extract information about the ATR byte array
        /// </summary>
        /// <returns>
        /// returns AtrInfo object if ATR is valid, null otherwise
        /// </returns>
        public static AtrInfo Parse(byte[] atrBytes)
        {
            AtrInfo atrInfo = new AtrInfo();
            byte initialChar = 0, formatByte = 0;
            int supportedProtocols = 0;

            using (DataReader reader = DataReader.FromBuffer(atrBytes.AsBuffer()))
            {
                initialChar = reader.ReadByte();

                if (initialChar != (byte)AtrHeader.InitialHeader)
                {
                    return null;
                }

                formatByte = reader.ReadByte();
                var interfacePresence = (byte)(formatByte.HiNibble() << 4);

                for (int i = 0; i < AtrInfo.MAXIMUM_ATR_CODES; i++)
                {
                    if ((interfacePresence & 0x10) != 0)
                        atrInfo.ProtocolInterfaceA[i] = reader.ReadByte();

                    if ((interfacePresence & 0x20) != 0)
                        atrInfo.ProtocolInterfaceB[i] = reader.ReadByte();

                    if ((interfacePresence & 0x40) != 0)
                        atrInfo.ProtocolInterfaceC[i] = reader.ReadByte();

                    if ((interfacePresence & 0x80) != 0)
                        atrInfo.ProtocolInterfaceD[i] = reader.ReadByte();
                    else
                        break;

                    interfacePresence = atrInfo.ProtocolInterfaceD[i];
                    supportedProtocols |= (1 << interfacePresence.LowNibble());
                }

                atrInfo.HistoricalBytes = reader.ReadBuffer(formatByte.LowNibble());

                if ((supportedProtocols & ~1) != 0)
                {
                    atrInfo.TckValid = ValidateTCK(atrBytes);
                }

                return atrInfo;
            }
        }
        /// <summary>
        /// Compute the ATR check byte (TCK)
        /// </summary>
        /// <returns>
        /// return the computed TCK
        /// </returns>
        private static bool ValidateTCK(byte[] atr)
        {
            byte ctk = 0;

            for (byte i = 1; i < atr.Length; i++)
            {
                ctk ^= atr[i];
            }

            return (ctk == 0);
        }
    }
    /// <summary>
    /// Extensions to the Byte primitive data type
    /// </summary>
    public static class ByteExtension
    {
        public const byte NIBBLE_SIZE = 4;
        /// <summary>
        /// Extracts the high nibble of a byte
        /// </summary>
        /// <returns>
        /// return byte represeting the high nibble of a byte
        /// </returns>
        public static byte HiNibble(this byte value)
        {
            return (byte)(value >> 4);
        }
        /// <summary>
        /// Extracts the low nibble of a byte
        /// </summary>
        /// <returns>
        /// return byte represeting the low nibble of a byte
        /// </returns>
        public static byte LowNibble(this byte value)
        {
            return (byte)(value & 0x0F);
        }
    }
}
