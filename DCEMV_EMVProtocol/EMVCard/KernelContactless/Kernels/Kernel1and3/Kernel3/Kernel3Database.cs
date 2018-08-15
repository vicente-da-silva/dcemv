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

namespace DCEMV.EMVProtocol.Kernels.K3
{
    public class Kernel3Configuration
    {
        #region 3.3.3.1
        public bool IDSSupported { get; set; }
        #endregion
        
        #region 3.3.4.1 and 3.3.4.3
        public bool FDDAForOnlineSupported { get; set; }
        #endregion
        
        #region 3.3.4.2 and 3.3.4.3
        public bool SDAForOnlineSupported { get; set; }
        #endregion
        
        #region 4.3.1.1
        public bool DisplayAvailableSpendingAmount { get; set; }
        #endregion

        #region 5.1.1
        public bool AUCManualCheckSupported { get; set; }
        public bool AUCCashbackCheckSupported { get; set; }
        public bool ATMOfflineCheck { get; set; }
        public bool ExceptionFileEnabled { get; set; }
        #endregion


        public Kernel3Configuration()
        {
            IDSSupported = false;
            FDDAForOnlineSupported = true;
            SDAForOnlineSupported = true;
            DisplayAvailableSpendingAmount = true;
            AUCManualCheckSupported = true;
            AUCCashbackCheckSupported = true;
            ATMOfflineCheck = true;
            ExceptionFileEnabled = true;
        }
    }

    public class Kernel3Database : KernelDatabaseBase
    {
        public EntryPointPreProcessingIndicators ProcessingIndicatorsForSelected { get; set; }

        public Kernel3Configuration Kernel3Configuration { get; set; }

        public bool DeclineRequiredByReaderIndicator;
        public bool OnlineRequiredByReaderIndicator;
        
        public Kernel3Database(EntryPointPreProcessingIndicators processingIndicatorsForSelected, PublicKeyCertificateManager publicKeyCertificateManager)
            : base(publicKeyCertificateManager)
        {
            ProcessingIndicatorsForSelected = processingIndicatorsForSelected;
        }

        protected override void LoadKernelDefaultConfigurationDataObjects(TransactionTypeEnum transactionTypeEnum, IConfigurationProvider configProvider)
        {
            Logger.Log("Using Global Kernel 3 Values:");
            Kernel3Configuration = XMLUtil<Kernel3Configuration>.Deserialize(configProvider.GetKernel3GlobalConfigurationDataXML());

            #region 3.3.4.2 and 3.3.4.3
            TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN ttq = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN(this);
            if (Kernel3Configuration.FDDAForOnlineSupported)
                ttq.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported = true;
            else
                ttq.Value.OfflineDataAuthenticationForOnlineAuthorizationsSupported = false;
            #endregion

            Logger.Log("Using Kernel 3 Defaults:");
            KernelConfigurationDataForTransactionType kcdott = new KernelConfigurationDataForTransactionType()
            {
                TransactionTypeEnum = transactionTypeEnum,
                KernelConfigurationDataObjects = TLVListXML.XmlDeserialize(configProvider.GetKernel3ConfigurationDataXML(Formatting.ByteArrayToHexString(new byte[] { (byte)transactionTypeEnum })))
            };

            int depth = 0;
            Logger.Log("Transaction Type: " + transactionTypeEnum +  " Using Kernel3 Defaults: \n" + kcdott.KernelConfigurationDataObjects.ToPrintString(ref depth));
            
            KernelConfigurationData.Add(kcdott);
        }
    }
}
