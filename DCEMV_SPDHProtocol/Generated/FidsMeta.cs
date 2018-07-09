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
using System.Collections.Generic;
using System.Linq;
namespace DCEMV.SPDHProtocol
{

    internal class FidMetaList
    {
        protected static List<FIDMeta> metaList = new List<FIDMeta>();
        public static FIDMeta FindMeta(char id, char subId)
        {
            return metaList.Find(x => x.Id == id && x.SubId == subId);
        }
        public static FIDMeta CustomerBillingAddress = new FIDMeta('A', ' ', "Customer Billing Address", FormatEnum.Hex, 1, 20);
        public static FIDMeta Amount1 = new FIDMeta('B', ' ', "Amount 1", FormatEnum.Numeric, 1, 18);
        public static FIDMeta Amount2 = new FIDMeta('C', ' ', "Amount 2", FormatEnum.Numeric, 1, 18);
        public static FIDMeta ApplicationAccountType = new FIDMeta('D', ' ', "Application Account Type", FormatEnum.Numeric, 1, 1);
        public static FIDMeta ApplicationAccountNumber = new FIDMeta('E', ' ', "Application Account Number", FormatEnum.Numeric, 1, 19);
        public static FIDMeta ApprovalCode = new FIDMeta('F', ' ', "Approval Code", FormatEnum.Hex, 8, 8);
        public static FIDMeta AuthenticationCode = new FIDMeta('G', ' ', "Authentication Code", FormatEnum.Hex, 8, 8);
        public static FIDMeta AuthenticationKey = new FIDMeta('H', ' ', "Authentication Key", FormatEnum.Hex, 16, 48);
        public static FIDMeta DataEncryptionKey = new FIDMeta('I', ' ', "Data Encryption Key", FormatEnum.Hex, 16, 48);
        public static FIDMeta AvailableBalance = new FIDMeta('J', ' ', "Available Balance", FormatEnum.Numeric, 18, 18);
        public static FIDMeta BusinessDate = new FIDMeta('K', ' ', "Business Date", FormatEnum.Numeric, 6, 6);
        public static FIDMeta CheckType = new FIDMeta('L', ' ', "Check Type", FormatEnum.Numeric, 1, 1);
        public static FIDMeta CommunicationsKey = new FIDMeta('M', ' ', "Communications Key", FormatEnum.Hex, 16, 48);
        public static FIDMeta CustomerID = new FIDMeta('N', ' ', "Customer ID", FormatEnum.Hex, 1, 40);
        public static FIDMeta CustomerIDType = new FIDMeta('O', ' ', "Customer ID Type", FormatEnum.Numeric, 2, 2);
        public static FIDMeta DraftCaptureFlag = new FIDMeta('P', ' ', "Draft Capture Flag", FormatEnum.Numeric, 1, 1);
        public static FIDMeta EchoData = new FIDMeta('Q', ' ', "Echo Data", FormatEnum.Hex, 1, 16);
        public static FIDMeta CardType = new FIDMeta('R', ' ', "Card Type", FormatEnum.Hex, 1, 1);
        public static FIDMeta InvoiceNumber = new FIDMeta('S', ' ', "Invoice Number", FormatEnum.Hex, 1, 10);
        public static FIDMeta InvoiceNumberOriginal = new FIDMeta('T', ' ', "Invoice Number/Original", FormatEnum.Hex, 1, 10);
        public static FIDMeta LanguageCode = new FIDMeta('U', ' ', "Language Code", FormatEnum.Hex, 1, 1);
        public static FIDMeta MailDownloadKey = new FIDMeta('V', ' ', "Mail/Download Key", FormatEnum.Hex, 15, 15);
        public static FIDMeta MailTextDownloadData = new FIDMeta('W', ' ', "Mail Text/Download Data", FormatEnum.Hex, 1, 957);
        public static FIDMeta ISOResponseCode = new FIDMeta('X', ' ', "ISO Response Code", FormatEnum.Numeric, 3, 3);
        public static FIDMeta CustomerZIPCode = new FIDMeta('Y', ' ', "Customer ZIP Code", FormatEnum.Hex, 1, 9);
        public static FIDMeta AddressVerificationStatusCode = new FIDMeta('Z', ' ', "Address Verification Status Code", FormatEnum.Hex, 1, 1);
        public static FIDMeta OptionalData = new FIDMeta('a', ' ', "Optional Data", FormatEnum.Hex, 1, 250);
        public static FIDMeta PINCustomer = new FIDMeta('b', ' ', "PIN/Customer", FormatEnum.Hex, 16, 16);
        public static FIDMeta PINSupervisor = new FIDMeta('c', ' ', "PIN/Supervisor", FormatEnum.Hex, 16, 16);
        public static FIDMeta RetailerID = new FIDMeta('d', ' ', "Retailer ID", FormatEnum.Numeric, 1, 12);
        public static FIDMeta POSConditionCode = new FIDMeta('e', ' ', "POS Condition Code", FormatEnum.Numeric, 2, 2);
        public static FIDMeta PINLengthOrReceiptData = new FIDMeta('f', ' ', "PIN Length or Receipt Data", FormatEnum.Hex, 1, 200);
        public static FIDMeta ResponseDisplay = new FIDMeta('g', ' ', "Response Display", FormatEnum.Hex, 1, 48);
        public static FIDMeta SequenceNumber = new FIDMeta('h', ' ', "Sequence Number", FormatEnum.Hex, 10, 10);
        public static FIDMeta SequenceNumberOriginal = new FIDMeta('i', ' ', "Sequence Number/Original", FormatEnum.Hex, 9, 9);
        public static FIDMeta StateCode = new FIDMeta('j', ' ', "State Code", FormatEnum.Hex, 2, 2);
        public static FIDMeta BirthDateDriversLicenseExpirationDateTerminalLocation = new FIDMeta('k', ' ', "Birth Date/Drivers License Expiration Date / Terminal Location", FormatEnum.Hex, 0, 25);
        public static FIDMeta TotalsBatch = new FIDMeta('l', ' ', "Totals/Batch", FormatEnum.Hex, 75, 75);
        public static FIDMeta TotalsDay = new FIDMeta('m', ' ', "Totals/Day", FormatEnum.Hex, 75, 75);
        public static FIDMeta TotalsEmployee = new FIDMeta('n', ' ', "Totals/Employee", FormatEnum.Hex, 75, 75);
        public static FIDMeta TotalsShift = new FIDMeta('o', ' ', "Totals/Shift", FormatEnum.Hex, 75, 75);
        public static FIDMeta Track2Customer = new FIDMeta('q', ' ', "Track 2/Customer", FormatEnum.Hex, 1, 40);
        public static FIDMeta Track2Supervisor = new FIDMeta('r', ' ', "Track 2/Supervisor", FormatEnum.Hex, 1, 40);
        public static FIDMeta TransactionDescription = new FIDMeta('s', ' ', "Transaction Description", FormatEnum.Hex, 1, 24);
        public static FIDMeta PINPadIdentifier = new FIDMeta('t', ' ', "PIN Pad Identifier", FormatEnum.Hex, 16, 16);
        public static FIDMeta AcceptorPostingDate = new FIDMeta('u', ' ', "Acceptor Posting Date", FormatEnum.Hex, 6, 6);
        public static FIDMeta AmericanExpressDataCollection = new FIDMeta('0', ' ', "American Express Data Collection", FormatEnum.Hex, 46, 118);
        public static FIDMeta PS2000Data = new FIDMeta('1', ' ', "PS2000 Data", FormatEnum.Hex, 24, 24);
        public static FIDMeta Track1Customer = new FIDMeta('2', ' ', "Track 1/Customer", FormatEnum.Hex, 1, 82);
        public static FIDMeta Track1Supervisor = new FIDMeta('3', ' ', "Track 1/Supervisor", FormatEnum.Hex, 1, 82);
        public static FIDMeta IndustryData = new FIDMeta('4', ' ', "Industry Data", FormatEnum.Hex, 156, 171);
        public static FIDMeta HostOriginalData = new FIDMeta('6', 'A', "Host original data", FormatEnum.Hex, 12, 12);
        public static FIDMeta ManualCardVerificationData1 = new FIDMeta('6', 'B', "Manual card verification data 1", FormatEnum.Hex, 4, 4);
        public static FIDMeta ManualCardVerificationData2 = new FIDMeta('6', 'C', "Manual card verification data 2", FormatEnum.Hex, 4, 4);
        public static FIDMeta PurchasingCardFleetCardData = new FIDMeta('6', 'D', "Purchasing card/fleet card data", FormatEnum.Hex, 30, 876);
        public static FIDMeta POSEntryMode = new FIDMeta('6', 'E', "POS entry mode", FormatEnum.Numeric, 3, 3);
        public static FIDMeta ElectronicCommerceData = new FIDMeta('6', 'F', "Electronic commerce data", FormatEnum.Hex, 1, 2);
        public static FIDMeta VisaCommercialCardTypeIndicator = new FIDMeta('6', 'G', "Visa commercial card type indicator", FormatEnum.Hex, 1, 1);
        public static FIDMeta CVDPresenceIndicatorAndCVDResult = new FIDMeta('6', 'H', "CVD presence indicator and CVD result", FormatEnum.Hex, 2, 2);
        public static FIDMeta TransactionCurrencyCode = new FIDMeta('6', 'I', "Transaction currency code", FormatEnum.Numeric, 3, 3);
        public static FIDMeta CardholderCertificateSerialNumber = new FIDMeta('6', 'J', "Cardholder certificate serial number", FormatEnum.Hex, 32, 32);
        public static FIDMeta MerchantCertificateSerialNumber = new FIDMeta('6', 'K', "Merchant certificate serial number", FormatEnum.Hex, 32, 32);
        public static FIDMeta XIDTransStain = new FIDMeta('6', 'L', "XID/trans stain", FormatEnum.Hex, 80, 80);
        public static FIDMeta MessageReasonCode = new FIDMeta('6', 'N', "Message reason code", FormatEnum.Numeric, 4, 4);
        public static FIDMeta EMVRequestData = new FIDMeta('6', 'O', "EMV request data", FormatEnum.Hex, 1, 136);
        public static FIDMeta EMVAdditionalRequestData = new FIDMeta('6', 'P', "EMV additional request data", FormatEnum.Hex, 1, 64);
        public static FIDMeta EMVResponseData = new FIDMeta('6', 'Q', "EMV response data", FormatEnum.Hex, 1, 64);
        public static FIDMeta EMVAdditionalResponseData = new FIDMeta('6', 'R', "EMV additional response data", FormatEnum.Hex, 1, 258);
        public static FIDMeta StoredValueData = new FIDMeta('6', 'S', "Stored value data", FormatEnum.Hex, 63, 63);
        public static FIDMeta KeySerialNumberAndDescriptor = new FIDMeta('6', 'T', "Key serial number and descriptor", FormatEnum.Hex, 23, 23);
        public static FIDMeta TransactionSubtypeData = new FIDMeta('6', 'U', "Transaction subtype data", FormatEnum.Hex, 16, 16);
        public static FIDMeta AuthenticationCollectionIndicator = new FIDMeta('6', 'V', "Authentication collection indicator", FormatEnum.Hex, 1, 1);
        public static FIDMeta CAVVAAVResultCode = new FIDMeta('6', 'W', "CAVV/AAV result code", FormatEnum.Hex, 1, 1);
        public static FIDMeta PointOfServiceData = new FIDMeta('6', 'X', "Point of service data", FormatEnum.Hex, 6, 6);
        public static FIDMeta AuthenticationData = new FIDMeta('6', 'Y', "Authentication data", FormatEnum.Hex, 2, 202);
        public static FIDMeta CardVerificationFlag2 = new FIDMeta('6', 'Z', "Card verification flag 2", FormatEnum.Hex, 1, 1);
        public static FIDMeta ElectronicCheckConversionData = new FIDMeta('6', 'b', "Electronic check conversion data", FormatEnum.Hex, 39, 39);
        public static FIDMeta MICRData = new FIDMeta('6', 'c', "MICR data", FormatEnum.Hex, 64, 64);
        public static FIDMeta ElectronicCheckCallbackInformation = new FIDMeta('6', 'd', "Electronic check callback information", FormatEnum.Hex, 115, 115);
        public static FIDMeta InterchangeComplianceData = new FIDMeta('6', 'e', "Interchange compliance data", FormatEnum.Hex, 21, 21);
        public static FIDMeta ResponseSourceOrReasonCode = new FIDMeta('6', 'f', "Response source or reason code", FormatEnum.Hex, 1, 1);
        public static FIDMeta POSMerchantData = new FIDMeta('6', 'g', "POS merchant data", FormatEnum.Hex, 4, 4);
        public static FIDMeta SystemTraceAuditNumberSTAN = new FIDMeta('6', 'h', "System Trace Audit Number (STAN)", FormatEnum.Hex, 6, 6);
        public static FIDMeta RetrievalReferenceNumber = new FIDMeta('6', 'i', "Retrieval Reference Number", FormatEnum.Hex, 12, 12);
        public static FIDMeta DebitNetworkSharingGroupID = new FIDMeta('6', 'j', "Debit Network/Sharing Group ID", FormatEnum.Hex, 4, 4);
        public static FIDMeta CardLevelResults = new FIDMeta('6', 'k', "Card Level Results", FormatEnum.Hex, 2, 2);
        public static FIDMeta HealthcareTransitData = new FIDMeta('6', 'l', "Healthcare/Transit Data", FormatEnum.Hex, 20, 120);
        public static FIDMeta HealthcareServiceData = new FIDMeta('6', 'm', "Healthcare Service Data", FormatEnum.Hex, 19, 95);
        public static FIDMeta ErrorFlag = new FIDMeta('6', 'n', "Error Flag", FormatEnum.Hex, 1, 1);
        public static FIDMeta AmericanExpressAdditionalData = new FIDMeta('6', 'o', "American Express Additional Data", FormatEnum.Hex, 3, 300);
        public static FIDMeta MobileTopupTrack2 = new FIDMeta('7', 'a', "Mobile Top-Up Track 2", FormatEnum.Hex, 1, 40);
        public static FIDMeta OriginalMobileTopupReferenceNumber = new FIDMeta('7', 'b', "Original Mobile Top-Up Reference Number", FormatEnum.Hex, 15, 15);
        public static FIDMeta MobileTopupResponse = new FIDMeta('7', 'c', "Mobile Top-Up Response", FormatEnum.Hex, 65, 65);
        public static FIDMeta EBTVoucherNumber = new FIDMeta('8', 'A', "EBT Voucher Number", FormatEnum.Hex, 18, 24);
        public static FIDMeta EBTAvailableBalance = new FIDMeta('8', 'B', "EBT Available Balance", FormatEnum.Numeric, 18, 18);
        public static FIDMeta FNBTerminalType = new FIDMeta('9', 'E', "FNB Terminal Type", FormatEnum.Hex, 2, 2);
        public static FIDMeta FNBMultilaneAdditionalData = new FIDMeta('9', 'I', "FNB Multilane additional data", FormatEnum.Hex, 44, 44);
        public static FIDMeta FNBAuthEntityPinPadSerialNumber = new FIDMeta('9', 'A', "FNB Auth Entity/Pin Pad Serial Number", FormatEnum.Hex, 1, 1);

