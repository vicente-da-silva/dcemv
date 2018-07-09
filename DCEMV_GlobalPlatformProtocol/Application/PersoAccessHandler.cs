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
using System.IO;
using DCEMV.ISO7816Protocol;
using System.Collections.Generic;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    /// <summary>
    /// Access handler class for Desfire based ICC. It provides wrappers for different Desfire 
    /// commands
    /// </summary>
    public class PersoAccessHandler
    {
        public static Logger Logger = new Logger(typeof(PersoAccessHandler));

        private CardQProcessor cardInterface;
        private GlobalPlatform gp;
        
        public PersoAccessHandler(CardQProcessor cardInterface)
        {
            this.cardInterface = cardInterface;
            gp = new GlobalPlatform(cardInterface);
        }
        
        public void AuthWithCard(byte[] mk, byte[] hostChallenge = null)
        {
            GPKey kmc = new GPKey(mk, KeyType.DES3);
            GPPlaintextKeys gpptk = GPPlaintextKeys.FromMasterKey(kmc, Diversification.VISA2);
            gp.OpenSecureChannel(gpptk, new List<APDUMode>() { APDUMode.CLR }, hostChallenge);
        }

        public GPRegistry GetAppList(String aid)
        {
            return gp.getRegistry();
        }

        public void RemoveApp(String aid)
        {
            gp.deleteAID(new AID(aid), true);
        }

        public void LockApp(string instanceAID)
        {
            gp.LockApp(new AID(instanceAID));
        }
        public void UnLockApp(string instanceAID)
        {
            gp.UnLockApp(new AID(instanceAID));
        }

        public String GetCardData()
        {
            return gp.GetData();
        }

        public void LoadCapFile(String aid, MemoryStream capFile)
        {
            gp.InstallForLoad(aid, capFile);
        }

        public void InstallAndMakeSelectable(String packageAID, String appletAID, String instanceId)
        {
            gp.InstallForInstallAndMakeSelectable(packageAID, appletAID, instanceId);
        }
        
        public void InstallForPerso(String instanceId)
        {
            gp.InstallForPerso(instanceId);
        }

        public void StoreDataToApplication(GPStoreData irfi)
        {
            gp.StoreDataToApplication(irfi);
        }
        
        public void SelectApplication(byte[] aid)
        {
            gp.SelectApplication(aid);
        }
        public TLVList DoGPOTest(String data)
        {
            return gp.DoGPOTest(data);
        }
    }
}
