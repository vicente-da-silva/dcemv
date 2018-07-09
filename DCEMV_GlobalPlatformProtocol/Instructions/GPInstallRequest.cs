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
using System.Linq;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.GlobalPlatformProtocol
{
    public class GPInstallRequestBase
    {
        protected byte[] ConvertLengthLength(int length)
        {
            if (length <= 0x80)
            {
                return new byte[] { BitConverter.GetBytes(length).Reverse().ToArray()[3] };
            }
            if (length >= 0x81 && (length - 0x81) <= 0xFF)
            {
                byte val = BitConverter.GetBytes(length - 0x81).Reverse().ToArray()[3];
                return new byte[] { 0x81, val };
            }
            if ((length - 0x81) > 0xFF)
            {
                byte[] vals = new byte[2];
                Array.Copy(BitConverter.GetBytes(length - 0x82).Reverse().ToArray(), 2, vals, 0, 2);
                return Formatting.ConcatArrays(new byte[] { 0x82 }, vals);
            }
            throw new Exception("Invalid length");
        }
        protected uint DeterminaLengthLength(byte[] input, ref int pos)
        {
            uint result;
            if (input[pos] <= 0x80)
            {
                result = Formatting.ConvertToInt32(new byte[] { input[pos] });
                pos++;
                return result;
            }
            if (input[pos] == 0x81)
            {
                result = Formatting.ConvertToInt32(new byte[] { input[pos + 1] }) + input[pos];
                pos = pos + 2;
                return result;
            }
            if (input[pos] == 0x82)
            {
                result = Formatting.ConvertToInt32(new byte[] { input[pos + 1], input[pos + 2] }) + input[pos];
                pos = pos + 3;
                return result;
            }
            throw new Exception("Invalid LengthLength byte");
        }
    }
    public class GPInstallRequestDataForInstall : GPInstallRequestBase
    {
        private byte LengthofExecutableLoadFileAID { get; set; }
        public string ExecutableLoadFileAID { get; set; }
        private byte LengthofExecutableModuleAID { get; set; }
        public string ExecutableModuleAID { get; set; }
        private byte LengthofApplicationAID { get; set; }
        public string ApplicationAID { get; set; }
        private byte LengthofPrivileges { get; set; }
        public byte[] Privileges { get; set; }
        private int LengthofInstallParametersfield { get; set; }
        private byte[] InstallParametersfield { get; set; }
        private int LengthofInstallToken { get; set; }
        public byte[] InstallToken { get; set; }

        public INSTALL_PARAM_C9_GP InstallParamC9 { get; set; }

        public GPInstallRequestDataForInstall()
        {
            Privileges = new byte[0];
            InstallParametersfield = new byte[0];
            InstallToken = new byte[0];
        }
       

        public virtual byte[] Serialize()
        {
            if (InstallParamC9 != null)
                InstallParametersfield = InstallParamC9.Serialize();
            else
                InstallParametersfield = new byte[2] { 0xC9, 0x00 };

            return Formatting.ConcatArrays(
                new byte[] { BitConverter.GetBytes(ExecutableLoadFileAID.Length/2)[0] },
                Formatting.HexStringToByteArray(ExecutableLoadFileAID),
                new byte[] { BitConverter.GetBytes(ExecutableModuleAID.Length/2)[0] },
                Formatting.HexStringToByteArray(ExecutableModuleAID),
                new byte[] { BitConverter.GetBytes(ApplicationAID.Length/2)[0] },
                Formatting.HexStringToByteArray(ApplicationAID),
                new byte[] { BitConverter.GetBytes(Privileges.Length)[0] },
                Privileges,

                ConvertLengthLength(InstallParametersfield.Length),
                InstallParametersfield,
                ConvertLengthLength(InstallToken.Length),
                InstallToken
                );
        }

        public virtual void Deserialize(byte[] input)
        {
            int pos = 0;
            LengthofExecutableLoadFileAID = input[pos];
            pos++;

            byte[] loadFileAIDBytes = new byte[LengthofExecutableLoadFileAID];
            Array.Copy(input, pos, loadFileAIDBytes, 0, LengthofExecutableLoadFileAID);
            ExecutableLoadFileAID = Formatting.ByteArrayToHexString(loadFileAIDBytes);
            pos = pos + LengthofExecutableLoadFileAID;

            LengthofExecutableModuleAID = input[pos];
            pos++;

            byte[] securityDomainAIDBytes = new byte[LengthofExecutableModuleAID];
            Array.Copy(input, pos, securityDomainAIDBytes, 0, LengthofExecutableModuleAID);
            ExecutableModuleAID = Formatting.ByteArrayToHexString(securityDomainAIDBytes);
            pos = pos + LengthofExecutableModuleAID;

            LengthofApplicationAID = input[pos];
            pos++;

            byte[] applicationAIDBytes = new byte[LengthofApplicationAID];
            Array.Copy(input, pos, applicationAIDBytes, 0, LengthofApplicationAID);
            ApplicationAID = Formatting.ByteArrayToHexString(applicationAIDBytes);
            pos = pos + LengthofApplicationAID;

            LengthofPrivileges = input[pos];
            pos++;

            Privileges = new byte[LengthofPrivileges];
            Array.Copy(input, pos, Privileges, 0, LengthofPrivileges);
            pos = pos + LengthofPrivileges;

            LengthofInstallParametersfield = (int)DeterminaLengthLength(input, ref pos);

            InstallParametersfield = new byte[LengthofInstallParametersfield];
            Array.Copy(input, pos, InstallParametersfield, 0, LengthofInstallParametersfield);
            pos = pos + LengthofInstallParametersfield;

            if (LengthofInstallParametersfield > 2)
            {
                InstallParamC9 = new INSTALL_PARAM_C9_GP();
                InstallParamC9.Deserialize(InstallParametersfield, 0);
            }

            LengthofInstallToken = (int)DeterminaLengthLength(input, ref pos);

            InstallToken = new byte[LengthofInstallToken];
            Array.Copy(input, pos, InstallToken, 0, LengthofInstallToken);
            pos = pos + LengthofInstallToken;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("{0,-30}:{1}", "ExecutableLoadFileAID", ExecutableLoadFileAID));
            sb.AppendLine(string.Format("{0,-30}:{1}", "ExecutableModuleAID" , ExecutableModuleAID));
            sb.AppendLine(string.Format("{0,-30}:{1}", "ApplicationAID" , ApplicationAID));
            sb.AppendLine(string.Format("{0,-30}:{1}", "Privileges" , Formatting.ByteArrayToHexString(Privileges)));
            if(InstallParamC9!=null)
                sb.AppendLine(InstallParamC9.ToString());
            else
                sb.AppendLine(string.Format("{0,-30}:{1}", "InstallParamC9", "Empty"));
            sb.Append(string.Format("{0,-30}:{1}", "InstallToken" , Formatting.ByteArrayToHexString(InstallToken)));
            return sb.ToString();
        }
    }
    public class GPInstallRequestDataForLoad : GPInstallRequestBase
    {
        byte LengthofLoadFileAID { get; set; }
        string LoadFileAID { get; set; }
        byte LengthofSecurityDomainAID { get; set; }
        string SecurityDomainAID { get; set; }
        byte LengthofLoadFileDataBlockHash { get; set; }
        byte[] LoadFileDataBlockHash { get; set; }
        int  LengthofLoadParametersfield { get; set; }
        byte[] LoadParametersfield { get; set; }
        int LengthofLoadToken { get; set; }
        byte[] LoadToken { get; set; }

        public virtual byte[] Serialize()
        {

            return Formatting.ConcatArrays(
                new byte[] { BitConverter.GetBytes(LoadFileAID.Length/2)[0] },
                Formatting.HexStringToByteArray(LoadFileAID),
                new byte[] { BitConverter.GetBytes(SecurityDomainAID.Length/2)[0] },
                Formatting.HexStringToByteArray(SecurityDomainAID),
                ConvertLengthLength(LoadFileDataBlockHash.Length),
                LoadFileDataBlockHash,
                ConvertLengthLength(LoadParametersfield.Length),
                LoadParametersfield,
                ConvertLengthLength(LoadToken.Length),
                LoadToken
                );
        }

        public virtual void Deserialize(byte[] input)
        {
            int pos = 0;
            LengthofLoadFileAID = input[pos];
            pos++;

            byte[] loadFileAIDBytes = new byte[LengthofLoadFileAID];
            Array.Copy(input, pos, loadFileAIDBytes, 0, LengthofLoadFileAID);
            LoadFileAID = Formatting.ByteArrayToHexString(loadFileAIDBytes);
            pos = pos + LengthofLoadFileAID;

            LengthofSecurityDomainAID = input[pos];
            pos++;

            byte[] securityDomainAIDBytes = new byte[LengthofSecurityDomainAID];
            Array.Copy(input, pos, securityDomainAIDBytes, 0, LengthofSecurityDomainAID);
            SecurityDomainAID = Formatting.ByteArrayToHexString(securityDomainAIDBytes);
            pos = pos + LengthofSecurityDomainAID;

            LengthofLoadFileDataBlockHash = input[pos];
            pos++;

            LoadFileDataBlockHash = new byte[LengthofLoadFileDataBlockHash];
            Array.Copy(input, pos, LoadFileDataBlockHash, 0, LengthofLoadFileDataBlockHash);
            pos = pos + LengthofLoadFileDataBlockHash;

            LengthofLoadParametersfield = (int)DeterminaLengthLength(input, ref pos);

            LoadParametersfield = new byte[LengthofLoadParametersfield];
            Array.Copy(input, pos, LoadParametersfield, 0, LengthofLoadParametersfield);
            pos = pos + LengthofLoadParametersfield;

            LengthofLoadToken = (int)DeterminaLengthLength(input, ref pos);

            byte[] LoadToken = new byte[LengthofLoadToken];
            Array.Copy(input, pos, LoadToken, 0, LengthofLoadToken);
            pos = pos + LengthofLoadToken;
        }
    }

    public enum InstallRequestP1Enum
    {
        LastOrOnlyCommand = 0x00,
        MoreInstallCommands = 0x80,
        ForRegistryUpdate = 0x40,
        ForPersonalization = 0x20,
        ForExtradition = 0x10,
        ForMakeSelectable = 0x08,
        ForInstall = 0x04,
        ForLoad = 0x02,
    }
    public class GPInstallRequest : GPCommand
    {
        public GPInstallRequest()
        {
        }

        public GPInstallRequest(byte p1) 
            : base(ISO7816Protocol.Cla.ProprietaryCla8x, GPInstructionEnum.Install, null, p1, 0x00)
        {
            ApduResponseType = typeof(GPInstallResponse);
        }

        public GPInstallRequest(GPInstructionEnum ins, byte[] data, byte p1, byte p2)
            :base(ISO7816Protocol.Cla.ProprietaryCla8x, ins, data,p1,p2)
        {
            ApduResponseType = typeof(GPInstallResponse);
        }
    }
    public class GPInstallResponse : GPResponse
    {
        //private TLV tlvResponse;

        public override void Deserialize(byte[] response)
        {
            base.Deserialize(response);
        }
       
        protected override TLV GetTLVResponse()
        {
            return null;// tlvResponse;
        }
    }
}
