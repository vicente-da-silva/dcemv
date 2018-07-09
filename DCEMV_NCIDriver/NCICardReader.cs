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

//#define NXP_CORE_CONF 		//0
//#define NXP_CORE_CONF_EXTN	//0
//#define NXP_CORE_STANDBY 	    //0
#define NXP_RF_CONF		 	    //1
//#define DEBUG_PRINT

using DCEMV.Shared;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace DCEMV.CardReaders.NCIDriver
{
    public class NCICardReader : ICardInterfaceManger
    {
        public static Logger Logger = new Logger(typeof(NCICardReader));

        private ICardReaderHAL cardReaderHAL;
        private NCIVersionEnum NfcControllerGeneration;
        private bool stopPollingLoop = false;

        private const int HOST_TIMEOUT = 2000;
        private int hostTimeoutCounter = HOST_TIMEOUT;

        public event EventHandler CardPutInField;
        public event EventHandler CardRemovedFromField;

        public NCICardReader(ICardReaderHAL cardReaderHAL)
        {
            this.cardReaderHAL = cardReaderHAL;
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
            return new ObservableCollection<string>() { "nci nfc" };
        }

        public async Task StartCardReaderAsync(string deviceId)
        {
            await cardReaderHAL.Init();

            NCIInitialConnect();
            NCICoreConfig();
            NCIConfigureMode(DeviceModeEnum.NXPNCI_MODE_RW);
            NCIStartDiscovery();

            StartPollingLoop();
        }

        public void StopCardReaderAsync()
        {
            StopPollingLoop();
        }

        public Task<byte[]> TransmitAsync(byte[] inputData)
        {
            int bytesRead = 0;
            ResetHostTimeOut();
            Task<byte[]> t = Task.Run(() =>
            {
                TransceiveData(inputData, out byte[] outputData, ref bytesRead);
                return outputData;
            });
            t.Wait();
            ResetHostTimeOut();
            return t;
        }

        private void ResetHostTimeOut()
        {
            hostTimeoutCounter = HOST_TIMEOUT;
        }

        private void StopPollingLoop()
        {
            stopPollingLoop = true;
        }

        private void StartPollingLoop()
        {
            stopPollingLoop = false;
            Task.Run(() =>
            {
                while (1 == 1)
                {
                    try
                    {
                        RFManagementNotification notification;
                        try
                        {
                            notification = NCIWaitForNotification(100);
                        }
                        catch (TMLTimeoutException te)
                        {
                            if (stopPollingLoop)
                            {
                                //cardReaderHAL.Close();
                                break;
                            }
                            else
                                continue;
                        }
                        catch(Exception ex)
                        {
                            throw ex;
                        }
                        RFInterfaceActivationNotification rfIAN = ProcessRFNotification(notification);

                        if (((byte)rfIAN.RFTechnologiesAndMode & (byte)RFModeMaskEnum.MODE_MASK) == (byte)RFModeMaskEnum.MODE_POLL)
                        {
                            if ((rfIAN.RFProtocol != RFProtocolEnum.PROT_NFCDEP) && (rfIAN.RFInterface != RFInterfaceEnum.INTF_UNDETERMINED))
                            {
                                try
                                {
                                    //NCI_PRINT("POLL MODE: Remote T{0:d}T activated", notification.RFProtocol);
                                    OnCardPutInField(EventArgs.Empty);

                                    while (hostTimeoutCounter > 0)
                                    {
                                        int delay = HOST_TIMEOUT / 5;
                                        Task.Delay(TimeSpan.FromMilliseconds(delay)).Wait();
                                        hostTimeoutCounter -= delay;
                                    }

                                    /* Process card Presence check */
                                    NCIProcessReaderMode(rfIAN.RFProtocol, NxpNciRWOperationEnum.PRESENCE_CHECK);
                                    NCI_PRINT("CARD REMOVED\n");
                                    OnCardRemovedFromField(EventArgs.Empty);
                                    /* Restart the discovery loop */
                                    NCIRestartDiscovery();
                                    ResetHostTimeOut();
                                }
                                catch (Exception ex)
                                {
                                    ResetHostTimeOut();
                                    throw ex;
                                }
                            }
                            else
                            {
                                NCI_PRINT("POLL MODE: Undetermined target\n");
                                /* Restart discovery loop */
                                NCIStopDiscovery();
                                NCIStartDiscovery();
                            }
                        }
                    }
                    catch
                    {
                        //do nothing
                    }
                }
            });
        }

        private void NCIInitialConnect()
        {
            short i = 10;
            ReponseCode ret = CheckDevicePresence();
            while (ret != ReponseCode.STATUS_OK)
            {
                if (i-- == 0)
                    throw new Exception("NCIInitialConnect timeout exception");
                Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                ret = CheckDevicePresence();
            }
            DoCoreInit();
        }

        private void NCICoreConfig()
        {
#if NXP_CORE_CONF
            if (NxpNci_CORE_CONF.Length != 0)
            {
                NxpNci_HostTransceive(NxpNci_CORE_CONF, ref Answer, ref AnswerSize);
                if ((Answer[0] != 0x40) || (Answer[1] != 0x02) || (Answer[3] != 0x00) || (Answer[4] != 0x00)) return NXPNCI_ERROR;
            }
#endif

#if NXP_CORE_CONF_EXTN
            if (NxpNci_CORE_CONF_EXTN.Length != 0)
            {
                NxpNci_HostTransceive(NxpNci_CORE_CONF_EXTN, ref Answer, ref AnswerSize);
                if ((Answer[0] != 0x40) || (Answer[1] != 0x02) || (Answer[3] != 0x00) || (Answer[4] != 0x00)) return NXPNCI_ERROR;
            }
#endif

#if NXP_CORE_STANDBY
            if (NxpNci_CORE_STANDBY.Length != 0)
            {
                NxpNci_HostTransceive(NxpNci_CORE_STANDBY, ref Answer, ref AnswerSize);
                if ((Answer[0] != 0x4F) || (Answer[1] != 0x00) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
            }

#endif

#if NXP_RF_CONF
            CoreSetConfigCommand cmd = new CoreSetConfigCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            CoreSetConfigResponse resp1 = new CoreSetConfigResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            byte configLength = 0;
            if (NfcControllerGeneration == NCIVersionEnum.NCIVERSION1)
            {
                configLength = (byte)NCIConfig.NxpNci_RF_CONF_1stGen.Length;
                if (configLength > 0)
                    cmd.deserialize(NCIConfig.NxpNci_RF_CONF_1stGen);

            }
            else
            {
                configLength = (byte)NCIConfig.NxpNci_RF_CONF_2ndGen.Length;
                if (configLength > 0)
                    cmd.deserialize(NCIConfig.NxpNci_RF_CONF_2ndGen);

            }
            if (configLength != 0)
            {
                SendCommand(cmd, resp1);
                //if ((Answer[0] != 0x40) || (Answer[1] != 0x02) || (Answer[3] != 0x00) || (Answer[4] != 0x00)) return NXPNCI_ERROR;
                if (resp1.Status != ReponseCode.STATUS_OK ||
                    resp1.OpcodeIdentifier != OpcodeCoreIdentifierEnum.CORE_SET_CONFIG_CMD ||
                    resp1.GroupIdentifier != GroupIdentifierEnum.NCI_Core ||
                    resp1.MessageType != PacketTypeEnum.ControlResponse ||
                    resp1.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment ||
                    resp1.getPLL() != 0)
                    throw new Exception("CoreSetConfigResponse returned error:" + resp1.Status);

                /* Reset the NFC Controller to insure RF settings apply */
                CoreResetResponse resp2 = new CoreResetResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                SendCommand(new CoreResetCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment, ResetCommandTypeEnum.RESET_CONFIG), resp2);
                //if ((Answer[0] != 0x40) || (Answer[1] != 0x00) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
                if (resp2.Status != ReponseCode.STATUS_OK ||
                     resp2.OpcodeIdentifier != OpcodeCoreIdentifierEnum.CORE_RESET_CMD ||
                     resp2.GroupIdentifier != GroupIdentifierEnum.NCI_Core ||
                     resp2.MessageType != PacketTypeEnum.ControlResponse ||
                     resp2.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                    throw new Exception("CoreResetCommand returned error:" + resp2.Status);

                CoreInitResponse resp3 = new CoreInitResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                SendCommand(new CoreInitCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment), resp3);
                //if ((Answer[0] != 0x40) || (Answer[1] != 0x01) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
                if (resp3.Status != ReponseCode.STATUS_OK ||
                     resp3.OpcodeIdentifier != OpcodeCoreIdentifierEnum.CORE_INIT_CMD ||
                     resp3.GroupIdentifier != GroupIdentifierEnum.NCI_Core ||
                     resp3.MessageType != PacketTypeEnum.ControlResponse ||
                     resp3.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                    throw new Exception("CoreInitResponse returned error:" + resp3.Status);
            }
#endif
        }

        private void NCIConfigureMode(DeviceModeEnum mode)
        {
            if (mode == 0) return;

            /* Enable Proprietary interface for T4T card presence check procedure */
            if (mode == DeviceModeEnum.NXPNCI_MODE_RW)
            {
                EnableProprietaryExtensionsCommand cmd = new EnableProprietaryExtensionsCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                EnableProprietaryExtensionsResponse resp = new EnableProprietaryExtensionsResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                SendCommand(cmd, resp);
                //if ((Answer[0] != 0x4F) || (Answer[1] != 0x02) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
                if (resp.Status != ReponseCode.STATUS_OK ||
                    resp.OpcodeIdentifier != OpcodeProprietaryExtensionsEnum.NCI_PROPRIETARY_ACT_CMD ||
                    resp.GroupIdentifier != GroupIdentifierEnum.PROPRIETARY ||
                    resp.MessageType != PacketTypeEnum.ControlResponse ||
                    resp.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                    throw new Exception("EnableProprietaryExtensionsResponse error:" + resp.Status);

                /* Building Discovery Map command */
                RFDiscoverMapCommand cmd2 = new RFDiscoverMapCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                RFDiscoverMapResponse resp2 = new RFDiscoverMapResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);

                MappingConfiguration mc1 = new MappingConfiguration() { RFProtocol = RFProtocolEnum.PROT_T1T, RFMode = RFModeEnum.MODE_POLL, RFInterface = RFInterfaceEnum.INTF_FRAME };
                MappingConfiguration mc2 = new MappingConfiguration() { RFProtocol = RFProtocolEnum.PROT_T2T, RFMode = RFModeEnum.MODE_POLL, RFInterface = RFInterfaceEnum.INTF_FRAME };
                MappingConfiguration mc3 = new MappingConfiguration() { RFProtocol = RFProtocolEnum.PROT_T3T, RFMode = RFModeEnum.MODE_POLL, RFInterface = RFInterfaceEnum.INTF_FRAME };
                MappingConfiguration mc4 = new MappingConfiguration() { RFProtocol = RFProtocolEnum.PROT_ISODEP, RFMode = RFModeEnum.MODE_POLL, RFInterface = RFInterfaceEnum.INTF_ISODEP };
                MappingConfiguration mc5 = new MappingConfiguration() { RFProtocol = RFProtocolEnum.PROT_MIFARE, RFMode = RFModeEnum.MODE_POLL, RFInterface = RFInterfaceEnum.PROPRIETARY_START };

                cmd2.MappingConfigurations = new MappingConfiguration[5];
                cmd2.MappingConfigurations[0] = mc1;
                cmd2.MappingConfigurations[1] = mc2;
                cmd2.MappingConfigurations[2] = mc3;
                cmd2.MappingConfigurations[3] = mc4;
                cmd2.MappingConfigurations[4] = mc5;

                SendCommand(cmd2, resp2);
                //if ((Answer[0] != 0x41) || (Answer[1] != 0x00) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
                if (resp2.Status != ReponseCode.STATUS_OK ||
                    resp2.OpcodeIdentifier != OpcodeRFIdentifierEnum.RF_DISCOVER_MAP_CMD ||
                    resp2.GroupIdentifier != GroupIdentifierEnum.RFMANAGEMENT ||
                    resp2.MessageType != PacketTypeEnum.ControlResponse ||
                    resp2.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                    throw new Exception("RFDiscoverMapResponse error:" + resp2.Status);
            }
        }

        private void NCIStartDiscovery()
        {
            RFDiscoverCommand cmd = new RFDiscoverCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);

            DiscoverConfiguration dc1 = new DiscoverConfiguration() { RFTechnologiesAndMode = RFTechnologiesAndModeEnum.NFC_A_PASSIVE_POLL_MODE, RFDiscoverFrequency = 0x01 };
            DiscoverConfiguration dc2 = new DiscoverConfiguration() { RFTechnologiesAndMode = RFTechnologiesAndModeEnum.NFC_B_PASSIVE_POLL_MODE, RFDiscoverFrequency = 0x01 };
            DiscoverConfiguration dc3 = new DiscoverConfiguration() { RFTechnologiesAndMode = RFTechnologiesAndModeEnum.NFC_F_PASSIVE_POLL_MODE, RFDiscoverFrequency = 0x01 };

            cmd.DiscoverConfigurations = new DiscoverConfiguration[3];
            cmd.DiscoverConfigurations[0] = dc1;
            cmd.DiscoverConfigurations[1] = dc2;
            cmd.DiscoverConfigurations[2] = dc3;

            RFDiscoverResponse resp = new RFDiscoverResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            SendCommand(cmd, resp);
            //if ((Answer[0] != 0x41) || (Answer[1] != 0x03) || (Answer[3] != 0x00)) return NXPNCI_ERROR;
            if (resp.Status != ReponseCode.STATUS_OK ||
                resp.OpcodeIdentifier != OpcodeRFIdentifierEnum.RF_DISCOVER_CMD ||
                resp.GroupIdentifier != GroupIdentifierEnum.RFMANAGEMENT ||
                resp.MessageType != PacketTypeEnum.ControlResponse ||
                resp.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                throw new Exception("NCIStartDiscovery error:" + resp.Status);
        }

        private void NCIStopDiscovery()
        {
            RFDeactivateCommand cmd = new RFDeactivateCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
            {
                DeactivationType = DeactivationTypeEnum.IDLE_MODE
            };
            RFDeactivateResponse resp = new RFDeactivateResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);

            SendCommand(cmd, resp);
            if (resp.Status != ReponseCode.STATUS_OK)
                throw new Exception("NCIStopDiscovery error:" + resp.Status);
        }

        private void NCIRestartDiscovery()
        {
            RFDeactivateCommand cmd = new RFDeactivateCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
            {
                DeactivationType = DeactivationTypeEnum.DISCOVERY
            };
            RFDeactivateResponse resp = new RFDeactivateResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);

            SendCommand(cmd, resp);
            if (resp.Status != ReponseCode.STATUS_OK)
                throw new Exception("NCIStopDiscovery error:" + resp.Status);
        }

        private RFManagementNotification NCIWaitForNotification(int timeout)
        {
            NotificationPacket packet;
            do
                packet = WaitForNotification(timeout);
            //if ((AnswerBuffer[0] == 0x61) || (AnswerBuffer[1] == 0x05))
            while (packet.MessageType != PacketTypeEnum.ControlNotification || packet.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment ||
                packet.GroupIdentifier != GroupIdentifierEnum.RFMANAGEMENT ||
                packet.OpcodeIdentifier != (byte)OpcodeRFIdentifierEnum.RF_INTF_ACTIVATED_NTF &&
                packet.OpcodeIdentifier != (byte)OpcodeRFIdentifierEnum.RF_DISCOVER_CMD);

            RFManagementNotification resp;
            if (packet.OpcodeIdentifier == (byte)OpcodeRFIdentifierEnum.RF_DISCOVER_CMD)
                resp = new RFDiscoverNotification(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            else if (packet.OpcodeIdentifier == (byte)OpcodeRFIdentifierEnum.RF_INTF_ACTIVATED_NTF)
                resp = new RFInterfaceActivationNotification(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            else
                throw new Exception("Invalid OpcodeRFIdentifierEnum sequence: " + packet.OpcodeIdentifier);

            resp.deserialize(packet.serialize());//TODO: to many deserialize / serialize?
            NCI_PRINT(resp.ToString());
            return resp;
        }

        private void NCIPresenceCheck(RFProtocolEnum protocol)
        {
            Packet cmd;
            Packet resp;
            DataPacket dataPacket;
            NotificationPacket notificationPacket;
            switch (protocol)
            {
                case RFProtocolEnum.PROT_T1T:
                    cmd = new CorePresenceCheckDataRequest(PacketBoundryFlagEnum.CompleteMessageOrLastSegment,
                        new byte[] { 0x78, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });
                    resp = new CorePresenceCheckDataResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment, 0x00);
                    do
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                        SendCommand(cmd, resp);
                        try
                        {
                            dataPacket = WaitForDataPacket(100);
                        }
                        catch (TMLTimeoutException te)
                        {
                            break; //ok to timeout, means card was removed
                        }
                    }
                    //while ((Answer[0] == 0x00) && (Answer[1] == 0x00));
                    while (dataPacket.MessageType == PacketTypeEnum.Data && dataPacket.PacketBoundryFlag == PacketBoundryFlagEnum.CompleteMessageOrLastSegment &&
                          dataPacket.ConnIdentifier == 0x00);
                    //RFU (1) = 0x00); 
                    break;

                case RFProtocolEnum.PROT_T2T:
                    cmd = new CorePresenceCheckDataRequest(PacketBoundryFlagEnum.CompleteMessageOrLastSegment,
                        new byte[] { 0x30, 0x00 });
                    resp = new CorePresenceCheckDataResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment, 0x00);
                    do
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                        SendCommand(cmd, resp);
                        try
                        {
                            dataPacket = WaitForDataPacket(100);
                        }
                        catch (TMLTimeoutException te)
                        {
                            break; //ok to timeout, means card was removed
                        }
                    }
                    // while ((Answer[0] == 0x00) && (Answer[1] == 0x00) && (Answer[2] == 0x11));
                    while (dataPacket.MessageType == PacketTypeEnum.Data && dataPacket.PacketBoundryFlag == PacketBoundryFlagEnum.CompleteMessageOrLastSegment &&
                          dataPacket.ConnIdentifier == 0x00 &&
                          //RFU (1) = 0x00
                          dataPacket.getPLL() == 0x11);
                    break;

                case RFProtocolEnum.PROT_T3T:
                    cmd = new RFT3TPollingCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                    resp = new RFT3TPollingResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                    // { 0x12, 0xFC, 0x00, 0x01 };
                    ((RFT3TPollingCommand)cmd).SensFReqParams[0] = 0x12;
                    ((RFT3TPollingCommand)cmd).SensFReqParams[1] = 0xFC;
                    ((RFT3TPollingCommand)cmd).SensFReqParams[2] = 0x00;
                    ((RFT3TPollingCommand)cmd).SensFReqParams[3] = 0x01;
                    do
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                        SendCommand(cmd, resp);
                        try
                        {
                            notificationPacket = WaitForNotification(100);
                        }
                        catch (TMLTimeoutException te)
                        {
                            break; //ok to timeout, means card was removed
                        }
                    }
                    //while ((Answer[0] == 0x61) && (Answer[1] == 0x08) && (Answer[3] == 0x00));
                    while (notificationPacket.MessageType == PacketTypeEnum.ControlNotification && notificationPacket.PacketBoundryFlag == PacketBoundryFlagEnum.CompleteMessageOrLastSegment &&
                            notificationPacket.GroupIdentifier == GroupIdentifierEnum.RFMANAGEMENT &&
                            notificationPacket.OpcodeIdentifier == (byte)OpcodeRFIdentifierEnum.RF_T3T_POLLING_CMD &&
                            notificationPacket.getPLL() == 0x00);

                    break;

                case RFProtocolEnum.PROT_ISODEP:
                    cmd = new PresenceCheckISODepCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                    resp = new PresenceCheckISODepResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                    do
                    {
                        Task.Delay(TimeSpan.FromMilliseconds(500)).Wait();
                        SendCommand(cmd, resp);
                        try
                        {
                            notificationPacket = WaitForNotification(100);
                        }
                        catch (TMLTimeoutException te)
                        {
                            break; //ok to timeout, means card was removed
                        }
                    }
                    //while ((Answer[0] == 0x6F) && (Answer[1] == 0x11) && (Answer[2] == 0x01) && (Answer[3] == 0x01));
                    while (resp.MessageType == PacketTypeEnum.ControlNotification && resp.PacketBoundryFlag == PacketBoundryFlagEnum.CompleteMessageOrLastSegment &&
                            notificationPacket.GroupIdentifier == GroupIdentifierEnum.PROPRIETARY &&
                            notificationPacket.OpcodeIdentifier == (byte)OpcodeProprietaryExtensionsEnum.NCI_PROPRIETARY_ISO_DEP_CHECK_CMD &&
                            notificationPacket.getPLL() == 0x01 &&
                            notificationPacket.getPayLoad()[0] == 0x01);
                    break;

            }
        }

        public void NCIProcessReaderMode(RFProtocolEnum protocol, NxpNciRWOperationEnum operation)
        {
            switch (operation)
            {
                case (NxpNciRWOperationEnum.PRESENCE_CHECK):
                    NCIPresenceCheck(protocol);
                    break;
            }
        }

        private RFInterfaceActivationNotification ProcessRFNotification(RFManagementNotification notification)
        {
            RFInterfaceActivationNotification nd = null;
            if (notification is RFInterfaceActivationNotification)
            {
                nd = (RFInterfaceActivationNotification)notification;
            }
            else if (notification is RFDiscoverNotification) //RF_DISCOVER_NTF
            {
                List<RFDiscoverNotification> rfdns = new List<RFDiscoverNotification>();
                RFDiscoverNotification rfdn = (RFDiscoverNotification)notification;
                while (rfdn.DiscoverNotificationType == DiscoverNotificationTypeEnum.MORE_TO_COME)
                {
                    rfdns.Add(rfdn);
                    rfdn = (RFDiscoverNotification)NCIWaitForNotification(100);
                }
                rfdns.Add(rfdn);

                if (rfdns.Count > 1)
                    //need to choose one
                    foreach (RFDiscoverNotification r in rfdns)
                        if (r.RFProtocol == RFProtocolEnum.PROT_ISODEP)
                        {
                            rfdn = r;
                            break;
                        }

                if (rfdn.RFProtocol == RFProtocolEnum.PROT_ISODEP)
                {
                    RFDiscoverSelectCommand cmd2 = new RFDiscoverSelectCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment)
                    {
                        RFDiscoveryId = rfdn.RFDiscoveryId,
                        RFProtocol = rfdn.RFProtocol,
                        RFInterface = RFInterfaceEnum.INTF_ISODEP //INTF_UNDETERMINED
                    };
                    RFDiscoverSelectResponse resp = new RFDiscoverSelectResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
                    SendCommand(cmd2, resp);
                    //if ((AnswerBuffer[0] == 0x41) || (AnswerBuffer[1] == 0x04) || (AnswerBuffer[3] == 0x00))
                    if (resp.MessageType != PacketTypeEnum.ControlResponse ||
                        resp.PacketBoundryFlag != PacketBoundryFlagEnum.CompleteMessageOrLastSegment ||
                        resp.GroupIdentifier != GroupIdentifierEnum.RFMANAGEMENT ||
                        resp.OpcodeIdentifier != OpcodeRFIdentifierEnum.RF_DISCOVER_SELECT_CMD ||
                        resp.Status != ReponseCode.STATUS_OK)

                        nd = (RFInterfaceActivationNotification)NCIWaitForNotification(100);
                }
                else
                    throw new Exception("Invalid RFProtocol");
            }
            return nd;
        }

        private void DoCoreInit()
        {
            CoreInitCommand cmd = new CoreInitCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            CoreInitResponse resp = new CoreInitResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            SendCommand(cmd, resp);
            if (resp.Status != ReponseCode.STATUS_OK)
                throw new Exception("CoreInitResponse returned error:" + resp.Status);
        }

        private ReponseCode CheckDevicePresence()
        {
            CoreResetCommand cmd = new CoreResetCommand(PacketBoundryFlagEnum.CompleteMessageOrLastSegment, ResetCommandTypeEnum.RESET_CONFIG);
            CoreResetResponse resp = new CoreResetResponse(PacketBoundryFlagEnum.CompleteMessageOrLastSegment);
            SendCommand(cmd, resp);
            NfcControllerGeneration = resp.NCIVersion;
            return resp.Status;
        }

        private static void NCI_PRINT(String desc)
        {
#if DEBUG_PRINT
            Logger.Log(desc);
#endif
        }

        private void SendCommand(Packet command, Packet response)
        {
            byte[] bytesToSend = command.serialize();
            int numberBytesReceived = 0;
            NCI_PRINT(command.ToString());
            cardReaderHAL.Transceive(bytesToSend, out byte[] bytesReceived, ref numberBytesReceived);
            response.deserialize(bytesReceived);
            NCI_PRINT(response.ToString());
        }

        private NotificationPacket WaitForNotification(int timeout)
        {
            WaitForData(out byte[] bytesReceived, timeout);
            NotificationPacket response = new NotificationPacket();
            response.deserialize(bytesReceived);
            NCI_PRINT(response.ToString());
            return response;
        }

        private DataPacket WaitForDataPacket(int timeout)
        {
            WaitForData(out byte[] bytesReceived, timeout);
            DataPacket response = new DataPacket();
            response.deserialize(bytesReceived);
            NCI_PRINT(response.ToString());
            return response;
        }

        private void DisplayCardInfo(RFInterfaceActivationNotification rfIAN)
        {
            NCI_PRINT(rfIAN.TechSpecificParam.ToString());
        }

        private void TransceiveData(byte[] pTBuff, out byte[] pRBuff, ref int pBytesread)
        {
            //pack / unpack into a Data Packet 
            DataPacket dpIn = new DataPacket(PacketBoundryFlagEnum.CompleteMessageOrLastSegment, 0x00, pTBuff);

            int bytesRead = 0;
            cardReaderHAL.Transceive(dpIn.serialize(), out byte[] dataOut, ref bytesRead);

            //credits notification
            CoreConnCreditsNotification nf = new CoreConnCreditsNotification();
            nf.deserialize(dataOut);
            NCI_PRINT(nf.ToString());

            //data
            WaitForData(out byte[] bytesReceived, 100);
            DataPacket dpOut = new DataPacket();
            dpOut.deserialize(bytesReceived);
            NCI_PRINT(dpOut.ToString());

            pRBuff = dpOut.getPayLoad();
            pBytesread = dpOut.getPayLoad().Length;
        }

        private void WaitForData(out byte[] bytesReceived, int timeout)
        {
            int numberBytesReceived = 0;
            cardReaderHAL.WaitForReception(out bytesReceived, ref numberBytesReceived, timeout);
        }

       
    }
}
