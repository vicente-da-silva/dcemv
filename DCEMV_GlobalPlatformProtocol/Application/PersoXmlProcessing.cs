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
using DCEMV.ISO7816Protocol;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class PersoProcessing
    {
        private PersoAccessHandler persoAccessHandler;
        private byte[] cardChallenge;

        public PersoProcessing(CardQProcessor cardInterface)
        {
            cardChallenge = Formatting.HexStringToByteArray("1122334455667788");
            persoAccessHandler = new PersoAccessHandler(cardInterface);
        }

        public GPRegistry GetAppList(string secDomainAID, string masterKey)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            return persoAccessHandler.GetAppList(secDomainAID);
        }

        public void RemoveApp(string secDomainAID, string masterKey,string packageAID)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            //persoAccessHandler.RemoveApp("A000000060"); //will remove dependencies
            //persoAccessHandler.RemoveApp("A000000050"); //will remove dependencies
            persoAccessHandler.RemoveApp(packageAID); 
        }
        public void LockApp(string secDomainAID, string masterKey, string instanceAID)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            persoAccessHandler.LockApp(instanceAID);
        }
        public void UnLockApp(string secDomainAID, string masterKey, string instanceAID)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            persoAccessHandler.UnLockApp(instanceAID);
        }
        
        public TLVList TestApp(String instanceId)
        {
            //persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray("325041592E5359532E4444463031"));
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(instanceId));// "A000000050010101"));
            return persoAccessHandler.DoGPOTest("000000000100" + "9264B836" + "20000000");
        }

        public String GetCardData()
        {
            return persoAccessHandler.GetCardData();
        }

        public void LoadCapFile(MemoryStream capFile,string secDomainAID, string masterKey)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            persoAccessHandler.LoadCapFile(secDomainAID, capFile);//gets package aid A000000060 from cap file
        }

        public void InstallAndMakeSelectable(string secDomainAID,string masterKey, string packageAid, string execModuleAid,string instanceAid)
        {
            persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
            persoAccessHandler.AuthWithCard(Formatting.HexStringToByteArray(masterKey), cardChallenge);
            //persoAccessHandler.InstallAndMakeSelectable("A000000060", "A0000000600101", "325041592E5359532E4444463031");//A000000060 and A0000000600101 must match cap file , 2PAY.SYS.DDF01
            //persoAccessHandler.InstallAndMakeSelectable("A000000050", "A0000000500101", "A000000050010101");//A000000050 and A0000000500101 must match cap file , A000000050010101
            persoAccessHandler.InstallAndMakeSelectable(packageAid, execModuleAid, instanceAid);//A000000050 and A0000000500101 must match cap file , A000000050010101
        }
        
        public void PersoFromXml(string xml, string secDomainAID, string masterKey)
        {
            perso perso = XMLUtil<perso>.Deserialize(xml);

            byte[] masterKeyBytes = Formatting.HexStringToByteArray(masterKey);

            perso.application.ToList().ForEach(app =>
            {
                installType it = app.commands.install;
                INSTALL_PARAM_C9_GP ip = null;
                if (it.tokens != null)
                {
                    if (it.tokens.tlvxml != null)
                    {
                        ip = new INSTALL_PARAM_C9_GP();
                        valueC9Type c9 = it.tokens.tlvxml.valueC9;
                        if (c9.C9_ApplicationInstanceSpecified) ip.Value.ApplicationInstance = MapC9_ApplicationInstanceEnum(c9.C9_ApplicationInstance);
                        if (c9.C9_PinSharingSpecified) ip.Value.PinSharing = MapC9_PinSharingEnum(c9.C9_PinSharing);
                        if (c9.C9_InterfacesAvailableSpecified) ip.Value.InterfacesAvailable = MapC9_InterfacesAvailableEnum(c9.C9_InterfacesAvailable);
                    }
                }

                GPInstallRequestDataForInstall irfi = new GPInstallRequestDataForInstall()
                {
                    ExecutableLoadFileAID = it.ExecutableLoadFileAID,
                    ExecutableModuleAID = it.ExecutableModuleAID,
                    ApplicationAID = it.ApplicationAID,
                    Privileges = Formatting.HexStringToByteArray(it.Privileges),
                    InstallToken = Formatting.HexStringToByteArray(it.InstallToken),
                    InstallParamC9 = ip,
                };

                persoAccessHandler.SelectApplication(Formatting.HexStringToByteArray(secDomainAID));
                //persoAccessHandler.AuthWithCard(masterKey);
                persoAccessHandler.AuthWithCard(masterKeyBytes, cardChallenge);

                persoAccessHandler.InstallForPerso(irfi.ApplicationAID);
                byte counter = 0;
                List<storeDataType> storeCommands = app.commands.storeData.ToList();
                storeCommands.ForEach(command =>
                {
                    bool isLast = (storeCommands.Count - 1) == counter;

                    GPStoreData sd = new GPStoreData
                    {
                        DGI = Formatting.HexStringToByteArray(command.DGI),
                        DataBlock = counter,
                        IsLastBlock = isLast,
                    };
                    if (command.data != null)
                    {
                        sd.DataBytes = Formatting.HexStringToByteArray(command.data);
                    }
                    else
                    {
                        sd.Data = DeserList(command.tlvxml);
                    }

                    persoAccessHandler.StoreDataToApplication(sd);
                    counter++;
                });
            });
        }

        public static void GPPersoFromXmlTest(string xml)
        {
            perso perso = XMLUtil<perso>.Deserialize(xml);

            perso.application.ToList().ForEach(app =>
            {
                installType it = app.commands.install;
                INSTALL_PARAM_C9_GP ip = null;
                if (it.tokens != null)
                {
                    if (it.tokens.tlvxml != null)
                    {
                        ip = new INSTALL_PARAM_C9_GP();
                        valueC9Type c9 = it.tokens.tlvxml.valueC9;
                        if (c9.C9_ApplicationInstanceSpecified) ip.Value.ApplicationInstance = MapC9_ApplicationInstanceEnum(c9.C9_ApplicationInstance);
                        if (c9.C9_PinSharingSpecified) ip.Value.PinSharing = MapC9_PinSharingEnum(c9.C9_PinSharing);
                        if (c9.C9_InterfacesAvailableSpecified) ip.Value.InterfacesAvailable = MapC9_InterfacesAvailableEnum(c9.C9_InterfacesAvailable);
                    }
                }

                GPInstallRequestDataForInstall irfi = new GPInstallRequestDataForInstall()
                {
                    ExecutableLoadFileAID = it.ExecutableLoadFileAID,
                    ExecutableModuleAID = it.ExecutableModuleAID,
                    ApplicationAID = it.ApplicationAID,
                    Privileges = Formatting.HexStringToByteArray(it.Privileges),
                    InstallToken = Formatting.HexStringToByteArray(it.InstallToken),
                    InstallParamC9 = ip,
                };

                //select
                //auth
                GPInstallRequest ir = new GPInstallRequest((byte)InstallRequestP1Enum.LastOrOnlyCommand | (byte)InstallRequestP1Enum.ForMakeSelectable | (byte)InstallRequestP1Enum.ForInstall)
                {
                    CommandData = irfi.Serialize()
                };
                System.Diagnostics.Debug.WriteLine(ir.ToPrintString());

                //select
                //auth
                app.commands.storeData.ToList().ForEach(command =>
                {
                    GPStoreData sd = new GPStoreData
                    {
                        DGI = Formatting.HexStringToByteArray(command.DGI)
                    };
                    if (command.data != null)
                    {
                        sd.DataBytes = Formatting.HexStringToByteArray(command.data);
                    }
                    else
                    {
                        sd.Data = DeserList(command.tlvxml);
                    }
                    GPStoreDataReqest sdr = new GPStoreDataReqest
                    {
                        CommandData = sd.Serialize()
                    };
                    System.Diagnostics.Debug.WriteLine(sdr.ToPrintString());
                });
            });
        }
        private static C9_ApplicationInstance MapC9_ApplicationInstanceEnum(C9_ApplicationInstanceEnum input)
        {
            switch (input)
            {
                case C9_ApplicationInstanceEnum.Alias:
                    return C9_ApplicationInstance.Alias;
                case C9_ApplicationInstanceEnum.Main:
                    return C9_ApplicationInstance.Main;
                default:
                    throw new Exception("unknown C9_ApplicationInstanceEnum:" + input);
            }
        }
        private static C9_PinSharing MapC9_PinSharingEnum(C9_PinSharingEnum input)
        {
            switch (input)
            {
                case C9_PinSharingEnum.GlobalPinSharing:
                    return C9_PinSharing.GlobalPinSharing;
                case C9_PinSharingEnum.NoPinSharingOrAliasNotApplicable:
                    return C9_PinSharing.NoPinSharingOrAliasNotApplicable;
                case C9_PinSharingEnum.PinSharingBetweenInstances:
                    return C9_PinSharing.PinSharingBetweenInstances;
                default:
                    throw new Exception("unknown C9_PinSharingEnum:" + input);
            }
        }
        private static C9_InterfacesAvailable MapC9_InterfacesAvailableEnum(C9_InterfacesAvailableEnum input)
        {
            switch (input)
            {
                case C9_InterfacesAvailableEnum.ContactOnly:
                    return C9_InterfacesAvailable.ContactOnly;
                case C9_InterfacesAvailableEnum.DualInterfaceContactAndContactless:
                    return C9_InterfacesAvailable.DualInterfaceContactAndContactless;
                case C9_InterfacesAvailableEnum.DualInterfaceContactlessOnly:
                    return C9_InterfacesAvailable.DualInterfaceContactlessOnly;
                case C9_InterfacesAvailableEnum.DualInterfaceContactOnly:
                    return C9_InterfacesAvailable.DualInterfaceContactOnly;
                default:
                    throw new Exception("unknown C9_InterfacesAvailableEnum:" + input);
            }
        }
        private static TLVList DeserList(tlvXMLType[] serXML)
        {
            TLVList result = new TLVList();
            serXML.ToList().ForEach((x) =>
            {
                TLV tlvToAdd = TLV.Create(x.tag.Name);
                //x.tag.Description = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(tlvToAdd.Tag.TagLable);
                result.AddToList(tlvToAdd);
                if (x.children != null)
                    tlvToAdd.Children.AddListToList(DeserList(x.children));
                else
                {
                    if (x.value != null)
                    {

                        if (x.value.Formatting == FormattingEnum.ASCII)
                            tlvToAdd.Value = Formatting.ASCIIStringToByteArray(x.value.Value);
                        else
                            tlvToAdd.Value = Formatting.HexStringToByteArray(x.value.Value);
                    }
                    if (x.valueC9 != null)
                    {

                    }
                }
            });
            return result;
        }
    }
}
