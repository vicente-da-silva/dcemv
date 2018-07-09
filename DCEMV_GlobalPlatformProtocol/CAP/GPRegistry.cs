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
using System.Collections.Generic;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPRegistry
    {
        public bool tags = true;
        public Dictionary<AID, GPRegistryEntry> entries = new Dictionary<AID, GPRegistryEntry>();

        public void add(GPRegistryEntry entry)
        {
            // "fix" the kind at a single location.
            if (entry is GPRegistryEntryApp)
            {
                GPRegistryEntryApp app = (GPRegistryEntryApp)entry;
                if (app.getPrivileges().has(Privilege.SecurityDomain) && entry.getType() == Kind.Application)
                {
                    entry.setType(Kind.SecurityDomain);
                }
            }
            entries.Add(entry.getAID(), entry);
        }
        public List<GPRegistryEntryPkg> allPackages()
        {
            List<GPRegistryEntryPkg> res = new List<GPRegistryEntryPkg>();
            foreach (GPRegistryEntry e in entries.Values)
            {
                if (e.isPackage())
                {
                    res.Add((GPRegistryEntryPkg)e);
                }
            }
            return res;
        }
        public List<AID> allPackageAIDs()
        {
            List<AID> res = new List<AID>();
            foreach (GPRegistryEntry e in entries.Values)
            {
                if (e.isPackage())
                {
                    res.Add(e.getAID());
                }
            }
            return res;
        }
        public List<AID> allAppletAIDs()
        {
            List<AID> res = new List<AID>();
            foreach (GPRegistryEntry e in entries.Values)
            {
                if (e.isApplet())
                {
                    res.Add(e.getAID());
                }
            }
            return res;
        }
        public List<AID> allAIDs()
        {
            List<AID> res = new List<AID>();
            foreach (GPRegistryEntry e in entries.Values)
            {
                res.Add(e.getAID());
            }
            return res;
        }
        public List<GPRegistryEntryApp> allApplets()
        {
            List<GPRegistryEntryApp> res = new List<GPRegistryEntryApp>();
            foreach (GPRegistryEntry e in entries.Values)
            {
                if (e.isApplet())
                {
                    res.Add((GPRegistryEntryApp)e);
                }
            }
            return res;
        }
        public AID getDefaultSelectedPackageAID()
        {
            AID defaultAID = getDefaultSelectedAID();
            if (defaultAID != null)
            {
                foreach (GPRegistryEntryPkg e in allPackages())
                {
                    if (e.getModules().Contains(defaultAID))
                        return e.getAID();
                }
                // Did not get a hit. Loop packages and look for prefixes
                foreach (GPRegistryEntryPkg e in allPackages())
                {
                    if (defaultAID.ToString().StartsWith(e.getAID().ToString()))
                        return e.getAID();
                }
            }
            return null;
        }
        public AID getDefaultSelectedAID()
        {
            foreach (GPRegistryEntryApp e in allApplets())
            {
                if (e.getPrivileges().has(Privilege.CardReset))
                {
                    return e.getAID();
                }
            }
            return null;
        }
        public GPRegistryEntryApp getISD()
        {
            foreach (GPRegistryEntryApp a in allApplets())
            {
                if (a.getType() == Kind.IssuerSecurityDomain)
                {
                    return a;
                }
            }
            // Could happen if the registry is a view from SSD
            return null;
        }

        public void parse(int p1, byte[] data, Kind type, GPSpec spec)
        {
            populate_tags(data, type);
        }
        private void populate_tags(byte[] data, Kind type)
        {
            TLVList tlvList = new TLVList();
            tlvList.Deserialize(data, true);
            foreach (TLV tlv in tlvList)//each E3
            {
                AID aid = new AID(tlv.Children.Get("4F").Value);
                AID domain = null;
                if (tlv.Children.IsPresent("CC"))
                    domain = new AID(tlv.Children.Get("CC").Value);
                
                if (type == Kind.ExecutableLoadFile)
                {
                    GPRegistryEntryPkg pkg = new GPRegistryEntryPkg();
                    pkg.setType(type);
                    pkg.setAID(aid);
                    pkg.setDomain(domain);
                    pkg.setVersion(tlv.Children.Get("CE").Value);
                    
                    foreach (TLV tlv84 in tlv.Children)
                    {
                        if (tlv84.Tag.TagLable == "84")
                        {
                            AID a = new AID(tlv84.Value);
                            pkg.addModule(a);
                        }
                    }
                    pkg.setLifeCycle(tlv.Children.Get("9F70").Value[0] & 0xFF);

                    add(pkg);

                }
                else
                {
                    GPRegistryEntryApp app = new GPRegistryEntryApp();
                    app.setType(type);
                    app.setAID(aid);
                    app.setDomain(domain);

                    Privileges privs = Privileges.fromBytes(tlv.Children.Get("C5").Value);
                    app.setPrivileges(privs);

                    if (tlv.Children.IsPresent("C4"))
                    {
                        AID a = new AID(tlv.Children.Get("C4").Value);
                        app.setLoadFile(a);
                    }

                    app.setLifeCycle(tlv.Children.Get("9F70").Value[0] & 0xFF);

                    add(app);
                }
            }
        }
    }
}
