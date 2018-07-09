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
using System.Linq;

namespace DCEMV.TCPIPDriver
{
    public class TCPIPManager
    {
        public static Logger Logger = new Logger(typeof(TCPIPManager));

        public static byte[] SendTransaction(TCPClientStream stream, byte[] txBytes)
        {
            try
            {
                byte[] rxBuffer = Receive(stream); //wait for ENQ
                if (rxBuffer[0] != 0x05)
                    throw new TCPIPManagerException("Could not send Transaction: ENQ char not received");

                Transmit(stream, txBytes);

                byte[] received = Receive(stream);

                SendACK(stream);

                return received;
            }
            catch(Exception ex)
            {
                try
                {
                    SendNACK(stream);
                }
                catch { }
                throw ex;
            }
        }

        private static void SendNACK(TCPClientStream stream)
        {
            byte chrACK = 0x15;

            Transmit(stream, new byte[] { chrACK });
        }
        private static void SendACK(TCPClientStream stream)
        {
            byte chrACK = 0x06; 

            Transmit(stream, new byte[] { chrACK });
        }

        private static void Transmit(TCPClientStream stream, byte[] txBytes)
        {
            byte chrSTX = 0x02; // Start of Text
            byte chrETX = 0x03; // End of Text
            //byte[] LRC;

            short length = (short)(txBytes.Length + 2);
            byte[] lengthBytes = new byte[] { (byte)(length / 256), (byte)(length % 256) };

            byte[] txBytesTCP = Formatting.ConcatArrays(lengthBytes, new byte[] { chrSTX }, txBytes, new byte[] { chrETX });

            Logger.Log("Sending: [" + Formatting.ConvertToInt16(lengthBytes) + "]" +
                "[" + Formatting.ByteArrayToHexString(new byte[] { chrSTX }) + "]" +
                "[" + Formatting.ByteArrayToASCIIString(txBytes) + "]" +
                "[" + Formatting.ByteArrayToHexString(new byte[] { chrETX }) + "]");

            stream.Write(txBytesTCP);
        }
        private static byte[] Receive(TCPClientStream stream)
        {
            byte chrSTX = 0x02; // Start of Text
            byte chrETX = 0x03; // End of Text

            byte[] rxBuffer = new Byte[4096];
            int countBytesRead = stream.Read(rxBuffer);
            
            byte[] lengthBytesReceived = new byte[2];
            Array.Copy(rxBuffer, 0, lengthBytesReceived, 0, lengthBytesReceived.Length);

            int bytesInRxPacket = Formatting.ConvertToInt16(lengthBytesReceived);
            if(bytesInRxPacket + 2 != countBytesRead)
                throw new TCPIPManagerException("Did not receive all expected bytes");

            byte[] result = new byte[bytesInRxPacket]; 
            Array.Copy(rxBuffer, 2, result, 0, result.Length);

            bool hasSTX = false;
            bool hasETX = false;
            if (result.First() == chrSTX)
            {
                byte[] strippedSTX = new byte[result.Length - 1];
                Array.Copy(result, 1, strippedSTX, 0, strippedSTX.Length);
                result = strippedSTX;
                hasSTX = true;
            }
            int lastPos = Array.FindIndex(result, 0, (x) => x == chrETX);
            if (lastPos != -1)
            {
                int lengthToCopy = result.Length - (result.Length - lastPos);
                byte[] strippedETX = new byte[lengthToCopy];
                Array.Copy(result, 0, strippedETX, 0, strippedETX.Length);
                result = strippedETX;
                hasETX = true;
            }

            Logger.Log("Received:[" + countBytesRead + "]" +
                    (hasSTX == true? "[" + Formatting.ByteArrayToHexString(new byte[] { chrSTX }) + "]": "[No STX]") +
                    "[" + Formatting.ByteArrayToASCIIString(result) + "]" +
                    (hasETX == true ? "[" + Formatting.ByteArrayToHexString(new byte[] { chrETX }) + "]": "[No ETX]"));

            return result;
        }
    }
}
