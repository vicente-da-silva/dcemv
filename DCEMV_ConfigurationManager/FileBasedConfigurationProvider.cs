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

namespace DCEMV.ConfigurationManager
{
    public class FileBasedConfigurationProvider : IConfigurationProvider
    {
        private const string EXCEPTION_FILE = "ExceptionFile.xml";

        private const string PUBLIC_KEY_CERTIFICATES_FILE = "PublicKeys.xml";
        private const string REVOKED_PUBLIC_KEY_CERTIFICATES_FILE = "RevokedPublicKeys.xml";

        private const string TERMINAL_SUPPORTED_CONTACTLESS_RID_FILE = "TerminalSupportedContactlessRIDs.xml";
        private const string TERMINAL_SUPPORTED_CONTACT_AID_FILE = "TerminalSupportedContactAIDs.xml";
        private const string TERMINAL_CONFIGURATION_DATA_FILE = "TerminalConfigurationData.xml";

        private const string KERNEL_CONFIGURATION_DATA_FILE = "KernelConfigurationData.xml";
        private const string KERNEL1_CONFIGURATION_DATA_FILE = "Kernel1ConfigurationData.xml";
        private const string KERNEL2_CONFIGURATION_DATA_FILE = "Kernel2ConfigurationData.xml";
        private const string KERNEL3_CONFIGURATION_DATA_FILE = "Kernel3ConfigurationData.xml";

        private const string KERNEL1_GLOBAL_CONFIGURATION_DATA_FILE = "Kernel1GlobalConfigurationData.xml";
        private const string KERNEL3_GLOBAL_CONFIGURATION_DATA_FILE = "Kernel3GlobalConfigurationData.xml";
        private const string KERNEL_GLOBAL_CONFIGURATION_DATA_FILE = "KernelGlobalConfigurationData.xml";

        private IFileDriver fileDriver;

        public FileBasedConfigurationProvider(IFileDriver fileDriver)
        {
            this.fileDriver = fileDriver;
        }
        private string WriteFileIfNotFound(string path, string data)
        {
            string dataRead = fileDriver.LoadFile(path);
            if (dataRead == null)
            {
                fileDriver.SaveFile(path, data);
                return fileDriver.LoadFile(path);
            }
            else
                return dataRead;
        }
        public string GetContactlessTerminalSupportedRIDsXML()
        {
            return WriteFileIfNotFound(TERMINAL_SUPPORTED_CONTACTLESS_RID_FILE, CodeData.TerminalSupportedContactlessRIDs);
        }

        public string GetContactTerminalSupportedAIDsXML()
        {
            return WriteFileIfNotFound(TERMINAL_SUPPORTED_CONTACT_AID_FILE, CodeData.TerminalSupportedContactAIDs);
        }

        public string GetExceptionFileXML()
        {
            return WriteFileIfNotFound(EXCEPTION_FILE, CodeData.ExceptionFile);
        }

        public string GetPublicKeyCertificatesXML()
        {
            return WriteFileIfNotFound(PUBLIC_KEY_CERTIFICATES_FILE, CodeData.Certs);
        }

        public string GetRevokedPublicKeyCertificatesXML()
        {
            return WriteFileIfNotFound(REVOKED_PUBLIC_KEY_CERTIFICATES_FILE, CodeData.RevokedCerts);
        }

        public string GetTerminalConfigurationDataXML(string kernelType)
        {
            return WriteFileIfNotFound(TERMINAL_CONFIGURATION_DATA_FILE, CodeData.TerminalConfigurationData);
        }

        public string GetKernelConfigurationDataXML(string transactionType)
        {
            return WriteFileIfNotFound(KERNEL_CONFIGURATION_DATA_FILE, CodeData.KernelConfigurationData);
        }

        public string GetKernel1ConfigurationDataXML(string transactionType)
        {
            return WriteFileIfNotFound(KERNEL1_CONFIGURATION_DATA_FILE, CodeData.Kernel1ConfigurationData);
        }

        public string GetKernel2ConfigurationDataXML(string transactionType)
        {
            return WriteFileIfNotFound(KERNEL2_CONFIGURATION_DATA_FILE, CodeData.Kernel2ConfigurationData);
        }

        public string GetKernel3ConfigurationDataXML(string transactionType)
        {
            return WriteFileIfNotFound(KERNEL3_CONFIGURATION_DATA_FILE, CodeData.Kernel3ConfigurationData);
        }

        public string GetKernel3GlobalConfigurationDataXML()
        {
            return WriteFileIfNotFound(KERNEL3_GLOBAL_CONFIGURATION_DATA_FILE, CodeData.Kernel3GlobalConfigurationData);
        }

        public string GetKernel1GlobalConfigurationDataXML()
        {
            return WriteFileIfNotFound(KERNEL1_GLOBAL_CONFIGURATION_DATA_FILE, CodeData.Kernel1GlobalConfigurationData);
        }

        public string GetKernelGlobalConfigurationDataXML()
        {
            return WriteFileIfNotFound(KERNEL_GLOBAL_CONFIGURATION_DATA_FILE, CodeData.KernelGlobalConfigurationData);
        }

    }
}
