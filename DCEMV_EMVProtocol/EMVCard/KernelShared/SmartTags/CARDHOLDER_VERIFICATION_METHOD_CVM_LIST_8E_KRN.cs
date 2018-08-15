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
using DataFormatters;
using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DCEMV.TLVProtocol;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum CVMCode
    {
        FailCVMProcessing = 0x00,
        PlaintextPINVerificationPerformedByICC = 0x01,
        EncipheredPINVerifiedOnline = 0x02,
        PlaintextPINVerificationPerformedByICCAndSignature_Paper = 0x03,
        EncipheredPINVerificationPerformedByICC = 0x04,
        EncipheredPINVerificationPerformedByICCAndSignature_Paper = 0x05,
        Signature_Paper = 0x1E,
        NoCVMRequired = 0x1F,
        NoCVMDone = 0x3F,
        RFU = 0x1D,
    }
    public enum CVMConditionCode
    {
        Always = 0x00,
        IfUnattendedCash = 0x01,
        IfNotUnattendedCashAndNotManualCashAndNotPurchaseWithCashBack = 0x02,
        IfTerminalSupportstheCVM = 0x03,
        IfManualCash = 0x04,
        IfPurchaseWithCashBack = 0x05,
        IfTransactionIsInTheApplicationCurrencyAndIsUnderX = 0x06,
        IfTransactionIsInTheApplicationCurrencyAndIsOverX = 0x07,
        IfTransactionIsInTheApplicationCurrencyAndIsUnderY = 0x08,
        IfTransactionIsInTheApplicationCurrencyAndIsOverY = 0x09,
    }
    public class CardHolderVerificationRule
    {
        public byte CVMRule { get; set; }
        public CVMConditionCode CVMConditionCode { get; set; }

        public byte[] serialize()
        {
            return new byte[] { CVMRule, (byte)CVMConditionCode };
        }
        public int deserialize(byte[] rawTlv, int pos)
        {
            CVMRule = rawTlv[pos];
            CVMConditionCode = (CVMConditionCode)TLV.GetEnum(typeof(CVMConditionCode), rawTlv[pos + 1]);
            return pos + 2;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            byte cvm = (byte)(CVMRule & 0x3F);
            byte failureCondition = (byte)(CVMRule & 0x40);
            sb.Append("Condition Code: " + CVMConditionCode);
            sb.Append(" | CVM: " + (CVMCode)TLV.GetEnum(typeof(CVMCode), cvm));
            sb.Append(" | Failure Condition: " + (CVMFailureCondition)TLV.GetEnum(typeof(CVMFailureCondition), failureCondition));
            return sb.ToString();
        }

        public bool IsConditionCodeUnderstood()
        {
            if ((byte)CVMConditionCode >= 0x00 && (byte)CVMConditionCode <= 0x09)
                return true;
            else
                return false;
        }

        internal bool IsCVMRecognised()
        {
            byte toCheck = (byte)(CVMRule & 0x3F);
            switch (toCheck)
            {
                case (byte)CVMCode.FailCVMProcessing:
                case (byte)CVMCode.PlaintextPINVerificationPerformedByICC:
                case (byte)CVMCode.EncipheredPINVerifiedOnline:
                case (byte)CVMCode.PlaintextPINVerificationPerformedByICCAndSignature_Paper:
                case (byte)CVMCode.EncipheredPINVerificationPerformedByICC:
                case (byte)CVMCode.EncipheredPINVerificationPerformedByICCAndSignature_Paper:
                case (byte)CVMCode.Signature_Paper:
                case (byte)CVMCode.NoCVMRequired:
                case (byte)CVMCode.RFU:
                    return true;
                default:
                    return false;
            }
        }
        internal bool IsFailCardholderVerificationIfThisCVMIsUnsuccessful(KernelDatabaseBase database)
        {
            //if bit 7 of cvm rule is 0: Fail cardholder verification if this CVM is unsuccessful
            //if bit 7 of cvm rule is 1: Apply succeeding CV Rule if this CVM is unsuccessful

            if ((CVMRule & 0x40) == (byte)CVMFailureCondition.FailCardholderVerificationIfThisCVMIsUnsuccessful)
                return true;
            else
                return false;
        }
        internal byte GetCVMCode()
        {
            return (byte)(CVMRule & 0x3F);
        }
        internal bool IsSupported(KernelDatabaseBase database)
        {
            TERMINAL_CAPABILITIES_9F33_KRN tc = new TERMINAL_CAPABILITIES_9F33_KRN(database);
            byte cvmCode = (byte)(CVMRule & 0x3F);

            if ((byte)CVMCode.PlaintextPINVerificationPerformedByICC == cvmCode)
                return tc.Value.PlainTextPINforICCVerificationCapable;

            if ((byte)CVMCode.EncipheredPINVerifiedOnline == cvmCode)
                return tc.Value.EncipheredPINForOnlineVerificationCapable;

            if ((byte)CVMCode.PlaintextPINVerificationPerformedByICCAndSignature_Paper == cvmCode)
                return tc.Value.PlainTextPINforICCVerificationCapable && tc.Value.SignaturePaperCapable;

            if ((byte)CVMCode.EncipheredPINVerificationPerformedByICC == cvmCode)
                return tc.Value.EncipheredPINForOfflineVerificationCapable;

            if ((byte)CVMCode.EncipheredPINVerificationPerformedByICCAndSignature_Paper == cvmCode)
                return tc.Value.EncipheredPINForOfflineVerificationCapable && tc.Value.SignaturePaperCapable;

            if ((byte)CVMCode.Signature_Paper == cvmCode)
                return tc.Value.SignaturePaperCapable;

            if ((byte)CVMCode.NoCVMRequired == cvmCode)
                return tc.Value.NoCVMRequiredCapable;

            if ((byte)CVMCode.RFU == cvmCode)
                return tc.Value.SignaturePaperCapable;

            return false;
        }
        internal bool AllDataRequiredIsAvailable(KernelDatabaseBase database)
        {
            switch (CVMConditionCode)
            {
                case CVMConditionCode.Always:
                    return true;

                case CVMConditionCode.IfUnattendedCash:
                    if (database.IsEmpty(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN.Tag))
                        return false;
                    else
                        return true;

                case CVMConditionCode.IfManualCash:
                    if (database.IsEmpty(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag))
                        return false;
                    return true;

                case CVMConditionCode.IfNotUnattendedCashAndNotManualCashAndNotPurchaseWithCashBack:
                    if (database.IsEmpty(EMVTagsEnum.TERMINAL_TYPE_9F35_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN.Tag))
                        return false;
                    return true;

                case CVMConditionCode.IfTerminalSupportstheCVM:
                    return true;

                case CVMConditionCode.IfPurchaseWithCashBack:
                    if (database.IsEmpty(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN.Tag))
                        return false;
                    return true;

                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsUnderX:
                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsOverX:
                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsUnderY:
                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsOverY:
                    if (database.IsEmpty(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN.Tag))
                        return false;
                    if (database.IsEmpty(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag))
                        return false;

                    return true;

                default:
                    throw new EMVProtocolException("invalid CVM ConditionCode in Rule");
            }
        }

        internal bool ConditionsSatisfied(KernelDatabaseBase database, long amountX, long amountY)
        {
            byte tt = database.Get(EMVTagsEnum.TRANSACTION_TYPE_9C_KRN).Value[0];
            TERMINAL_TYPE_9F35_KRN.TerminalType terminalType = new TERMINAL_TYPE_9F35_KRN(database).Value.TerminalType;
            bool isManual = false;
            byte posEntryMode = database.Get(EMVTagsEnum.POINTOFSERVICE_POS_ENTRY_MODE_9F39_KRN).Value[0];
            if (posEntryMode == 0x01)
                isManual = true;

            long aa = Formatting.BcdToLong(database.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN).Value);

            bool tccEqualsacc = false;
            if (database.IsNotEmpty(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN.Tag) && database.IsNotEmpty(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN.Tag))
            {
                string tcc = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.TRANSACTION_CURRENCY_CODE_5F2A_KRN).Value);
                string acc = Formatting.ByteArrayToHexString(database.Get(EMVTagsEnum.APPLICATION_CURRENCY_CODE_9F42_KRN).Value);
                if (tcc == acc)
                    tccEqualsacc = true;
            }

            switch (CVMConditionCode)
            {
                case CVMConditionCode.Always:
                    return true;

                case CVMConditionCode.IfUnattendedCash:
                    if (terminalType.AttendedUnattended == TERMINAL_TYPE_9F35_KRN.AttendedUnattended.Unattended &&
                        tt == (byte)TransactionTypeEnum.CashWithdrawal)
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfManualCash:

                    if (isManual && tt == (byte)TransactionTypeEnum.CashWithdrawal)
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfNotUnattendedCashAndNotManualCashAndNotPurchaseWithCashBack:
                    if (!(terminalType.AttendedUnattended == TERMINAL_TYPE_9F35_KRN.AttendedUnattended.Unattended &&
                        tt == (byte)TransactionTypeEnum.CashWithdrawal) &&
                        (!(isManual && tt == (byte)TransactionTypeEnum.CashWithdrawal)) &&
                        (!(tt == (byte)TransactionTypeEnum.PurchaseWithCashback))
                        )
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfTerminalSupportstheCVM:
                    return IsSupported(database);

                case CVMConditionCode.IfPurchaseWithCashBack:
                    if ((tt == (byte)TransactionTypeEnum.PurchaseWithCashback))
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsUnderX:

                    if (tccEqualsacc && aa <= amountX)
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsOverX:
                    if (tccEqualsacc && aa > amountX)
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsUnderY:
                    if (tccEqualsacc && aa <= amountY)
                        return true;
                    else
                        return false;

                case CVMConditionCode.IfTransactionIsInTheApplicationCurrencyAndIsOverY:
                    if (tccEqualsacc && aa > amountY)
                        return true;
                    else
                        return false;

                default:
                    throw new EMVProtocolException("invalid CVM ConditionCode in Rule");
            }
        }
    }

    public enum CVMFailureCondition
    {
        FailCardholderVerificationIfThisCVMIsUnsuccessful = 0x00,
        ApplySucceedingCVRuleIfThisCVMIsUnsuccessful = 0x40,
    }

    public class CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN : SmartTag
    {
        //"Fail cardholder verification if this CVM is unsuccessful"
        //"Apply succeeding CV Rule if this CVM is unsuccessful"
        //"Fail CVM processing Plaintext PIN verification performed by ICC"
        //"Enciphered PIN verified online"
        //"Plaintext PIN verification performed by ICC and signature(paper)"
        //"Enciphered PIN verification performed by ICC"
        //"Enciphered PIN verification performed by ICC and signature(paper)"
        //"Signature(paper)"
        //"No CVM required"
        
        public class CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN_VALUE : SmartValue
        {
            public CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN_VALUE(DataFormatterBase dataFormatter)
                :base(dataFormatter)
            {
                CardHolderVerificationRules = new List<CardHolderVerificationRule>();
            }
            public long AmountX { get; set; }
            public long AmountY { get; set; }
            public List<CardHolderVerificationRule> CardHolderVerificationRules { get; set; }

            public override byte[] Serialize()
            {
                List<byte[]> result = new List<byte[]>();
                result.Add(BitConverter.GetBytes(AmountX));
                result.Add(BitConverter.GetBytes(AmountY));

                foreach(CardHolderVerificationRule cvr in CardHolderVerificationRules)
                {
                    result.Add(cvr.serialize());
                }

                Value = result.SelectMany(a => a).ToArray();

                return base.Serialize();
            }

            public override string ToString()
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("AmountX: " + AmountX + " | Amount Y: " + AmountY + " ");
                CardHolderVerificationRules.ForEach(x => sb.AppendLine(x.ToString()));
                return sb.ToString();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                byte[] amountXBytes = new byte[4];
                byte[] amountYBytes = new byte[4];
                Array.Copy(Value, 0, amountXBytes, 0, 4);
                Array.Copy(Value, 4, amountYBytes, 0, 4);

                AmountX = Formatting.BcdToLong(amountXBytes);
                AmountY = Formatting.BcdToLong(amountYBytes);

                int noEntriesLength = Value.Length;
                for(int i = 8; i< noEntriesLength;)//dont increment
                {
                    CardHolderVerificationRule r = new CardHolderVerificationRule();
                    i = r.deserialize(Value, i);
                    CardHolderVerificationRules.Add(r);
                }

                return pos;
            }
        }
        public new CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN_VALUE Value { get { return (CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN_VALUE)Val; } }
        public CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN(KernelDatabaseBase database)
             : base(database, EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN, 
                   new CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN_VALUE(EMVTagsEnum.CARDHOLDER_VERIFICATION_METHOD_CVM_LIST_8E_KRN.DataFormatter))
        {
        }

        public override string ToString()
        {
            int depth = 0;
            return ToPrintString(ref depth);
        }

        public override string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();
            string tagName = TLVMetaDataSourceSingleton.Instance.DataSource.GetName(Tag.TagLable);

            string formatter = "{0,-75}";

            sb.Append(string.Format(formatter, Tag.ToString() + " " + tagName + " " + Val.GetLength()));

            sb.AppendLine(" V:[");
            sb.Append(Value.ToString());
            sb.Append("]");
            
            return sb.ToString();
        }
    }
}
