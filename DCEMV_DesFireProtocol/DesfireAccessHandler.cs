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
using DCEMV.FormattingUtils;
using DCEMV.Shared;
using System;
using System.Linq;
using DCEMV.ISO7816Protocol;

//Select PICC Application
//Authenticate with default Master key(Authenticate)
//Change Master Key for PICC(only 1 key for PICC application)
//Change Master Key Settings for PICC
//SetConfiguration(set key defaults)

//Select PICC Application
//Authenticate with current Master key(Authenticate)
//Create Application, set Encryption type(use AES), set AID etc
//
//Select Application
//Add Application Master Key Key(AMK)(key no = 0)
//Add ChangeKey Key if this is required by adding a key(ChangeKey)
//Change Application Master Key Settings  for PICC(ChangeKeySettings, specify changekey key if this is required, or if different from settings speciedin create application)
//Create Files Required
//(Up to 32 files of different size and type can be created within each application. Different levels of access rights for each single file can be linked to the keys of the application.)

//Select Created Application
//Authenticate with Application Master Key
//Read files(providing key associated with each file, if setup this way)
//Write Files
namespace DCEMV.DesFireProtocol
{
    /// <summary>
    /// Access handler class for Desfire based ICC. It provides wrappers for different Desfire 
    /// commands
    /// </summary>
    public class DesfireAccessHandler
    {
        public static Logger Logger = new Logger(typeof(DesfireAccessHandler));

        private CardQProcessor cardInterface;
       
        public DesfireAccessHandler(CardQProcessor cardInterface)
        {
            this.cardInterface = cardInterface;
        }

        private ApduResponse SendCommand(ApduCommand apduCommand)
        {
            ApduResponse response = cardInterface.SendCommand(apduCommand);
            if (response is DesfireResponse)
                return response;

            throw new DesFireException("Error returned from card");
        }

        public byte[] AuthenticateAES()
        {
            string initialIV = "00000000000000000000000000000000";
            string key = "00000000000000000000000000000000";

            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.AuthenticateAES,
                Data = new byte[] { 0x00 } //1 byte key number
            };
            DesfireResponse desfireRes1 = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireRes1.SubsequentFrame) //additional frames expected
            {
                throw new DesFireException("Error:" + desfireRes1.SW);
            }

            byte[] rndBBytes = AESCrypto.AESDecrypt(
               Formatting.HexStringToByteArray(key),
               Formatting.HexStringToByteArray(initialIV),
               desfireRes1.ResponseData);

            byte[] rndBRotatedBytes = Formatting.RotateRight(rndBBytes);
            string rndAGenerated = "2347C1557F80707ABDFF86BF9D965CA7";
            string rndAPlusRndBRotated = rndAGenerated + Formatting.ByteArrayToHexString(rndBRotatedBytes);

            string ivNew = Formatting.ByteArrayToHexString(desfireRes1.ResponseData);
            byte[] rndAPlusRndBRotatedCipherBytes = AESCrypto.AESEncrypt(
               Formatting.HexStringToByteArray(key),
               Formatting.HexStringToByteArray(ivNew),
               Formatting.HexStringToByteArray(rndAPlusRndBRotated));

            desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetAdditionalFrame,
                Data = rndAPlusRndBRotatedCipherBytes //32 byte enciphered RndA/RndB
            };
            byte[] extractIV = new byte[16];
            Array.Copy(rndAPlusRndBRotatedCipherBytes, rndAPlusRndBRotatedCipherBytes.Length - 16, extractIV, 0, 16);

            DesfireResponse desfireRes2 = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireRes2.Succeeded)
            {
                throw new DesFireException("Error:" + desfireRes2.SW);
            }

            byte[] rndARotatedBytes = AESCrypto.AESDecrypt(
                Formatting.HexStringToByteArray(key),
                extractIV,
                desfireRes2.ResponseData);

            byte[] rndABytes = Formatting.RotateLeft(rndARotatedBytes);
            if (rndAGenerated != Formatting.ByteArrayToHexString(rndABytes))
            {
                throw new DesFireException("rndA generated not same as rndA received");
            }

            string rndAString = Formatting.ByteArrayToHexString(rndABytes);
            string rndBString = Formatting.ByteArrayToHexString(rndBBytes);

            string sessionKey = rndAString.Substring(0, 8) + rndBString.Substring(0, 8) +
                rndAString.Substring(24, 8) + rndBString.Substring(24, 8);

            return Formatting.HexStringToByteArray(sessionKey);
        }

        public byte[] ReadData(byte[] sessionKey, byte[] updatedIV, byte fileNo)
        {
            byte[] offset = new byte[] { 0x00, 0x00, 0x00 }; //offset of 0
            byte[] length = new byte[] { 0x00, 0x00, 0x01 }; //length of 1

            byte[] dataForCMAC = new byte[] { (byte)DesfireCommand.CommandType.ReadData, fileNo, offset[2], offset[1], offset[0], length[2], length[1], length[0] };
            byte[] cmac = AESCMAC.CMAC(
                sessionKey,
                updatedIV,
                dataForCMAC);

            updatedIV = cmac;

            byte[] cmdToSend = new byte[] { fileNo, offset[2], offset[1], offset[0], length[2], length[1], length[0] };
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.ReadData,
                Data = cmdToSend
            };
            DesfireResponse desfireRes = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireRes.Succeeded)
            {
                throw new DesFireException("Error:" + desfireRes.SW);
            }

            byte[] dataBytes = AESCrypto.AESDecrypt(
                sessionKey,
                updatedIV,
                desfireRes.ResponseData);

            byte[] extractIV = new byte[16];
            Array.Copy(desfireRes.ResponseData, desfireRes.ResponseData.Length - 16, extractIV, 0, 16);

            int lengthofData = 0x01;
            byte[] data = new byte[lengthofData];
            Array.Copy(dataBytes, data, data.Length);
            byte[] crc = new byte[4];
            Array.Copy(dataBytes, data.Length, crc, 0, crc.Length); //crc is 4 bytes long
            byte[] dataPlusStatus = new byte[data.Length + 1];
            Array.Copy(data, dataPlusStatus, data.Length);
            Array.Copy(new byte[] { 0x00 }, 0, dataPlusStatus, data.Length, 1);
            uint crc32 = Crc32.Compute(dataPlusStatus);
            //compare crc32 to crc
            if(BitConverter.ToUInt32(crc,0) != crc32)
            {
                throw new DesFireException("CRC32 error");
            }
            return data;
        }

        public byte[] WriteData(byte[] sessionKey, byte fileNo, byte dataIn)
        {
            //byte fileNo = 0x01;
            byte[] offset = new byte[] { 0x00, 0x00, 0x00 }; //offset of 0
            byte[] length = new byte[] { 0x00, 0x00, 0x01 }; //length of 1
            byte[] dataToWrite = new byte[] { dataIn };

            byte[] dataForCRC32Header = new byte[] { (byte)DesfireCommand.CommandType.WriteData, fileNo, offset[2], offset[1], offset[0], length[2], length[1], length[0]};
            byte[] dataForCRC32 = new byte[dataForCRC32Header.Length + dataToWrite.Length];
            Array.Copy(dataForCRC32Header, dataForCRC32, dataForCRC32Header.Length);
            Array.Copy(dataToWrite, 0, dataForCRC32, dataForCRC32Header.Length, dataToWrite.Length);

            uint crc32 = Crc32.Compute(dataForCRC32);
            string dataPlusCRC = Formatting.ByteArrayToHexString(dataToWrite) + Formatting.ByteArrayToHexString(BitConverter.GetBytes(crc32));
            byte[] dataPlusCRCPadded = Formatting.HexStringToByteArray(dataPlusCRC);
            Formatting.PadToMultipleOf(ref dataPlusCRCPadded, 16);
            string initialIV = "00000000000000000000000000000000";
            byte[] dataPlusCRCResultPaddedEncrypted = AESCrypto.AESEncrypt(
                sessionKey,
                Formatting.HexStringToByteArray(initialIV),
                dataPlusCRCPadded);
            byte[] extractIV = new byte[16];
            Array.Copy(dataPlusCRCResultPaddedEncrypted, dataPlusCRCResultPaddedEncrypted.Length - 16, extractIV, 0, 16);

            byte[] cmdToSendHeader = new byte[] { fileNo, offset[2], offset[1], offset[0], length[2], length[1], length[0] };
            byte[] cmdToSend = new byte[cmdToSendHeader.Length + dataPlusCRCResultPaddedEncrypted.Length];
            Array.Copy(cmdToSendHeader, cmdToSend, cmdToSendHeader.Length);
            Array.Copy(dataPlusCRCResultPaddedEncrypted, 0, cmdToSend, cmdToSendHeader.Length, dataPlusCRCResultPaddedEncrypted.Length);

            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.WriteData,
                Data = cmdToSend
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }

            byte[] cmac = AESCMAC.CMAC(
                sessionKey,
                extractIV,
                new byte[] { desfireResFile.SW2 });

            byte[] updatedIV = cmac;

            byte[] data = ReadData(sessionKey, updatedIV, fileNo);

            if(data[0] != dataToWrite[0])
            {
                throw new DesFireException("Data read not the same as data written");
            }
            return data;
        }

        public void GetApplicationIDs()
        {
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetApplicationIDs
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }

        }

        public void GetFileIDs()
        {
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetFileIDs
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }

        }

        public void GetKeySettings()
        {
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetKeySettings
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }

        }

        public void GetKeyVersion(byte keyNo)
        {
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetKeyVersion,
                Data = new byte[] { keyNo }
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }

        }

        public void CreateApplication(byte[] aid)
        {
            AppMasterKeySettings apks = new AppMasterKeySettings()
            {
                Bit0_AllowChangeMasterKey = true,
                Bit1_FreeDirectoryListAccessWithoutMasterKey = true,//TODO: chnge to false
                Bit2_FreeCreateDeleteFileWithoutMasterKey = true,//TODO: chnge to false
                Bit3_ConfigurationChangeable = true
            };
            ChangeKeyAccessRights ckar = new ChangeKeyAccessRights()
            {
                ChangeKeyAccessRightsType = ChangeKeyAccessRightsEnum.ApplicationMasterKeyAuthenticationIsNecessaryToChangeAnyKey
            };
            apks.Bit4_Bit7_ChangeKeyAccessRights = ckar;

            CreateApplicationKeySettings2 caks2 = new CreateApplicationKeySettings2()
            {
                NumberOfKeysThatCanbeStoredinApplicationForCryptographicPurposes = 1,
                TwoByteFileIdentifiersSupported = false,
                CryptoMethod = CryptoMethodEnum.Crypto_AES
            };
            //byte[] aid = new byte[] {0x00, 0x00, 0x01 };//3 byte aid MSB : LSB
            byte keyset1 = apks.getValue();
            byte keyset2 = caks2.getValue();
            byte[] data = new byte[] { aid[2], aid[1], aid[0], keyset1, keyset2 };

            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.CreateApplication,
                Data = data
            };
            DesfireResponse desfireRes = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireRes.Succeeded)
            {
                throw new DesFireException("Error:" + desfireRes.SW);
            }
        }

        public void CreateFile(byte fileNo)
        {
            //create a std data file
            //byte fileNo = 0x01;
            byte commSettings = (byte)CommunicationSettings.FullEnciphered;
            AccessRights accessRights = new AccessRights()
            {
                ChangeAccess = 0x00, //use application master key for all access
                ReadWriteAccess = 0x00, //use application master key for all access
                WriteAccess = 0x00, //use application master key for all access
                ReadAccess = 0x00 //use application master key for all access
            };
            byte[] accessRightsBytes = accessRights.getValue();
            byte[] fileSize = new byte[] { 0x00, 0x00, 0x20 }; //32 bytes in size
            byte[] dataFile = new byte[] { fileNo, commSettings, accessRightsBytes[1], accessRightsBytes[0], fileSize[2], fileSize[1], fileSize[0] };

            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.CreateStdDataFile,
                Data = dataFile
            };
            DesfireResponse desfireResFile = SendCommand(desfireCommand) as DesfireResponse;
            if (!desfireResFile.Succeeded)
            {
                throw new DesFireException("Error:" + desfireResFile.SW);
            }
        }

        public CardDetails ReadCardDetails()
        {
            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetVersion,
                Data = null
            };
            DesfireResponse desfireRes = SendCommand(desfireCommand) as DesfireResponse;

            if (!desfireRes.SubsequentFrame || desfireRes.ResponseData.Length != 7)
            {
                return null;
            }

            CardDetails card = new CardDetails()
            {
                HardwareVendorID = desfireRes.ResponseData[0],
                HardwareType = desfireRes.ResponseData[1],
                HardwareSubType = desfireRes.ResponseData[2],
                HardwareMajorVersion = desfireRes.ResponseData[3],
                HardwareMinorVersion = desfireRes.ResponseData[4],
                HardwareStorageSize = desfireRes.ResponseData[5],
                HardwareProtocolType = desfireRes.ResponseData[6]
            };
            desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.GetAdditionalFrame
            };
            desfireRes = SendCommand(desfireCommand) as DesfireResponse;

            if (!desfireRes.SubsequentFrame || desfireRes.ResponseData.Length != 7)
            {
                // Not expected
                return null;
            }
            card.SoftwareVendorID = desfireRes.ResponseData[0];
            card.SoftwareType = desfireRes.ResponseData[1];
            card.SoftwareSubType = desfireRes.ResponseData[2];
            card.SoftwareMajorVersion = desfireRes.ResponseData[3];
            card.SoftwareMinorVersion = desfireRes.ResponseData[4];
            card.SoftwareStorageSize = desfireRes.ResponseData[5];
            card.SoftwareProtocolType = desfireRes.ResponseData[6];

            desfireRes = SendCommand(desfireCommand) as DesfireResponse;

            if (!desfireRes.Succeeded || desfireRes.ResponseData.Length != 14)
            {
                // Not expected
                return null;
            }

            card.UID = new byte[7];
            System.Buffer.BlockCopy(desfireRes.ResponseData, 0, card.UID, 0, 7);

            card.ProductionBatchNumber = new byte[5];
            System.Buffer.BlockCopy(desfireRes.ResponseData, 7, card.ProductionBatchNumber, 0, 5);

            card.WeekOfProduction = desfireRes.ResponseData[12];
            card.YearOfProduction = desfireRes.ResponseData[13];

            return card;
        }
        
        public void SelectApplication(byte[] aid)
        {
            if (aid.Length != 3)
            {
                throw new NotSupportedException();
            }

            DesfireCommand desfireCommand = new DesfireCommand()
            {
                Command = (byte)DesfireCommand.CommandType.SelectApplication,
                Data = aid.Reverse().ToArray() //little endian
            };
            DesfireResponse desfireRes = SendCommand(desfireCommand) as DesfireResponse;

            if (!desfireRes.Succeeded)
            {
                throw new Exception("Failure selecting application, SW=" + desfireRes.SW + " (" + desfireRes.SWTranslation + ")");
            }
        }
    }
 
    public class CardDetails
    {
        public byte HardwareVendorID { get; set; }
        public byte HardwareType { get; set; }
        public byte HardwareSubType { get; set; }
        public byte HardwareMajorVersion { get; set; }
        public byte HardwareMinorVersion { get; set; }
        public byte HardwareStorageSize { get; set; }
        public byte HardwareProtocolType { get; set; }
        public byte SoftwareVendorID { get; set; }
        public byte SoftwareType { get; set; }
        public byte SoftwareSubType { get; set; }
        public byte SoftwareMajorVersion { get; set; }
        public byte SoftwareMinorVersion { get; set; }
        public byte SoftwareStorageSize { get; set; }
        public byte SoftwareProtocolType { get; set; }
        // 7 bytes
        public byte[] UID { get; set; }
        // 5 bytes
        public byte[] ProductionBatchNumber { get; set; }
        public byte WeekOfProduction { get; set; }
        public byte YearOfProduction { get; set; }
        public override string ToString()
        {
            return
                "HardwareVendorID = " + HardwareVendorID.ToString() + Environment.NewLine +
                "HardwareType = " + HardwareType.ToString() + Environment.NewLine +
                "HardwareSubType = " + HardwareSubType.ToString() + Environment.NewLine +
                "HardwareMajorVersion = " + HardwareMajorVersion.ToString() + Environment.NewLine +
                "HardwareMinorVersion = " + HardwareMinorVersion.ToString() + Environment.NewLine +
                "HardwareStorageSize = " + HardwareStorageSize.ToString() + Environment.NewLine +
                "HardwareProtocolType = " + HardwareProtocolType.ToString() + Environment.NewLine +
                "SoftwareVendorID = " + SoftwareVendorID.ToString() + Environment.NewLine +
                "SoftwareType = " + SoftwareType.ToString() + Environment.NewLine +
                "SoftwareSubType = " + SoftwareSubType.ToString() + Environment.NewLine +
                "SoftwareMajorVersion = " + SoftwareMajorVersion.ToString() + Environment.NewLine +
                "SoftwareMinorVersion = " + SoftwareMinorVersion.ToString() + Environment.NewLine +
                "SoftwareStorageSize = " + SoftwareStorageSize.ToString() + Environment.NewLine +
                "SoftwareProtocolType = " + SoftwareProtocolType.ToString() + Environment.NewLine +
                "UID = " + BitConverter.ToString(UID) + Environment.NewLine +
                "ProductionBatchNumber = " + BitConverter.ToString(ProductionBatchNumber) + Environment.NewLine +
                "WeekOfProduction = " + WeekOfProduction.ToString() + Environment.NewLine +
                "YearOfProduction = " + YearOfProduction.ToString() + Environment.NewLine;
        }
    }
}
