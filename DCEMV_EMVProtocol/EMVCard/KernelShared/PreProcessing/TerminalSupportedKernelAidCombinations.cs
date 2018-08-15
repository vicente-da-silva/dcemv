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
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum AIDEnum
    {
        A000000050010101,           //DC EMV

        A0000000046000,             //Cirrus

        A0000000031010,             //Visa International VISA Debit/Credit (Classic)
        A000000003101001,           //Visa Credit
        A000000003101002,           //Visa Debit
        A000000003101003,
        A000000003101004,
        A000000003101005,
        A000000003101008,
        A000000003101009,

        A0000000032010,             //Visa International VISA Electron (Debit)
        A0000000032020,             //Visa International VISA   V PAY
        A0000000033010,             //Visa International VISA Interlink
        A0000000034010,             //Visa International VISA Specific
        A0000000035010,             //Visa International VISA Specific

        A0000000041010,             //Mastercard International MasterCard Credit/Debit (Global)
        A000000004101001,           
        A000000004101002,           
        A0000000042010,             //Mastercard International MasterCard Specific
        A0000000043010,             //Mastercard International MasterCard Specific
        A0000000043060,             //Mastercard International Maestro (Debit)
        A0000000044010,             //Mastercard International MasterCard Specific
        A0000000045010,             //Mastercard International MasterCard Specific
      
        None,
    }
    public enum RIDEnum
    {
        A000000004, //MC
        A000000050, //OWN
        A000000003, //VISA
        A000000025, //American Express
        A000000065, //JCB
        A000000324, //Discovery
        A000000333, //China Union Pay
        None,
    }

    //Kernel 1 for some cards with JCB AIDs and some cards with Visa AIDs
    //Kernel 2 for MasterCard AIDs
    //Kernel 3 for Visa AIDs
    //Kernel 4 for American Express AIDs
    //Kernel 5 for JCB AIDs
    //Kernel 6 for Discover AIDs
    //Kernel 7 for UnionPay AIDs
    public enum KernelEnum
    {
        Kernel = 0x00,
        Kernel1 = 0x01,
        Kernel2 = 0x02,
        Kernel3 = 0x03,
        Kernel4 = 0x04,
        Kernel5 = 0x05,
        Kernel6 = 0x06,
        Kernel7 = 0x07,
    }
    public enum TransactionTypeEnum
    {
        PurchaseGoodsAndServices = 0x00,
        CashWithdrawal = 0x01,
        CashDisbursement = 0x17,
        Adjustment = 0x02,
        PurchaseWithCashback = 0x09,
        BalanceInquiry = 0x31,
        Refund = 0x20
    }
    public class TerminalSupportedKernelAidTransactionTypeCombination
    {

    }

    public class TerminalSupportedContactKernelAidTransactionTypeCombination : TerminalSupportedKernelAidTransactionTypeCombination
    {
        public AIDEnum AIDEnum { get; set; }
        public bool ApplicationSelectionIndicator { get; set; } 
    }
    public class TerminalSupportedContactlessKernelAidTransactionTypeCombination : TerminalSupportedKernelAidTransactionTypeCombination
    {
        public KernelEnum KernelEnum { get; set; }
        public RIDEnum RIDEnum { get; set; }
        public TransactionTypeEnum TransactionTypeEnum { get; set; }

        public bool? StatusCheckSupportFlag { get; set; }
        public bool? ZeroAmountAllowedFlag { get; set; }
        public long? ReaderContactlessTransactionLimit { get; set; }
        public long? ReaderContactlessFloorLimit { get; set; }
        public long? TerminalFloorLimit_9F1B { get; set; }
        public long? ReaderCVMRequiredLimit { get; set; }

        [XmlIgnore]
        public TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN TTQ { get; set; }

        [XmlElement(DataType = "hexBinary")]
        public byte[] TTQAsBytes;

        public bool? ExtendedSelectionSupportFlag { get; set; }
    }

    public static class TerminalSupportedKernelAidTransactionTypeCombinations
    {
        public static Logger Logger = new Logger(typeof(TerminalSupportedKernelAidTransactionTypeCombinations));

        public static List<TerminalSupportedContactlessKernelAidTransactionTypeCombination> SupportedContactlessCombinations = new List<TerminalSupportedContactlessKernelAidTransactionTypeCombination>();

        public static List<TerminalSupportedContactKernelAidTransactionTypeCombination> SupportedContactCombinations = new List<TerminalSupportedContactKernelAidTransactionTypeCombination>();

        public static void LoadContactSupportedCombination(IConfigurationProvider configProvider, TransactionTypeEnum tt)
        {
            Logger.Log("Contact Terminal Supported AIDs:");
            SupportedContactCombinations = XMLUtil<List<TerminalSupportedContactKernelAidTransactionTypeCombination>>.Deserialize(configProvider.GetContactTerminalSupportedAIDsXML());
        }

        public static void LoadContactlessSupportedCombination(IConfigurationProvider configProvider, TransactionTypeEnum tt)
        {
            Logger.Log("Contactless Terminal Supported AIDs:");
            SupportedContactlessCombinations = XMLUtil<List<TerminalSupportedContactlessKernelAidTransactionTypeCombination>>.Deserialize(configProvider.GetContactlessTerminalSupportedRIDsXML());

            SupportedContactlessCombinations = SupportedContactlessCombinations.Where(x => x.TransactionTypeEnum == tt).ToList();

            SupportedContactlessCombinations
                .ForEach((x) =>
                {
                    if (x.TTQAsBytes != null)
                    {
                        x.TTQ = new TERMINAL_TRANSACTION_QUALIFIERS_9F66_KRN();
                        int pos = 0;
                        x.TTQ.Value.Deserialize(TLV.Create(x.TTQAsBytes, ref pos).Val.Serialize(), 0);
                    }
                });
        }
    }
}
