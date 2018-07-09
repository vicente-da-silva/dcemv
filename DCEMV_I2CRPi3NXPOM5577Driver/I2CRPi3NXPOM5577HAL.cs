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
using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Devices.I2c;
using Microsoft.IoT.Lightning.Providers;
using Windows.Devices;
using DCEMV.Shared;
using DCEMV.FormattingUtils;


//https://developer.microsoft.com/en-us/windows/iot/win10/samples/pinmappingsrpi2
namespace DCEMV.CardReaders.I2CRPi3NXPOM5577Driver
{
    public class I2CRPi3NXPOM5577HAL : ICardReaderHAL, IDisposable
    {
        public static Logger Logger = new Logger(typeof(I2CRPi3NXPOM5577HAL));

        private GpioPin pinIRQ;//GPIO23
        private GpioPin pinRESET;//GPIO24
        private I2cDevice i2c7120;

        private bool isInitialized = false;

        //IRQ GPIO 23 pin 16
        //RESET GPIO 24 pin 18
        private const int NXP_NCI_IRQ_GPIO = 23;
        private const int NXP_NCI_RST_GPIO = 24;
        private const byte NXP_NCI_I2C_ADDR = 0x28;
        //private const string I2C_CONTROLLER_NAME = "I2C1";

        private const int MAX_NCI_FRAME_SIZE = 256;
        private const int TIMEOUT_100MS = 100;
        //private const int TIMEOUT_1S = 1000;
        //private const int TIMEOUT_2S = 2000;
        //private const int TIMEOUT_INFINITE = 0;

        public I2CRPi3NXPOM5577HAL()
        {

        }

        public void WaitForReception(out byte[] pRBuff, ref int pBytesread, int timeout)
        {
            pRBuff = new byte[MAX_NCI_FRAME_SIZE];
            Tml_Receive(ref pRBuff, ref pBytesread, timeout);
            //NCI_PRINT_BUF("NxpNci_WaitForReception << ", pRBuff, pBytesread);
        }

        public void Transceive(byte[] pTBuff, out byte[] pRBuff, ref int pBytesread)
        {
            pRBuff = new byte[MAX_NCI_FRAME_SIZE];
            if (pTBuff.Length > MAX_NCI_FRAME_SIZE)
                throw new Exception("bytesToSend greater than MAX_NCI_FRAME_SIZE");

            Tml_Send(pTBuff, ref pBytesread);
            //NCI_PRINT_BUF("NxpNci_HostTransceive >> ", pTBuff, pTBuff.Length);
            Tml_Receive(ref pRBuff, ref pBytesread, TIMEOUT_100MS);
            //NCI_PRINT_BUF("NxpNci_HostTransceive << ", pRBuff, pBytesread);
        }

        public async Task Init()
        {
            if (!isInitialized)
            {
                await Tml_Init(NXP_NCI_IRQ_GPIO, NXP_NCI_RST_GPIO);
                isInitialized = true;
            }
            Tml_Reset();
        }

        private async Task Tml_Init(int irqPinNumber, int resetPinNumber)
        {
            if (LightningProvider.IsLightningEnabled)
            {
                LowLevelDevicesController.DefaultProvider = LightningProvider.GetAggregateProvider();
            }

            GpioController gpio = GpioController.GetDefault();

            pinIRQ = gpio.OpenPin(irqPinNumber);
            pinRESET = gpio.OpenPin(resetPinNumber);

            pinRESET.Write(GpioPinValue.High);

            pinIRQ.SetDriveMode(GpioPinDriveMode.Input);
            pinRESET.SetDriveMode(GpioPinDriveMode.Output);

            I2cConnectionSettings settings = new I2cConnectionSettings(NXP_NCI_I2C_ADDR)
            {
                BusSpeed = I2cBusSpeed.FastMode,
                SharingMode = I2cSharingMode.Shared
            };

            //string aqs = I2cDevice.GetDeviceSelector(I2C_CONTROLLER_NAME);
            //DeviceInformationCollection dis = await DeviceInformation.FindAllAsync(aqs);
            //i2c7120 = await I2cDevice.FromIdAsync(dis[0].Id, settings);
    
            I2cController i2cController = await I2cController.GetDefaultAsync();
            i2c7120 = i2cController.GetDevice(settings);
        }

        public void Close()
        {
            pinIRQ.Dispose();
            pinRESET.Dispose();
            i2c7120.Dispose();
        }

        private void Tml_Reset()
        {
            pinRESET.Write(GpioPinValue.Low);
            Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
            pinRESET.Write(GpioPinValue.High);
            Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
        }

        private void Tml_Tx(byte[] pBuff)
        {
            try { I2c_Write(pBuff); }
            catch
            {
                Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
                try { I2c_Write(pBuff); }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
        }

        private void Tml_Rx(ref byte[] pBuff1, ref int pBytesRead)
        {
            byte[] pBuff2 = new byte[3];
            try
            {
                I2c_Read(pBuff2);
                if (pBuff2[2] != 0)
                {
                    byte[] pBuff3 = new byte[pBuff2[2]];
                    I2c_Read(pBuff3);
                    pBytesRead = pBuff2[2] + 3;
                    pBuff1 = new byte[pBytesRead];
                    pBuff2.CopyTo(pBuff1, 0);
                    pBuff3.CopyTo(pBuff1, pBuff2.Length);
                }
                else
                {
                    pBytesRead = 3;
                    pBuff1 = new byte[pBytesRead];
                    pBuff2.CopyTo(pBuff1, 0);
                }
            }
            catch(Exception ex)
            {
                pBytesRead = 0;
                pBuff1 = new byte[pBytesRead];
                throw ex;
            }
        }

        private void Tml_WaitForRx(int timeout)
        {
            if (timeout == 0)
                while ((pinIRQ.Read() == GpioPinValue.Low)) ;
            else
            {
                timeout = 1000;
                while ((pinIRQ.Read() == GpioPinValue.Low))
                {
                    Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
                    timeout -= 10;
                    if (timeout <= 0)
                        throw new TMLTimeoutException();
                }
            }
        }

        private void Tml_Send(byte[] pBuffer, ref int pBytesSent)
        {
            try
            {
                Tml_Tx(pBuffer);
                pBytesSent = pBuffer.Length;
            }
            catch(Exception ex)
            {
                pBytesSent = 0;
                throw ex;
            }
        }

        private void Tml_Receive(ref byte[] pBuffer, ref int pBytesRead, int timeout)
        {
            try
            {
                Tml_WaitForRx(timeout);
                Tml_Rx(ref pBuffer, ref pBytesRead);
            }
            catch (TMLTimeoutException te)
            {
                pBytesRead = 0;
                pBuffer = new byte[pBytesRead];
                throw te;
            }
        }

        private void I2c_Write(byte[] pBuff)
        {
            i2c7120.Write(pBuff);
        }

        private void I2c_Read(byte[] pBuff)
        {
            i2c7120.Read(pBuff);
        }

        private static void NCI_PRINT_BUF(String desc, byte[] bytes, int numberOfBytes)
        {
            Logger.Log(desc + ByteArrayToHexUtil.ByteArrayToHexViaLookup32(bytes) + " Size: [" + Convert.ToString(numberOfBytes) + "]");
        }

        public void Dispose()
        {
            Close();
        }
    }
}
