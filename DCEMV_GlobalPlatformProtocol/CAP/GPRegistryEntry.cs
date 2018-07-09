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
using System;
using System.Text;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPRegistryEntry
    {
        protected AID aid;
        protected int lifecycle;
        protected Kind kind;
        protected AID domain;

        public String toShortString(Kind kind)
        {
            switch (kind)
            {
                case Kind.IssuerSecurityDomain:
                    return "ISD";
                case Kind.Application:
                    return "APP";
                case Kind.SecurityDomain:
                    return "DOM";
                case Kind.ExecutableLoadFile:
                    return "PKG";
                default:
                    throw new Exception("Unknown entry type");
            }
        }


        public AID getAID()
        {
            return aid;
        }
        public void setAID(AID aid)
        {
            this.aid = aid;
        }


        public void setDomain(AID dom)
        {
            this.domain = dom;
        }

        public AID getDomain()
        {
            return domain;
        }
        public void setLifeCycle(int lifecycle)
        {
            this.lifecycle = lifecycle;
        }

        public void setType(Kind type)
        {
            this.kind = type;
        }

        public Kind getType()
        {
            return kind;
        }
        public bool isPackage()
        {
            return kind == Kind.ExecutableLoadFile;
        }
        public bool isApplet()
        {
            return kind == Kind.Application;
        }
        public bool isDomain()
        {
            return kind == Kind.SecurityDomain || kind == Kind.IssuerSecurityDomain;
        }

        public String toString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("AID: " + aid + ", " + lifecycle + ", Kind: " + toShortString(kind));
            return result.ToString();
        }

        public String getLifeCycleString()
        {
            return getLifeCycleString(kind, lifecycle);
        }

        public static String getLifeCycleString(Kind kind, int lifeCycleState)
        {
            switch (kind)
            {
                case Kind.IssuerSecurityDomain:
                    switch (lifeCycleState)
                    {
                        case 0x1:
                            return "OP_READY";
                        case 0x7:
                            return "INITIALIZED";
                        case 0xF:
                            return "SECURED";
                        case 0x7F:
                            return "CARD_LOCKED";
                        case 0xFF:
                            return "TERMINATED";
                        default:
                            return "ERROR (0x" + Formatting.ByteArrayToHexString(BitConverter.GetBytes(lifeCycleState)) + ")";
                    }
                case Kind.Application:
                    if (lifeCycleState == 0x3)
                    {
                        return "INSTALLED";
                    }
                    else if (lifeCycleState <= 0x7F)
                    {
                        if ((lifeCycleState & 0x78) != 0x00)
                        {
                            return "SELECTABLE (0x" + Formatting.ByteArrayToHexString(BitConverter.GetBytes(lifeCycleState)) + ")";
                        }
                        else
                        {
                            return "SELECTABLE";
                        }
                    }
                    else if (lifeCycleState > 0x83)
                    {
                        return "LOCKED";
                    }
                    else
                    {
                        return "ERROR (0x" + Formatting.ByteArrayToHexString(BitConverter.GetBytes(lifeCycleState)) + ")";
                    }
                case Kind.ExecutableLoadFile:
                    // GP 2.2.1 Table 11-3
                    if (lifeCycleState == 0x1)
                    {
                        return "LOADED";
                    }
                    else if (lifeCycleState == 0x00)
                    {
                        // OP201 TODO: remove in v0.5
                        return "LOGICALLY_DELETED";
                    }
                    else
                    {
                        return "ERROR (0x" + Formatting.ByteArrayToHexString(BitConverter.GetBytes(lifeCycleState)) + ")";
                    }
                case Kind.SecurityDomain:
                    // GP 2.2.1 Table 11-5
                    if (lifeCycleState == 0x3)
                    {
                        return "INSTALLED";
                    }
                    else if (lifeCycleState == 0x7)
                    {
                        return "SELECTABLE";
                    }
                    else if (lifeCycleState == 0xF)
                    {
                        return "PERSONALIZED";
                    }
                    else if ((lifeCycleState & 0x83) == 0x83)
                    {
                        return "LOCKED";
                    }
                    else
                    {
                        return "ERROR (0x" + Formatting.ByteArrayToHexString(BitConverter.GetBytes(lifeCycleState)) + ")";
                    }
                default:
                    return "ERROR";
            }
        }
    }
}