        static FidMetaList()
        {
            metaList.Add(CustomerBillingAddress);
            metaList.Add(Amount1);
            metaList.Add(Amount2);
            metaList.Add(ApplicationAccountType);
            metaList.Add(ApplicationAccountNumber);
            metaList.Add(ApprovalCode);
            metaList.Add(AuthenticationCode);
            metaList.Add(AuthenticationKey);
            metaList.Add(DataEncryptionKey);
            metaList.Add(AvailableBalance);
            metaList.Add(BusinessDate);
            metaList.Add(CheckType);
            metaList.Add(CommunicationsKey);
            metaList.Add(CustomerID);
            metaList.Add(CustomerIDType);
            metaList.Add(DraftCaptureFlag);
            metaList.Add(EchoData);
            metaList.Add(CardType);
            metaList.Add(InvoiceNumber);
            metaList.Add(InvoiceNumberOriginal);
            metaList.Add(LanguageCode);
            metaList.Add(MailDownloadKey);
            metaList.Add(MailTextDownloadData);
            metaList.Add(ISOResponseCode);
            metaList.Add(CustomerZIPCode);
            metaList.Add(AddressVerificationStatusCode);
            metaList.Add(OptionalData);
            metaList.Add(PINCustomer);
            metaList.Add(PINSupervisor);
            metaList.Add(RetailerID);
            metaList.Add(POSConditionCode);
            metaList.Add(PINLengthOrReceiptData);
            metaList.Add(ResponseDisplay);
            metaList.Add(SequenceNumber);
            metaList.Add(SequenceNumberOriginal);
            metaList.Add(StateCode);
            metaList.Add(BirthDateDriversLicenseExpirationDateTerminalLocation);
            metaList.Add(TotalsBatch);
            metaList.Add(TotalsDay);
            metaList.Add(TotalsEmployee);
            metaList.Add(TotalsShift);
            metaList.Add(Track2Customer);
            metaList.Add(Track2Supervisor);
            metaList.Add(TransactionDescription);
            metaList.Add(PINPadIdentifier);
            metaList.Add(AcceptorPostingDate);
            metaList.Add(AmericanExpressDataCollection);
            metaList.Add(PS2000Data);
            metaList.Add(Track1Customer);
            metaList.Add(Track1Supervisor);
            metaList.Add(IndustryData);
            metaList.Add(HostOriginalData);
            metaList.Add(ManualCardVerificationData1);
            metaList.Add(ManualCardVerificationData2);
            metaList.Add(PurchasingCardFleetCardData);
            metaList.Add(POSEntryMode);
            metaList.Add(ElectronicCommerceData);
            metaList.Add(VisaCommercialCardTypeIndicator);
            metaList.Add(CVDPresenceIndicatorAndCVDResult);
            metaList.Add(TransactionCurrencyCode);
            metaList.Add(CardholderCertificateSerialNumber);
            metaList.Add(MerchantCertificateSerialNumber);
            metaList.Add(XIDTransStain);
            metaList.Add(MessageReasonCode);
            metaList.Add(EMVRequestData);
            metaList.Add(EMVAdditionalRequestData);
            metaList.Add(EMVResponseData);
            metaList.Add(EMVAdditionalResponseData);
            metaList.Add(StoredValueData);
            metaList.Add(KeySerialNumberAndDescriptor);
            metaList.Add(TransactionSubtypeData);
            metaList.Add(AuthenticationCollectionIndicator);
            metaList.Add(CAVVAAVResultCode);
            metaList.Add(PointOfServiceData);
            metaList.Add(AuthenticationData);
            metaList.Add(CardVerificationFlag2);
            metaList.Add(ElectronicCheckConversionData);
            metaList.Add(MICRData);
            metaList.Add(ElectronicCheckCallbackInformation);
            metaList.Add(InterchangeComplianceData);
            metaList.Add(ResponseSourceOrReasonCode);
            metaList.Add(POSMerchantData);
            metaList.Add(SystemTraceAuditNumberSTAN);
            metaList.Add(RetrievalReferenceNumber);
            metaList.Add(DebitNetworkSharingGroupID);
            metaList.Add(CardLevelResults);
            metaList.Add(HealthcareTransitData);
            metaList.Add(HealthcareServiceData);
            metaList.Add(ErrorFlag);
            metaList.Add(AmericanExpressAdditionalData);
            metaList.Add(MobileTopupTrack2);
            metaList.Add(OriginalMobileTopupReferenceNumber);
            metaList.Add(MobileTopupResponse);
            metaList.Add(EBTVoucherNumber);
            metaList.Add(EBTAvailableBalance);
            metaList.Add(FNBTerminalType);
            metaList.Add(FNBMultilaneAdditionalData);
            metaList.Add(FNBAuthEntityPinPadSerialNumber);

        }
    }

}
