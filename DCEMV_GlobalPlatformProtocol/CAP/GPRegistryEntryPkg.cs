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
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPRegistryEntryPkg : GPRegistryEntry
    {
        private byte[] version;
        private List<AID> modules = new List<AID>();

        public byte[] getVersion()
        {
            return version;
        }
        public String getVersionString()
        {
            if (version != null && version.Length == 2)
            {
                return version[0] + "." + version[1];
            }
            return "<unknown format>";
        }

        public void setVersion(byte[] v)
        {
            version = Arrays.CopyOf(v, v.Length);
        }

        public void addModule(AID aid)
        {
            modules.Add(aid);
        }

        public List<AID> getModules()
        {
            List<AID> r = new List<AID>();
            r.AddRange(modules);
            return r;
        }
    }
}
