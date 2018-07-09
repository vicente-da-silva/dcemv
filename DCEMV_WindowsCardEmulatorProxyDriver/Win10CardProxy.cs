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
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace DCEMV.CardReaders.WindowsCardEmulatorProxyDriver
{
    public class Win10CardProxy : ICardInterfaceManger
    {
        public event EventHandler CardPutInField;
        public event EventHandler CardRemovedFromField;

        private string ip;
        private int port;

        private TCPClientStream tcpClientStream;

        public Win10CardProxy(string ip, int port)
        {
            tcpClientStream = new TCPClientStreamUWP();
            this.ip = ip;
            this.port = port;
        }

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
            return new ObservableCollection<string>() { "Javacard emulator proxy" };
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            tcpClientStream.Connect(ip,port);
            DelayBeforeCardPutInField();
        }

        public void StopCardReaderAsync()
        {

        }

        private async void DelayBeforeCardPutInField()
        {
            await Task.Delay(2000);
            OnCardPutInField(new EventArgs());
        }

        public async Task<byte[]> TransmitAsync(byte[] inputData)
        {
            return TCPIPManager.SendTransaction(tcpClientStream, inputData);
        }

        public static void LogMessage(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
