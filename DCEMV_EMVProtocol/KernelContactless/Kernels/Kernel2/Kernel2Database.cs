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

namespace DCEMV.EMVProtocol.Kernels.K2
{
    public class Kernel2Database : KernelDatabaseBase
    {
        public new static Logger Logger = new Logger(typeof(Kernel2Database));

        public TLVList TagsToWriteBeforeGenACYet { get; }
        public TLVList TagsToWriteAfterGenACYet { get; }
        
        //Indicates if CDA is to be performed for the transaction in progress.
        public byte ODAStatus { get; set; } 
        public long ReaderContactlessTransactionLismit { get; set; }
        public int FailedMSCntr { get; set; }
        public string ActiveTag { get; set; }
        public TORN_RECORD_FF8101_KRN2 TornTempRecord { get; set; }
        public int NUN { get; set; }

        //ac type wanted by terminal
        public DS_AC_TYPE_DF8108_KRN2 ACType { get; set; }

        public Kernel2Database(PublicKeyCertificateManager publicKeyCertificateManager)
            :base(publicKeyCertificateManager)
        {
            TagsToWriteBeforeGenACYet = new TLVList();
            TagsToWriteAfterGenACYet = new TLVList();
        }

        public override void UpdateWithDETData(TLVList terminalSentData)
        {
            base.UpdateWithDETData(terminalSentData);

            TLV tagsToRead = terminalSentData.Get(EMVTagsEnum.TAGS_TO_READ_DF8112_KRN2.Tag);
            if (tagsToRead != null)
            {
                TagsToReadYet.AddListToList(TLV.DeserializeChildrenWithNoLV(tagsToRead.Value, 0));
            }
            TLV tagsToWriteBeforeGenAC = terminalSentData.Get(EMVTagsEnum.TAGS_TO_WRITE_BEFORE_GEN_AC_FF8102_KRN2.Tag);
            if (tagsToWriteBeforeGenAC != null)
            {
                TagsToWriteBeforeGenACYet.AddListToList(tagsToWriteBeforeGenAC.Children);
            }
            TLV tagsToWriteAfterGenAC = terminalSentData.Get(EMVTagsEnum.TAGS_TO_WRITE_AFTER_GEN_AC_FF8103_KRN2.Tag);
            if (tagsToWriteAfterGenAC != null)
            {
                TagsToWriteAfterGenACYet.AddListToList(tagsToWriteAfterGenAC.Children);
            }
        }

        protected override void LoadKernelDefaultConfigurationDataObjects(TransactionTypeEnum transactionTypeEnum, IConfigurationProvider configProvider)
        {
            Logger.Log("Using Kernel 2 Defaults:");
            KernelConfigurationDataForTransactionType kcdott = new KernelConfigurationDataForTransactionType()
            {
                TransactionTypeEnum = transactionTypeEnum,
                KernelConfigurationDataObjects = TLVListXML.XmlDeserialize(configProvider.GetKernel2ConfigurationDataXML(Formatting.ByteArrayToHexString(new byte[] { (byte)transactionTypeEnum })))
            };

            int depth = 0;
            Logger.Log("Transaction Type: " + transactionTypeEnum + " Using Kernel2 Defaults: \n" + kcdott.KernelConfigurationDataObjects.ToPrintString(ref depth));
            
            TLV _9f1d = kcdott.KernelConfigurationDataObjects.Get(EMVTagsEnum.TERMINAL_RISK_MANAGEMENT_DATA_9F1D_KRN.Tag);

            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(kcdott.KernelConfigurationDataObjects.Get(EMVTagsEnum.TERMINAL_CAPABILITIES_9F33_KRN.Tag));
            if (tc.Value.EncipheredPINForOnlineVerificationCapable)
            {
                Formatting.SetBitPosition(ref _9f1d.Value[0], true, 7);
            }
            KERNEL_CONFIGURATION_DF811B_KRN2 kc = new KERNEL_CONFIGURATION_DF811B_KRN2(kcdott.KernelConfigurationDataObjects.Get(EMVTagsEnum.KERNEL_CONFIGURATION_DF811B_KRN2.Tag));
            if (kc.Value.OnDeviceCardholderVerificationSupported)
            {
                Formatting.SetBitPosition(ref _9f1d.Value[0], true, 3);
            }

            KernelConfigurationData.Add(kcdott);
        }
        
        public bool IDSSupported()
        {
            if (IsPresent(EMVTagsEnum.DS_REQUESTED_OPERATOR_ID_9F5C_KRN2.Tag) && IsNotEmpty(EMVTagsEnum.DS_REQUESTED_OPERATOR_ID_9F5C_KRN2.Tag))
                return true;
            else
                return false;
        }
    }
}
