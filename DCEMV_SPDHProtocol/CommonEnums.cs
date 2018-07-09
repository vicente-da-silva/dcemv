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
namespace DCEMV.SPDHProtocol
{
    public static class SPDHConstants
    {
        public const string TransmissionNumberNotChecked = "00";
        public const string DialOrLeasedLineTerminalOrNetwork = "9.";
    }

    public enum SPDHMessageType
    {
        AdministrativeTransaction = 'A',
        FinancialTransaction = 'F',
        PassThroughAdministrativeTransaction = 'L',
        PassThroughFinancialTransaction = 'M',
    }
    public enum SPDHMessageSubType
    {
        TimeOutReveralAdvice = 'A',
        TerminalOrControllerReversal = 'C',
        TerminalDecryptionReversal = 'D',
        EMVChipCardLog = 'E',
        ForcePost = 'F',
        Online = 'O',
        MACReversal = 'R',
        StoreAndForward = 'S',
        TimeoutReversal = 'T',
        CusomerCanceledReversal = 'U',
        EMVLogCancellation = 'V',
    }
    public static class SPDHTransactionCode
    {
        public static string NormalPurchase = "00";
        public static string PreauthorizationPurchase = "01";
        public static string PreauthorizationPurchaseCompletion = "02";
        public static string MailOrTelephoneOrder = "03";
        public static string MerchandiseReturn = "04";
        public static string CashAdvance = "05";
        public static string CardVerification = "06";
        public static string BalanceInquiry = "07";
        public static string PurchaseWithCashBack = "08";
        public static string CheckVerification = "09";
        public static string CheckGuarantee = "10";
        public static string PurchaseAdjustment = "11";
        public static string MerchandiseReturnAdjustment = "12";
        public static string CashAdvanceAdjustment = "13";
        public static string CashBackAdjustment = "14";
        public static string CardActivation = "15";
        public static string AdditionalCardActivation = "16";
        public static string Replenishment = "17";
        public static string FullRedemption = "18";
        public static string LogonRequest = "50";
        public static string LogoffRequest = "51";
        public static string CloseBatchRequest = "60";
        public static string CloseShiftRequest = "61";
        public static string CloseDayRequest = "62";
        public static string EmployeeSubtotalsRequest = "64";
        public static string BatchSubtotalsRequest = "65";
        public static string ShiftSubtotalsRequest = "66";
        public static string DaySubtotalsRequest = "67";
        public static string ReadMailRequest = "70";
        public static string MailDeliveredRequest = "71";
        public static string SendMailRequest = "75";
        public static string DownloadRequest = "90";
        public static string HandshakeRequest = "95";
        public static string KeyChangeRequest = "96";
    }
}
