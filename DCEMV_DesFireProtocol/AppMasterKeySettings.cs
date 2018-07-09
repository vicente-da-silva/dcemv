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
using System.Collections;

namespace DCEMV.DesFireProtocol
{
    public enum ChangeKeyAccessRightsEnum
    {
        ApplicationMasterKeyAuthenticationIsNecessaryToChangeAnyKey,
        AuthenticationWithTheKeyToBeChanged_SameKeyNo_IsNecessaryToChangeAKey,
        AllKeys_ExceptAppMasterKeySeeBit0AreFrozen,
        AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey
    }
    public class ChangeKeyAccessRights
    {
        public ChangeKeyAccessRightsEnum ChangeKeyAccessRightsType { get; set; }
        public byte KeyNoFor_AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey { get; set; }

        public BitArray getValue()
        {
            switch(ChangeKeyAccessRightsType)
            {
                case ChangeKeyAccessRightsEnum.ApplicationMasterKeyAuthenticationIsNecessaryToChangeAnyKey:
                    return new BitArray(new byte[] { 0x00 });

                case ChangeKeyAccessRightsEnum.AuthenticationWithTheKeyToBeChanged_SameKeyNo_IsNecessaryToChangeAKey:
                    return new BitArray(new byte[] { 0x0E }); 

                case ChangeKeyAccessRightsEnum.AllKeys_ExceptAppMasterKeySeeBit0AreFrozen:
                    return new BitArray(new byte[] { 0x0F });

                case ChangeKeyAccessRightsEnum.AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey:
                    if(KeyNoFor_AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey == 0x00)
                    {
                        throw new Exception("Invalid KeyNoFor_AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey");
                    }
                    return new BitArray(new byte[] { KeyNoFor_AuthenticationWithTheSpecifiedKeyIsNecessaryToChangeAnyKey });
                default:
                    throw new Exception("Invalid ChangeKeyAccessRightsType");
            }
        }
    }

    public class AppMasterKeySettings
    {
        public bool Bit3_ConfigurationChangeable { get; set; }
        public bool Bit2_FreeCreateDeleteFileWithoutMasterKey { get; set; }
        public bool Bit1_FreeDirectoryListAccessWithoutMasterKey { get; set; }
        public bool Bit0_AllowChangeMasterKey { get; set; }
        public ChangeKeyAccessRights Bit4_Bit7_ChangeKeyAccessRights { get; set; }

        public byte getValue()
        {
            BitArray ckar = Bit4_Bit7_ChangeKeyAccessRights.getValue();

            BitArray ba = new BitArray(new bool[] {
                ckar.Get(3),
                ckar.Get(2),
                ckar.Get(1),
                ckar.Get(0),
                Bit3_ConfigurationChangeable,
                Bit2_FreeCreateDeleteFileWithoutMasterKey,
                Bit1_FreeDirectoryListAccessWithoutMasterKey,
                Bit0_AllowChangeMasterKey,
            });
            return Util.ConvertToByte(ba);
        }
    }
}
