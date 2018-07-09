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
    internal class FID_A_CustomerBillingAddress : FIDBase
    {
        public FID_A_CustomerBillingAddress(byte[] value)
           : base('A', ' ', value)
        {
        }
    }
    internal class FID_B_Amount1 : FIDBase
    {
        public FID_B_Amount1(byte[] value)
           : base('B', ' ', value)
        {
        }
    }
    internal class FID_C_Amount2 : FIDBase
    {
        public FID_C_Amount2(byte[] value)
           : base('C', ' ', value)
        {
        }
    }
    internal class FID_D_ApplicationAccountType : FIDBase
    {
        public FID_D_ApplicationAccountType(byte[] value)
           : base('D', ' ', value)
        {
        }
    }
    internal class FID_E_ApplicationAccountNumber : FIDBase
    {
        public FID_E_ApplicationAccountNumber(byte[] value)
           : base('E', ' ', value)
        {
        }
    }
    internal class FID_F_ApprovalCode : FIDBase
    {
        public FID_F_ApprovalCode(byte[] value)
           : base('F', ' ', value)
        {
        }
    }
    internal class FID_G_AuthenticationCode : FIDBase
    {
        public FID_G_AuthenticationCode(byte[] value)
           : base('G', ' ', value)
        {
        }
    }
    internal class FID_H_AuthenticationKey : FIDBase
    {
        public FID_H_AuthenticationKey(byte[] value)
           : base('H', ' ', value)
        {
        }
    }
    internal class FID_I_DataEncryptionKey : FIDBase
    {
        public FID_I_DataEncryptionKey(byte[] value)
           : base('I', ' ', value)
        {
        }
    }
    internal class FID_J_AvailableBalance : FIDBase
    {
        public FID_J_AvailableBalance(byte[] value)
           : base('J', ' ', value)
        {
        }
    }
    internal class FID_K_BusinessDate : FIDBase
    {
        public FID_K_BusinessDate(byte[] value)
           : base('K', ' ', value)
        {
        }
    }
    internal class FID_L_CheckType : FIDBase
    {
        public FID_L_CheckType(byte[] value)
           : base('L', ' ', value)
        {
        }
    }
    internal class FID_M_CommunicationsKey : FIDBase
    {
        public FID_M_CommunicationsKey(byte[] value)
           : base('M', ' ', value)
        {
        }
    }
    internal class FID_N_CustomerID : FIDBase
    {
        public FID_N_CustomerID(byte[] value)
           : base('N', ' ', value)
        {
        }
    }
    internal class FID_O_CustomerIDType : FIDBase
    {
        public FID_O_CustomerIDType(byte[] value)
           : base('O', ' ', value)
        {
        }
    }
    internal class FID_P_DraftCaptureFlag : FIDBase
    {
        public FID_P_DraftCaptureFlag(byte[] value)
           : base('P', ' ', value)
        {
        }
    }
    internal class FID_Q_EchoData : FIDBase
    {
        public FID_Q_EchoData(byte[] value)
           : base('Q', ' ', value)
        {
        }
    }
    internal class FID_R_CardType : FIDBase
    {
        public FID_R_CardType(byte[] value)
           : base('R', ' ', value)
        {
        }
    }
    internal class FID_S_InvoiceNumber : FIDBase
    {
        public FID_S_InvoiceNumber(byte[] value)
           : base('S', ' ', value)
        {
        }
    }
    internal class FID_T_InvoiceNumber_Original : FIDBase
    {
        public FID_T_InvoiceNumber_Original(byte[] value)
           : base('T', ' ', value)
        {
        }
    }
    internal class FID_U_LanguageCode : FIDBase
    {
        public FID_U_LanguageCode(byte[] value)
           : base('U', ' ', value)
        {
        }
    }
    internal class FID_V_Mail_DownloadKey : FIDBase
    {
        public FID_V_Mail_DownloadKey(byte[] value)
           : base('V', ' ', value)
        {
        }
    }
    internal class FID_W_MailText_DownloadData : FIDBase
    {
        public FID_W_MailText_DownloadData(byte[] value)
           : base('W', ' ', value)
        {
        }
    }
    internal class FID_X_ISOResponseCode : FIDBase
    {
        public FID_X_ISOResponseCode(byte[] value)
           : base('X', ' ', value)
        {
        }
    }
    internal class FID_Y_CustomerZIPCode : FIDBase
    {
        public FID_Y_CustomerZIPCode(byte[] value)
           : base('Y', ' ', value)
        {
        }
    }
    internal class FID_Z_AddressVerificationStatusCode : FIDBase
    {
        public FID_Z_AddressVerificationStatusCode(byte[] value)
           : base('Z', ' ', value)
        {
        }
    }
    internal class FID_a_OptionalData : FIDBase
    {
        public FID_a_OptionalData(byte[] value)
           : base('a', ' ', value)
        {
        }
    }
    internal class FID_b_PIN_Customer : FIDBase
    {
        public FID_b_PIN_Customer(byte[] value)
           : base('b', ' ', value)
        {
        }
    }
    internal class FID_c_PIN_Supervisor : FIDBase
    {
        public FID_c_PIN_Supervisor(byte[] value)
           : base('c', ' ', value)
        {
        }
    }
    internal class FID_d_RetailerID : FIDBase
    {
        public FID_d_RetailerID(byte[] value)
           : base('d', ' ', value)
        {
        }
    }
    internal class FID_e_POSConditionCode : FIDBase
    {
        public FID_e_POSConditionCode(byte[] value)
           : base('e', ' ', value)
        {
        }
    }
    internal class FID_f_PINLengthOrReceiptData : FIDBase
    {
        public FID_f_PINLengthOrReceiptData(byte[] value)
           : base('f', ' ', value)
        {
        }
    }
    internal class FID_g_ResponseDisplay : FIDBase
    {
        public FID_g_ResponseDisplay(byte[] value)
           : base('g', ' ', value)
        {
        }
    }
    internal class FID_h_SequenceNumber : FIDBase
    {
        public FID_h_SequenceNumber(byte[] value)
           : base('h', ' ', value)
        {
        }
    }
    internal class FID_i_SequenceNumber_Original : FIDBase
    {
        public FID_i_SequenceNumber_Original(byte[] value)
           : base('i', ' ', value)
        {
        }
    }
    internal class FID_j_StateCode : FIDBase
    {
        public FID_j_StateCode(byte[] value)
           : base('j', ' ', value)
        {
        }
    }
    internal class FID_k_BirthDate_DriversLicenseExpirationDate_TerminalLocation : FIDBase
    {
        public FID_k_BirthDate_DriversLicenseExpirationDate_TerminalLocation(byte[] value)
           : base('k', ' ', value)
        {
        }
    }
    internal class FID_l_Totals_Batch : FIDBase
    {
        public FID_l_Totals_Batch(byte[] value)
           : base('l', ' ', value)
        {
        }
    }
    internal class FID_m_Totals_Day : FIDBase
    {
        public FID_m_Totals_Day(byte[] value)
           : base('m', ' ', value)
        {
        }
    }
    internal class FID_n_Totals_Employee : FIDBase
    {
        public FID_n_Totals_Employee(byte[] value)
           : base('n', ' ', value)
        {
        }
    }
    internal class FID_o_Totals_Shift : FIDBase
    {
        public FID_o_Totals_Shift(byte[] value)
           : base('o', ' ', value)
        {
        }
    }
    internal class FID_q_Track2_Customer : FIDBase
    {
        public FID_q_Track2_Customer(byte[] value)
           : base('q', ' ', value)
        {
        }
    }
    internal class FID_r_Track2_Supervisor : FIDBase
    {
        public FID_r_Track2_Supervisor(byte[] value)
           : base('r', ' ', value)
        {
        }
    }
    internal class FID_s_TransactionDescription : FIDBase
    {
        public FID_s_TransactionDescription(byte[] value)
           : base('s', ' ', value)
        {
        }
    }
    internal class FID_t_PINPadIdentifier : FIDBase
    {
        public FID_t_PINPadIdentifier(byte[] value)
           : base('t', ' ', value)
        {
        }
    }
    internal class FID_u_AcceptorPostingDate : FIDBase
    {
        public FID_u_AcceptorPostingDate(byte[] value)
           : base('u', ' ', value)
        {
        }
    }
    internal class FID_0_AmericanExpressDataCollection : FIDBase
    {
        public FID_0_AmericanExpressDataCollection(byte[] value)
           : base('0', ' ', value)
        {
        }
    }
    internal class FID_1_PS2000Data : FIDBase
    {
        public FID_1_PS2000Data(byte[] value)
           : base('1', ' ', value)
        {
        }
    }
    internal class FID_2_Track1_Customer : FIDBase
    {
        public FID_2_Track1_Customer(byte[] value)
           : base('2', ' ', value)
        {
        }
    }
    internal class FID_3_Track1_Supervisor : FIDBase
    {
        public FID_3_Track1_Supervisor(byte[] value)
           : base('3', ' ', value)
        {
        }
    }
    internal class FID_4_IndustryData : FIDBase
    {
        public FID_4_IndustryData(byte[] value)
           : base('4', ' ', value)
        {
        }
    }
    internal class FID_6_A_HostOriginalData : FIDBase
    {
        public FID_6_A_HostOriginalData(byte[] value)
           : base('6', 'A', value)
        {
        }
    }
    internal class FID_6_B_ManualCardVerificationData1 : FIDBase
    {
        public FID_6_B_ManualCardVerificationData1(byte[] value)
           : base('6', 'B', value)
        {
        }
    }
    internal class FID_6_C_ManualCardVerificationData2 : FIDBase
    {
        public FID_6_C_ManualCardVerificationData2(byte[] value)
           : base('6', 'C', value)
        {
        }
    }
    internal class FID_6_D_PurchasingCard_FleetCardData : FIDBase
    {
        public FID_6_D_PurchasingCard_FleetCardData(byte[] value)
           : base('6', 'D', value)
        {
        }
    }
    internal class FID_6_E_POSEntryMode : FIDBase
    {
        public FID_6_E_POSEntryMode(byte[] value)
           : base('6', 'E', value)
        {
        }
    }
    internal class FID_6_F_ElectronicCommerceData : FIDBase
    {
        public FID_6_F_ElectronicCommerceData(byte[] value)
           : base('6', 'F', value)
        {
        }
    }
    internal class FID_6_G_VisaCommercialCardTypeIndicator : FIDBase
    {
        public FID_6_G_VisaCommercialCardTypeIndicator(byte[] value)
           : base('6', 'G', value)
        {
        }
    }
    internal class FID_6_H_CVDPresenceIndicatorAndCVDResult : FIDBase
    {
        public FID_6_H_CVDPresenceIndicatorAndCVDResult(byte[] value)
           : base('6', 'H', value)
        {
        }
    }
    internal class FID_6_I_TransactionCurrencyCode : FIDBase
    {
        public FID_6_I_TransactionCurrencyCode(byte[] value)
           : base('6', 'I', value)
        {
        }
    }
    internal class FID_6_J_CardholderCertificateSerialNumber : FIDBase
    {
        public FID_6_J_CardholderCertificateSerialNumber(byte[] value)
           : base('6', 'J', value)
        {
        }
    }
    internal class FID_6_K_MerchantCertificateSerialNumber : FIDBase
    {
        public FID_6_K_MerchantCertificateSerialNumber(byte[] value)
           : base('6', 'K', value)
        {
        }
    }
    internal class FID_6_L_XID_TransStain : FIDBase
    {
        public FID_6_L_XID_TransStain(byte[] value)
           : base('6', 'L', value)
        {
        }
    }
    internal class FID_6_N_MessageReasonCode : FIDBase
    {
        public FID_6_N_MessageReasonCode(byte[] value)
           : base('6', 'N', value)
        {
        }
    }
    internal class FID_6_O_EMVRequestData : FIDBase
    {
        public FID_6_O_EMVRequestData(byte[] value)
           : base('6', 'O', value)
        {
        }
    }
    internal class FID_6_P_EMVAdditionalRequestData : FIDBase
    {
        public FID_6_P_EMVAdditionalRequestData(byte[] value)
           : base('6', 'P', value)
        {
        }
    }
    internal class FID_6_Q_EMVResponseData : FIDBase
    {
        public FID_6_Q_EMVResponseData(byte[] value)
           : base('6', 'Q', value)
        {
        }
    }
    internal class FID_6_R_EMVAdditionalResponseData : FIDBase
    {
        public FID_6_R_EMVAdditionalResponseData(byte[] value)
           : base('6', 'R', value)
        {
        }
    }
    internal class FID_6_S_StoredValueData : FIDBase
    {
        public FID_6_S_StoredValueData(byte[] value)
           : base('6', 'S', value)
        {
        }
    }
    internal class FID_6_T_KeySerialNumberAndDescriptor : FIDBase
    {
        public FID_6_T_KeySerialNumberAndDescriptor(byte[] value)
           : base('6', 'T', value)
        {
        }
    }
    internal class FID_6_U_TransactionSubtypeData : FIDBase
    {
        public FID_6_U_TransactionSubtypeData(byte[] value)
           : base('6', 'U', value)
        {
        }
    }
    internal class FID_6_V_AuthenticationCollectionIndicator : FIDBase
    {
        public FID_6_V_AuthenticationCollectionIndicator(byte[] value)
           : base('6', 'V', value)
        {
        }
    }
    internal class FID_6_W_CAVV_AAVResultCode : FIDBase
    {
        public FID_6_W_CAVV_AAVResultCode(byte[] value)
           : base('6', 'W', value)
        {
        }
    }
    internal class FID_6_X_PointOfServiceData : FIDBase
    {
        public FID_6_X_PointOfServiceData(byte[] value)
           : base('6', 'X', value)
        {
        }
    }
    internal class FID_6_Y_AuthenticationData : FIDBase
    {
        public FID_6_Y_AuthenticationData(byte[] value)
           : base('6', 'Y', value)
        {
        }
    }
    internal class FID_6_Z_CardVerificationFlag2 : FIDBase
    {
        public FID_6_Z_CardVerificationFlag2(byte[] value)
           : base('6', 'Z', value)
        {
        }
    }
    internal class FID_6_b_ElectronicCheckConversionData : FIDBase
    {
        public FID_6_b_ElectronicCheckConversionData(byte[] value)
           : base('6', 'b', value)
        {
        }
    }
    internal class FID_6_c_MICRData : FIDBase
    {
        public FID_6_c_MICRData(byte[] value)
           : base('6', 'c', value)
        {
        }
    }
    internal class FID_6_d_ElectronicCheckCallbackInformation : FIDBase
    {
        public FID_6_d_ElectronicCheckCallbackInformation(byte[] value)
           : base('6', 'd', value)
        {
        }
    }
    internal class FID_6_e_InterchangeComplianceData : FIDBase
    {
        public FID_6_e_InterchangeComplianceData(byte[] value)
           : base('6', 'e', value)
        {
        }
    }
    internal class FID_6_f_ResponseSourceOrReasonCode : FIDBase
    {
        public FID_6_f_ResponseSourceOrReasonCode(byte[] value)
           : base('6', 'f', value)
        {
        }
    }
    internal class FID_6_g_POSMerchantData : FIDBase
    {
        public FID_6_g_POSMerchantData(byte[] value)
           : base('6', 'g', value)
        {
        }
    }
    internal class FID_6_h_SystemTraceAuditNumber_STAN_ : FIDBase
    {
        public FID_6_h_SystemTraceAuditNumber_STAN_(byte[] value)
           : base('6', 'h', value)
        {
        }
    }
    internal class FID_6_i_RetrievalReferenceNumber : FIDBase
    {
        public FID_6_i_RetrievalReferenceNumber(byte[] value)
           : base('6', 'i', value)
        {
        }
    }
    internal class FID_6_j_DebitNetwork_SharingGroupID : FIDBase
    {
        public FID_6_j_DebitNetwork_SharingGroupID(byte[] value)
           : base('6', 'j', value)
        {
        }
    }
    internal class FID_6_k_CardLevelResults : FIDBase
    {
        public FID_6_k_CardLevelResults(byte[] value)
           : base('6', 'k', value)
        {
        }
    }
    internal class FID_6_l_Healthcare_TransitData : FIDBase
    {
        public FID_6_l_Healthcare_TransitData(byte[] value)
           : base('6', 'l', value)
        {
        }
    }
    internal class FID_6_m_HealthcareServiceData : FIDBase
    {
        public FID_6_m_HealthcareServiceData(byte[] value)
           : base('6', 'm', value)
        {
        }
    }
    internal class FID_6_n_ErrorFlag : FIDBase
    {
        public FID_6_n_ErrorFlag(byte[] value)
           : base('6', 'n', value)
        {
        }
    }
    internal class FID_6_o_AmericanExpressAdditionalData : FIDBase
    {
        public FID_6_o_AmericanExpressAdditionalData(byte[] value)
           : base('6', 'o', value)
        {
        }
    }
    internal class FID_7_a_MobileTop_UpTrack2 : FIDBase
    {
        public FID_7_a_MobileTop_UpTrack2(byte[] value)
           : base('7', 'a', value)
        {
        }
    }
    internal class FID_7_b_OriginalMobileTop_UpReferenceNumber : FIDBase
    {
        public FID_7_b_OriginalMobileTop_UpReferenceNumber(byte[] value)
           : base('7', 'b', value)
        {
        }
    }
    internal class FID_7_c_MobileTop_UpResponse : FIDBase
    {
        public FID_7_c_MobileTop_UpResponse(byte[] value)
           : base('7', 'c', value)
        {
        }
    }
    internal class FID_8_A_EBTVoucherNumber : FIDBase
    {
        public FID_8_A_EBTVoucherNumber(byte[] value)
           : base('8', 'A', value)
        {
        }
    }
    internal class FID_8_B_EBTAvailableBalance : FIDBase
    {
        public FID_8_B_EBTAvailableBalance(byte[] value)
           : base('8', 'B', value)
        {
        }
    }
    internal class FID_9_E_FNBTerminalType : FIDBase
    {
        public FID_9_E_FNBTerminalType(byte[] value)
           : base('9', 'E', value)
        {
        }
    }
    internal class FID_9_I_FNBMultilaneAdditionalData : FIDBase
    {
        public FID_9_I_FNBMultilaneAdditionalData(byte[] value)
           : base('9', 'I', value)
        {
        }
    }
    internal class FID_9_A_FNBAuthEntity_PinPadSerialNumber : FIDBase
    {
        public FID_9_A_FNBAuthEntity_PinPadSerialNumber(byte[] value)
           : base('9', 'A', value)
        {
        }
    }

}
