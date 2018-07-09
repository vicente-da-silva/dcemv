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
using System.Net.Sockets;
using System.Threading.Tasks;

namespace DCEMV.DemoServer
{
    public class TCPClientStreamNetCore : TCPClientStream
    {
        private TcpClient client;
        private NetworkStream stream;
       
        public override void Connect(string ip, int port)
        {
            try
            {
                client = new TcpClient();
                Task.Run(async ()=> await client.ConnectAsync(ip, port)).Wait();
                stream = client.GetStream();
                stream.ReadTimeout = 2000;
                stream.WriteTimeout = 2000;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public override int Read(byte[] buffer)
        {
            return stream.Read(buffer, 0, buffer.Length);
        }

        public override void Write(byte[] buffer)
        {
            stream.Write(buffer, 0, buffer.Length);
        }

        public override void Dispose()
        {
            if(stream!=null)stream.Dispose();
            if(client!=null)client.Dispose();
        }
    }
}
