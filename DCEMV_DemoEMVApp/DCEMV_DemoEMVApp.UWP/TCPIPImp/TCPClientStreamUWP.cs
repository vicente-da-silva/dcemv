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
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace DCEMV.DemoEMVApp
{
    public class TCPClientStreamUWP : TCPClientStream
    {
        private StreamSocket socket;
        public TCPClientStreamUWP()
        {
           
        }
        public override void Connect(string ip, int port)
        {
            socket = new StreamSocket();
            HostName hostName = new HostName(ip);
            string portS = Convert.ToString(port);
            try
            {
                Task.Run(async () => await socket.ConnectAsync(hostName, portS)).Wait();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public override int Read(byte[] buffer)
        {
            try
            {
                DataReader reader = new DataReader(socket.InputStream)
                {
                    InputStreamOptions = InputStreamOptions.Partial
                };
                uint bytesAvailable = 0;
                Task.Run(async () => bytesAvailable = await reader.LoadAsync((uint)buffer.Length)).Wait();
                byte[] data = new byte[bytesAvailable];
                reader.ReadBytes(data);
                Array.Copy(data, 0, buffer, 0, data.Length);
                reader.DetachStream();
                reader.Dispose();
                return (int)bytesAvailable;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void Write(byte[] buffer)
        {
            try
            { 
                DataWriter dataWriter = new DataWriter(socket.OutputStream);
                dataWriter.WriteBytes(buffer);
                Task.Run(async () => await dataWriter.StoreAsync()).Wait();
                Task.Run(async () => await dataWriter.FlushAsync()).Wait();
                dataWriter.DetachStream();
                dataWriter.Dispose();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override void Dispose()
        {
            socket.Dispose();
        }
    }
}
