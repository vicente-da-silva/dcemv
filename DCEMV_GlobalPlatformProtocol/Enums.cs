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

namespace DCEMV.GlobalPlatformProtocol
{
    public enum APDUMode
    {
        // bit values as expected by EXTERNAL AUTHENTICATE
        CLR = 0x00, MAC = 0x01, ENC = 0x02, RMAC = 0x10
    }
    public enum KeySessionType
    {
        // ID is as used in diversification/derivation
        // That is - one based.
        ENC = 1,
        MAC = 2,
        KEK = 3,
        RMAC = 4
    }
    public enum KeyType
    {
        ANY, DES, DES3, AES
    }
    public class KeyTypeList
    {
        private static List<KeySessionType> list = new List<KeySessionType>();

        public static List<KeySessionType> Values
        {
            get
            {
                return list;
            }
        }

        static KeyTypeList()
        {
            list.Add(KeySessionType.ENC);
            list.Add(KeySessionType.MAC);
            list.Add(KeySessionType.KEK);
            list.Add(KeySessionType.RMAC);
        }
    }
    public enum Diversification
    {
        NONE, VISA2, EMV
    }
    public enum SCPVersions
    {
        SCP_01_05,
        //SCP_01_15,

        //SCP_02_04,
        //SCP_02_05,
        //SCP_02_0A,
        //SCP_02_0B,
        //SCP_02_14,
        SCP_02_15,
        //SCP_02_1A,
        //SCP_02_1B,

        SCP_03
    }
    public enum GPSpec { OP201, GP211, GP22 };
    public enum Kind
    {
        IssuerSecurityDomain, Application, SecurityDomain, ExecutableLoadFile
    }
    public enum Privilege
    {
        SecurityDomain,
        DAPVerification,
        DelegatedManagement,
        CardLock,
        CardTerminate,
        CardReset,
        CVMManagement,
        MandatedDAPVerification,
        TrustedPath,
        AuthorizedManagement,
        TokenVerification,
        GlobalDelete,
        GlobalLock,
        GlobalRegistry,
        FinalApplication,
        GlobalService,
        ReceiptGeneration,
        CipheredLoadFileDataBlock,
        ContactlessActivation,
        ContactlessSelfActivation
    }
}
