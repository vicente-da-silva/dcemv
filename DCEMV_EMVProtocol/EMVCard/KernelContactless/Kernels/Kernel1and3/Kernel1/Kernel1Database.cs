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
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels.K1
{
    public class Kernel1Configuration
    {
        public bool VLPTerminalSupportIndicator { get; set; }

        public Kernel1Configuration()
        {
            VLPTerminalSupportIndicator = true; //online/offline support
        }
    }

    public class Kernel1Database : KernelDatabaseBase
    {
        public EntryPointPreProcessingIndicators ProcessingIndicatorsForSelected { get; set; }

        public Kernel1Configuration Kernel1Configuration { get; set; }
        
        public Kernel1Database(EntryPointPreProcessingIndicators processingIndicatorsForSelected, PublicKeyCertificateManager publicKeyCertificateManager)
            :base(publicKeyCertificateManager)
        {
            ProcessingIndicatorsForSelected = processingIndicatorsForSelected;
        }

        protected override void LoadKernelDefaultConfigurationDataObjects(TransactionTypeEnum transactionTypeEnum, IConfigurationProvider configProvider)
        {
            Logger.Log("Using Global Kernel 1 Values:");
            Kernel1Configuration = XMLUtil<Kernel1Configuration>.Deserialize(configProvider.GetKernel1GlobalConfigurationDataXML());

            Logger.Log("Using Kernel 1 Defaults:");
            KernelConfigurationDataForTransactionType kcdott = new KernelConfigurationDataForTransactionType()
            {
                TransactionTypeEnum = transactionTypeEnum,
                KernelConfigurationDataObjects = TLVListXML.XmlDeserialize(configProvider.GetKernel3ConfigurationDataXML(Formatting.ByteArrayToHexString(new byte[] { (byte)transactionTypeEnum })))
            };

            int depth = 0;
            Logger.Log("Transaction Type: " + transactionTypeEnum + " Using Kernel1 Defaults: \n" + kcdott.KernelConfigurationDataObjects.ToPrintString(ref depth));
            
            KernelConfigurationData.Add(kcdott);
        }
    }
}
