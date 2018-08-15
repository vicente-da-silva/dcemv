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

namespace DCEMV.EMVProtocol.Kernels.K
{
    public class KernelConfiguration
    {
        
    }

    public class KernelDatabase : KernelDatabaseBase
    {
        public new static Logger Logger = new Logger(typeof(KernelDatabase));

        public KernelConfiguration KernelConfiguration { get; set; }
        
        public string ActiveTag { get; set; }
        
        //ac type wanted by terminal
        public DS_AC_TYPE_DF8108_KRN2 ACType { get; set; }
        
        public bool IsScriptProcessingBeforeGenACInProgress { get; set; }
        //scripts list built from script template
        public TLVList ScriptsToRunBeforeGenAC { get; set; }
        public TLVList ScriptsToRunAfterGenAC { get; set; }
        
        public KernelDatabase(PublicKeyCertificateManager publicKeyCertificateManager)
            : base(publicKeyCertificateManager)
        {
            ScriptsToRunBeforeGenAC = new TLVList();
            ScriptsToRunAfterGenAC = new TLVList();
        }

        protected override void LoadKernelDefaultConfigurationDataObjects(TransactionTypeEnum transactionTypeEnum, IConfigurationProvider configProvider)
        {
            Logger.Log("Using Global Kernel Values:");
            KernelConfiguration = XMLUtil<KernelConfiguration>.Deserialize(configProvider.GetKernelGlobalConfigurationDataXML());

            Logger.Log("Using Kernel Defaults:");
            KernelConfigurationDataForTransactionType kcdott = new KernelConfigurationDataForTransactionType()
            {
                TransactionTypeEnum = transactionTypeEnum,
                KernelConfigurationDataObjects = TLVListXML.XmlDeserialize(configProvider.GetKernelConfigurationDataXML(Formatting.ByteArrayToHexString(new byte[] { (byte)transactionTypeEnum })))
            };
            
            int depth = 0;
            Logger.Log("Transaction Type: " + transactionTypeEnum + " Using Kernel Defaults: \n" + kcdott.KernelConfigurationDataObjects.ToPrintString(ref depth));
            
            KernelConfigurationData.Add(kcdott);
        }
    }
}
