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
using Android.App;
using Android.Nfc;
using Android.Nfc.Tech;
using DCEMV.Shared;
using System;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace DCEMV.CardReaders.AndroidNFCDriver
{
    public class AndroidNFCCardReader : Java.Lang.Object, ICardInterfaceManger, NfcAdapter.IReaderCallback//, NfcAdapter.IOnTagRemovedListener //only in API level 24
    {
        // Recommend NfcAdapter flags for reading from other Android devices. Indicates that this
        // activity is interested in NFC-A devices (including other Android devices), and that the
        // system should not check for the presence of NDEF-formatted data (e.g. Android Beam).
        private NfcReaderFlags READER_FLAGS = NfcReaderFlags.NfcA | NfcReaderFlags.NfcB | NfcReaderFlags.NfcF | NfcReaderFlags.NfcV | NfcReaderFlags.SkipNdefCheck;
        private IsoDep isoDep;
        private Activity activity;

        public AndroidNFCCardReader(Activity activity)
        {
            this.activity = activity;
        }
        
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

        public async Task<ObservableCollection<string>> GetCardReaders()
        {
            return new ObservableCollection<string>() { "android default" };
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            await Task.Run(() =>
            {
                NfcAdapter nfc = NfcAdapter.GetDefaultAdapter(activity);
                if (nfc != null)
                {
                    nfc.EnableReaderMode(activity, this, READER_FLAGS, null);
                }
            });
        }

        public void StopCardReaderAsync()
        {
            NfcAdapter nfc = NfcAdapter.GetDefaultAdapter(activity);
            if (nfc != null)
            {
                nfc.DisableReaderMode(activity);
            }
        }

        public async Task<byte[]> TransmitAsync(byte[] inputData)
        {
            return await Task.Run(()=> 
            {
                return isoDep.Transceive(inputData);
            });
        }

        public void OnTagDiscovered(Tag tag)
        {
            //OnTagRemoved();//TODO: fix this hack

            isoDep = IsoDep.Get(tag);
            if (isoDep != null)
            {
                try
                {
                    isoDep.Connect();
                    OnCardPutInField(new EventArgs());
                }
                catch (Exception e)
                {
                    throw e;
                }
            }
        }

        public void OnTagRemoved()
        {
            try
            {
                isoDep.Close();
                OnCardRemovedFromField(new EventArgs());
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        
    }
}
