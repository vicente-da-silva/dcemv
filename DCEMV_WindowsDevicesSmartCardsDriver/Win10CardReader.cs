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
using DCEMV.Shared;
using Pcsc;
using Pcsc.Common;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.SmartCards;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DCEMV.CardReaders.WindowsDevicesSmartCardsDriver
{
    public class Win10CardReader : ICardInterfaceManger
    {
        private SmartCardReader cardReader;
        private SmartCardConnection connection;
        private SmartCard card;

        public event EventHandler CardPutInField;
        public event EventHandler CardRemovedFromField;

        protected virtual void OnCardPutInField(EventArgs e)
        {
            CardPutInField?.Invoke(this, e);
        }

        protected virtual void OnCardRemovedFromField(EventArgs e)
        {
            CardRemovedFromField?.Invoke(this, e);
        }

        protected void Cleanup()
        {
            if (connection != null)
                connection.Dispose();

            if (cardReader != null)
            {
                cardReader.CardAdded -= CardReader_CardAdded;
                cardReader.CardRemoved -= CardReader_CardRemoved;
                cardReader = null;
            }
        }
        
        public async Task<ObservableCollection<string>> GetCardReaders()
        {
            //List<string> readers = await SmartCardReaderUtils.GetAllSmartCardReaderInfo(SmartCardReaderKind.Nfc);
            //if(readers.Count == 0)
            //{
            List<string> readers = await SmartCardReaderUtils.GetAllSmartCardReaderInfo(SmartCardReaderKind.Any);
            //}
            ObservableCollection<string> ret = new ObservableCollection<string>();
            readers.ForEach((x) => { ret.Add(x); });
            return ret;
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            if (cardReader == null)
            {
                if(String.IsNullOrEmpty(deviceId))
                    throw new Exception("device id not supplied");

                cardReader = await SmartCardReader.FromIdAsync(deviceId);
                cardReader.CardAdded += CardReader_CardAdded;
                cardReader.CardRemoved += CardReader_CardRemoved;
            }
        }

        public void StopCardReaderAsync()
        {
            Cleanup();
        }

        public async Task<byte[]> TransmitAsync(byte[] inputData)
        {
            IBuffer responseBuf;
            byte[] output;
            using (DataWriter writer = new DataWriter())
            {
                writer.WriteBytes(inputData);
                try
                {
                    responseBuf = await connection.TransmitAsync(writer.DetachBuffer());
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            using (DataReader reader = DataReader.FromBuffer(responseBuf))
            {
                output = new byte[responseBuf.Length];
                reader.ReadBytes(output);
            }
            return output;
        }

        protected virtual void CardReader_CardRemoved(SmartCardReader sender, CardRemovedEventArgs args)
        {
            if (connection != null)
                connection.Dispose();

            OnCardRemovedFromField(new EventArgs());
        }

        protected virtual async void CardReader_CardAdded(SmartCardReader sender, CardAddedEventArgs args)
        {
            try
            {
                card = args.SmartCard;
                connection = await card.ConnectAsync();
                //Tuple<CardName, DeviceClass> cardType = await DetectCardType();
                OnCardPutInField(new EventArgs());
            }
            catch(Exception ex)
            {
                LogMessage(ex.Message);
            }
        }

        public async Task<Tuple<CardName, DeviceClass>> DetectCardType()
        {
            try
            {
                if (connection == null)
                    throw new Exception("Card connection not initialised");

                // Try to identify what type of card it was
                IccDetection cardIdentification = new IccDetection(card, connection);
                await cardIdentification.DetectCardTypeAync();
                LogMessage("Connected to card\r\nPC/SC device class: " + cardIdentification.PcscDeviceClass.ToString());
                LogMessage("Card name: " + cardIdentification.PcscCardName.ToString());
                LogMessage("ATR: " + BitConverter.ToString(cardIdentification.Atr));

                return Tuple.Create(cardIdentification.PcscCardName, cardIdentification.PcscDeviceClass);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }

        
    }
}
