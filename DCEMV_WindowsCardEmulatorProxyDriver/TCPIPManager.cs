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
using DCEMV.FormattingUtils;
using System;

namespace DCEMV.CardReaders.WindowsCardEmulatorProxyDriver
{
    internal class TCPIPManagerException : Exception
    {
        public TCPIPManagerException()
        {
        }

        public TCPIPManagerException(string message) : base(message)
        {
        }

        public TCPIPManagerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
    public class TCPIPManager
    {
        public static Logger Logger = new Logger(typeof(TCPIPManager));

        public static byte[] SendTransaction(TCPClientStream stream, byte[] txBytes)
        {
            try
            {
                Transmit(stream, txBytes);
                return Receive(stream);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private static void Transmit(TCPClientStream stream, byte[] txBytes)
        {
            short length = (short)(txBytes.Length);
            byte[] lengthBytes = new byte[] { (byte)(length / 256), (byte)(length % 256) };

            byte[] txBytesTCP = Formatting.ConcatArrays(lengthBytes, txBytes);

            Logger.Log("Sending: [" + Formatting.ConvertToInt16(lengthBytes) + "]" +
                "[" + Formatting.ByteArrayToHexString(txBytes) + "]");

            stream.Write(txBytesTCP);
        }
        private static byte[] Receive(TCPClientStream stream)
        {
            byte[] rxBuffer = new Byte[4096];
            int countBytesRead = stream.Read(rxBuffer);

            byte[] lengthBytesReceived = new byte[2];
            Array.Copy(rxBuffer, 0, lengthBytesReceived, 0, lengthBytesReceived.Length);

            int bytesInRxPacket = Formatting.ConvertToInt16(lengthBytesReceived);
            if (bytesInRxPacket + 2 != countBytesRead)
                throw new TCPIPManagerException("Did not receive all expected bytes");

            byte[] result = new byte[bytesInRxPacket];
            Array.Copy(rxBuffer, 2, result, 0, result.Length);
            
            Logger.Log("Received:[" + bytesInRxPacket + "]" +
                    "[" + Formatting.ByteArrayToHexString(result) + "]");

            return result;
        }
    }
}
