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
    public class CodeBasedConfigurationProvider : IConfigurationProvider
    {
        public string GetExceptionFileXML()
        {
            return CodeData.ExceptionFile;
        }

        public string GetPublicKeyCertificatesXML()
        {
            return CodeData.Certs;
        }

        public string GetRevokedPublicKeyCertificatesXML()
        {
            return CodeData.RevokedCerts;
        }

        public string GetTerminalConfigurationDataXML(string kernelType)
        {
            return CodeData.TerminalConfigurationData;
        }
        public string GetContactTerminalSupportedAIDsXML()
        {
            return CodeData.TerminalSupportedContactAIDs;
        }
        
        public string GetContactlessTerminalSupportedRIDsXML()
        {
            return CodeData.TerminalSupportedContactlessRIDs;
        }

        public string GetKernelConfigurationDataXML(string transactionType)
        {
            return CodeData.KernelConfigurationData;
        }

        public string GetKernel1ConfigurationDataXML(string transactionType)
        {
            return CodeData.Kernel1ConfigurationData;
        }

        public string GetKernel2ConfigurationDataXML(string transactionType)
        {
            return CodeData.Kernel2ConfigurationData;
        }

        public string GetKernel3ConfigurationDataXML(string transactionType)
        {
            return CodeData.Kernel3ConfigurationData;
        }

        public string GetKernel3GlobalConfigurationDataXML()
        {
            return CodeData.Kernel3GlobalConfigurationData;
        }

        public string GetKernel1GlobalConfigurationDataXML()
        {
            return CodeData.Kernel1GlobalConfigurationData;
        }

        public string GetKernelGlobalConfigurationDataXML()
        {
            return CodeData.KernelGlobalConfigurationData;
        }
        
    }
}
