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
using Windows.Devices.SmartCards;

using Windows.Storage.Streams;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;

namespace Pcsc.Common
{
    /// <summary>
    /// Class used to detect the type of the ICC card detected. It accept a connection object
    /// and gets the ATR from the ICC. After the ATR is parsed, the ICC Detection class inspects
    /// the historical bytes in order to detect the ICC type as specified by PCSC specification.
    /// </summary>
    public class IccDetection
    {
        /// <summary>
        /// PCSC device type
        /// </summary>
        public DeviceClass PcscDeviceClass { set; get; }
        /// <summary>
        /// PCSC card name provided in the nn short int
        /// </summary>
        public Pcsc.CardName PcscCardName { set; get; }
        /// <summary>
        /// ATR byte array
        /// </summary>
        public byte[] Atr { set; get; }
        /// <summary>
        /// ATR info holds information about the interface character along other info
        /// </summary>
        public AtrInfo AtrInformation { set; get; }
        /// <summary>
        /// smard card object passed in the constructor
        /// </summary>
        private SmartCard smartCard { set; get; }
        /// <summary>
        /// Smard card connection passed in the constructor
        /// </summary>
        private SmartCardConnection connectionObject { set; get; }
        /// <summary>
        /// class constructor.
        /// </summary>
        /// <param name="card">
        /// smart card object
        /// </param>
        /// <param name="connection">
        /// connection object to the smard card
        /// </param>
        public IccDetection(SmartCard card, SmartCardConnection connection)
        {
            smartCard = card;
            connectionObject = connection;
            PcscDeviceClass = DeviceClass.Unknown;
            PcscCardName = Pcsc.CardName.Unknown;
        }
        /// <summary>
        /// Detects the ICC type by parsing, and analyzing the ATR
        /// </summary>
        /// <returns>
        /// none
        /// </returns>
        public async System.Threading.Tasks.Task DetectCardTypeAync()
        {
            try
            {
                var atrBuffer = await smartCard.GetAnswerToResetAsync();
                Atr = atrBuffer.ToArray();

                Debug.WriteLine("Status: " + (await smartCard.GetStatusAsync()) + "ATR [" + atrBuffer.Length.ToString() + "] = " + BitConverter.ToString(Atr));

                AtrInformation = AtrParser.Parse(Atr);

                if (AtrInformation != null && AtrInformation.HistoricalBytes.Length > 0)
                {
                    DetectCard();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message + e.StackTrace);
            }
        }
        /// <summary>
        /// Internal method that analyzes the ATR Historical Bytes,
        /// it populate the object with info about the ICC
        /// </summary>
        private void DetectCard()
        {
            if (AtrInformation.HistoricalBytes.Length > 1)
            {
                byte categoryIndicator;

                using (DataReader reader = DataReader.FromBuffer(AtrInformation.HistoricalBytes))
                {
                    categoryIndicator = reader.ReadByte();

                    if (categoryIndicator == (byte)CategoryIndicator.StatusInfoPresentInTlv)
                    {
                        while (reader.UnconsumedBufferLength > 0)
                        {
                            const byte appIdPresenceIndTag = 0x4F;
                            const byte appIdPresenceIndTagLen = 0x0C;

                            var tagValue = reader.ReadByte();
                            var tagLength = reader.ReadByte();

                            if (tagValue == appIdPresenceIndTag && tagLength == appIdPresenceIndTagLen)
                            {
                                byte[] pcscRid = { 0xA0, 0x00, 0x00, 0x03, 0x06 };
                                byte[] pcscRidRead = new byte[pcscRid.Length];

                                reader.ReadBytes(pcscRidRead);

                                if (pcscRid.SequenceEqual(pcscRidRead))
                                {
                                    byte storageStandard = reader.ReadByte();
                                    ushort cardName = reader.ReadUInt16();

                                    PcscCardName = (Pcsc.CardName)cardName;
                                    PcscDeviceClass = DeviceClass.StorageClass;
                                }

                                reader.ReadBuffer(4); // RFU bytes
                            }
                            else
                            {
                                reader.ReadBuffer(tagLength);
                            }
                        }
                    }
                }
            }
            else
            {
                // Compare with Mifare DesFire card ATR
                byte[] desfireAtr = { 0x3B, 0x81, 0x80, 0x01, 0x80, 0x80 };

                if (Atr.SequenceEqual(desfireAtr))
                {
                    PcscDeviceClass = DeviceClass.MifareDesfire;
                }
            }
        }
        /// <summary>
        /// Helper enum to hold various constants
        /// </summary>
        enum CategoryIndicator : byte
        {
            StatusInfoPresentAtEnd = 0x00,
            StatusInfoPresentInTlv = 0x80
        }
    }
}
