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
using System.Text;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum RelayResistancePerformedEnum
    {
        RELAY_RESISTANCE_PROTOCOL_NOT_SUPPORTED = 0x00,
        RRP_NOT_PERFORMED = 0x01,
        RRP_PERFORMED = 0x10,
    }
    public class TERMINAL_VERIFICATION_RESULTS_95_KRN : SmartTag
    {
        public override string ToPrintString(ref int depth)
        {
            StringBuilder sb = new StringBuilder();

            int depthBase = depth;
            sb.AppendLine(base.ToPrintString(ref depth));
            sb.AppendLine(string.Format("{0}:{1}", "OfflineDataAuthenticationWasNotPerformed", Value.OfflineDataAuthenticationWasNotPerformed));
            sb.AppendLine(string.Format("{0}:{1}", "SDAFailed", Value.SDAFailed));
            sb.AppendLine(string.Format("{0}:{1}", "ICCDataMissing", Value.ICCDataMissing));
            sb.AppendLine(string.Format("{0}:{1}", "CardAppearsOnTerminalExceptionFile", Value.CardAppearsOnTerminalExceptionFile));
            sb.AppendLine(string.Format("{0}:{1}", "DDAFailed", Value.CardAppearsOnTerminalExceptionFile));
            sb.AppendLine(string.Format("{0}:{1}", "CDAFailed", Value.CDAFailed));
            sb.AppendLine(string.Format("{0}:{1}", "ICCAndTerminalHaveDifferentApplicationVersions", Value.ICCAndTerminalHaveDifferentApplicationVersions));
            sb.AppendLine(string.Format("{0}:{1}", "ExpiredApplication", Value.ExpiredApplication));
            sb.AppendLine(string.Format("{0}:{1}", "ApplicationNotYetEffective", Value.ApplicationNotYetEffective));
            sb.AppendLine(string.Format("{0}:{1}", "RequestedServiceNotAllowedForCardProduct", Value.ApplicationNotYetEffective));
            sb.AppendLine(string.Format("{0}:{1}", "NewCard", Value.NewCard));
            sb.AppendLine(string.Format("{0}:{1}", "CardholderVerificationWasNotSuccessful", Value.CardholderVerificationWasNotSuccessful));
            sb.AppendLine(string.Format("{0}:{1}", "UnrecognisedCVM", Value.UnrecognisedCVM));
            sb.AppendLine(string.Format("{0}:{1}", "PINTryLimitExceeded", Value.PINTryLimitExceeded));
            sb.AppendLine(string.Format("{0}:{1}", "PINEntryRequiredAndPINPadNotPresentOrNotWorking", Value.PINEntryRequiredAndPINPadNotPresentOrNotWorking));
            sb.AppendLine(string.Format("{0}:{1}", "PINEntryRequiredPINPadPresentButPINWasNotEntered", Value.PINEntryRequiredPINPadPresentButPINWasNotEntered));
            sb.AppendLine(string.Format("{0}:{1}", "OnlinePINEntered", Value.OnlinePINEntered));
            sb.AppendLine(string.Format("{0}:{1}", "TransactionExceedsFloorLimit", Value.TransactionExceedsFloorLimit));
            sb.AppendLine(string.Format("{0}:{1}", "LowerConsecutiveOfflineLimitExceeded", Value.LowerConsecutiveOfflineLimitExceeded));
            sb.AppendLine(string.Format("{0}:{1}", "UpperConsecutiveOfflineLimitExceeded", Value.UpperConsecutiveOfflineLimitExceeded));
            sb.AppendLine(string.Format("{0}:{1}", "TransactionSelectedRandomlyForOnlineProcessing", Value.TransactionSelectedRandomlyForOnlineProcessing));
            sb.AppendLine(string.Format("{0}:{1}", "MerchantForcedTransactionOnline", Value.MerchantForcedTransactionOnline));
            sb.AppendLine(string.Format("{0}:{1}", "DefaultTDOLUsed", Value.DefaultTDOLUsed));
            sb.AppendLine(string.Format("{0}:{1}", "IssuerAuthenticationFailed", Value.IssuerAuthenticationFailed));
            sb.AppendLine(string.Format("{0}:{1}", "ScriptProcessingFailedBeforeFinalGENERATEAC", Value.ScriptProcessingFailedBeforeFinalGENERATEAC));
            sb.AppendLine(string.Format("{0}:{1}", "ScriptProcessingFailedAfterFinalGENERATEAC", Value.ScriptProcessingFailedAfterFinalGENERATEAC));
            sb.AppendLine(string.Format("{0}:{1}", "RelayResistanceThresholdExceeded", Value.RelayResistanceThresholdExceeded));
            sb.AppendLine(string.Format("{0}:{1}", "RelayResistanceTimeLimitsExceeded", Value.RelayResistanceTimeLimitsExceeded));
            return sb.ToString();
        }

        public class TERMINAL_VERIFICATION_RESULTS_95_KRN_VALUE : SmartValue
        {
            public TERMINAL_VERIFICATION_RESULTS_95_KRN_VALUE(DataFormatterBase dataFormatter)
                : base(dataFormatter)
            {

            }

            public bool OfflineDataAuthenticationWasNotPerformed { get; set; }
            public bool SDAFailed { get; set; }
            public bool ICCDataMissing { get; set; }
            public bool CardAppearsOnTerminalExceptionFile { get; protected set; }
            public bool DDAFailed { get; set; }
            public bool CDAFailed { get; set; }

            public bool ICCAndTerminalHaveDifferentApplicationVersions { get; set; }
            public bool ExpiredApplication { get; set; }
            public bool ApplicationNotYetEffective { get; set; }
            public bool RequestedServiceNotAllowedForCardProduct { get; set; }
            public bool NewCard { get; set; }

            public bool CardholderVerificationWasNotSuccessful { get; set; }
            public bool UnrecognisedCVM { get; set; }
            public bool PINTryLimitExceeded { get; set; }
            public bool PINEntryRequiredAndPINPadNotPresentOrNotWorking { get; set; }
            public bool PINEntryRequiredPINPadPresentButPINWasNotEntered { get; set; }
            public bool OnlinePINEntered { get; set; }

            public bool TransactionExceedsFloorLimit { get; set; }
            public bool LowerConsecutiveOfflineLimitExceeded { get; set; }
            public bool UpperConsecutiveOfflineLimitExceeded { get; set; }
            public bool TransactionSelectedRandomlyForOnlineProcessing { get; set; }
            public bool MerchantForcedTransactionOnline { get; set; }

            public bool DefaultTDOLUsed { get; protected set; }
            public bool IssuerAuthenticationFailed { get; set; }
            public bool ScriptProcessingFailedBeforeFinalGENERATEAC { get; set; }
            public bool ScriptProcessingFailedAfterFinalGENERATEAC { get; set; }
            public bool RelayResistanceThresholdExceeded { get; set; }
            public bool RelayResistanceTimeLimitsExceeded { get; set; }

            public RelayResistancePerformedEnum RelayResistancePerformedEnum { get; set; }

            public override byte[] Serialize()
            {
                Formatting.SetBitPosition(ref Value[0], OfflineDataAuthenticationWasNotPerformed, 8);
                Formatting.SetBitPosition(ref Value[0], SDAFailed, 7);
                Formatting.SetBitPosition(ref Value[0], ICCDataMissing, 6);
                Formatting.SetBitPosition(ref Value[0], CardAppearsOnTerminalExceptionFile, 5);
                Formatting.SetBitPosition(ref Value[0], DDAFailed, 4);
                Formatting.SetBitPosition(ref Value[0], CDAFailed, 3);

                Formatting.SetBitPosition(ref Value[1], ICCAndTerminalHaveDifferentApplicationVersions, 8);
                Formatting.SetBitPosition(ref Value[1], ExpiredApplication, 7);
                Formatting.SetBitPosition(ref Value[1], ApplicationNotYetEffective, 6);
                Formatting.SetBitPosition(ref Value[1], RequestedServiceNotAllowedForCardProduct, 5);
                Formatting.SetBitPosition(ref Value[1], NewCard, 4);

                Formatting.SetBitPosition(ref Value[2], CardholderVerificationWasNotSuccessful, 8);
                Formatting.SetBitPosition(ref Value[2], UnrecognisedCVM, 7);
                Formatting.SetBitPosition(ref Value[2], PINTryLimitExceeded, 6);
                Formatting.SetBitPosition(ref Value[2], PINEntryRequiredAndPINPadNotPresentOrNotWorking, 5);
                Formatting.SetBitPosition(ref Value[2], PINEntryRequiredPINPadPresentButPINWasNotEntered, 4);
                Formatting.SetBitPosition(ref Value[2], OnlinePINEntered, 3);

                Formatting.SetBitPosition(ref Value[3], TransactionExceedsFloorLimit, 8);
                Formatting.SetBitPosition(ref Value[3], LowerConsecutiveOfflineLimitExceeded, 7);
                Formatting.SetBitPosition(ref Value[3], UpperConsecutiveOfflineLimitExceeded, 6);
                Formatting.SetBitPosition(ref Value[3], TransactionSelectedRandomlyForOnlineProcessing, 5);
                Formatting.SetBitPosition(ref Value[3], MerchantForcedTransactionOnline, 4);

                Formatting.SetBitPosition(ref Value[4], DefaultTDOLUsed, 8);
                Formatting.SetBitPosition(ref Value[4], IssuerAuthenticationFailed, 7);
                Formatting.SetBitPosition(ref Value[4], ScriptProcessingFailedBeforeFinalGENERATEAC, 6);
                Formatting.SetBitPosition(ref Value[4], ScriptProcessingFailedAfterFinalGENERATEAC, 5);
                Formatting.SetBitPosition(ref Value[4], RelayResistanceThresholdExceeded, 4);
                Formatting.SetBitPosition(ref Value[4], RelayResistanceTimeLimitsExceeded, 3);


                Value[4] = (byte)(Value[4] & 0xFC);//clear last 2 bits
                Value[4] = (byte)(Value[4] | (byte)RelayResistancePerformedEnum);//set last 2 bits

                return base.Serialize();
            }

            public override int Deserialize(byte[] rawTlv, int pos)
            {
                pos = base.Deserialize(rawTlv, pos);

                OfflineDataAuthenticationWasNotPerformed = Formatting.GetBitPosition(Value[0], 8);
                SDAFailed = Formatting.GetBitPosition(Value[0], 7);
                ICCDataMissing = Formatting.GetBitPosition(Value[0], 6);
                CardAppearsOnTerminalExceptionFile = Formatting.GetBitPosition(Value[0], 5);
                DDAFailed = Formatting.GetBitPosition(Value[0], 4);
                CDAFailed = Formatting.GetBitPosition(Value[0], 3);

                ICCAndTerminalHaveDifferentApplicationVersions = Formatting.GetBitPosition(Value[1], 8);
                ExpiredApplication = Formatting.GetBitPosition(Value[1], 7);
                ApplicationNotYetEffective = Formatting.GetBitPosition(Value[1], 6);
                RequestedServiceNotAllowedForCardProduct = Formatting.GetBitPosition(Value[1], 5);
                NewCard = Formatting.GetBitPosition(Value[1], 4);

                CardholderVerificationWasNotSuccessful = Formatting.GetBitPosition(Value[2], 8);
                UnrecognisedCVM = Formatting.GetBitPosition(Value[2], 7);
                PINTryLimitExceeded = Formatting.GetBitPosition(Value[2], 6);
                PINEntryRequiredAndPINPadNotPresentOrNotWorking = Formatting.GetBitPosition(Value[2], 5);
                PINEntryRequiredPINPadPresentButPINWasNotEntered = Formatting.GetBitPosition(Value[2], 4);
                OnlinePINEntered = Formatting.GetBitPosition(Value[2], 3);

                TransactionExceedsFloorLimit = Formatting.GetBitPosition(Value[3], 8);
                LowerConsecutiveOfflineLimitExceeded = Formatting.GetBitPosition(Value[3], 7);
                UpperConsecutiveOfflineLimitExceeded = Formatting.GetBitPosition(Value[3], 6);
                TransactionSelectedRandomlyForOnlineProcessing = Formatting.GetBitPosition(Value[3], 5);
                MerchantForcedTransactionOnline = Formatting.GetBitPosition(Value[3], 4);

                DefaultTDOLUsed = Formatting.GetBitPosition(Value[4], 8);
                IssuerAuthenticationFailed = Formatting.GetBitPosition(Value[4], 7);
                ScriptProcessingFailedBeforeFinalGENERATEAC = Formatting.GetBitPosition(Value[4], 6);
                ScriptProcessingFailedAfterFinalGENERATEAC = Formatting.GetBitPosition(Value[4], 5);
                RelayResistanceThresholdExceeded = Formatting.GetBitPosition(Value[4], 4);
                RelayResistanceTimeLimitsExceeded = Formatting.GetBitPosition(Value[4], 3);

                RelayResistancePerformedEnum = (RelayResistancePerformedEnum)GetEnum(typeof(RelayResistancePerformedEnum), Value[4] & 0x03);

                return pos;
            }
        }

        public new TERMINAL_VERIFICATION_RESULTS_95_KRN_VALUE Value { get { return (TERMINAL_VERIFICATION_RESULTS_95_KRN_VALUE)Val; } }
        public TERMINAL_VERIFICATION_RESULTS_95_KRN(KernelDatabaseBase database)
            : base(database, EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN, 
                  new TERMINAL_VERIFICATION_RESULTS_95_KRN_VALUE(EMVTagsEnum.TERMINAL_VERIFICATION_RESULTS_95_KRN.DataFormatter))
        {
        }

       
    }
}
