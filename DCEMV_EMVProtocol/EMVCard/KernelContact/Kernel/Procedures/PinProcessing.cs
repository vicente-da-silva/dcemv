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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.FormattingUtils;
using System;
using System.Text;

namespace DCEMV.EMVProtocol.Contact
{
    public class PinProcessing
    {
        public static byte[] BuildPinVerifyData(KernelDatabaseBase database, CAPublicKeyCertificate caPublicKey, byte[] pinBlock, byte[] challenge)
        {
            IssuerPublicKeyCertificate ipk = IssuerPublicKeyCertificate.BuildAndValidatePublicKey(database, caPublicKey.Modulus, caPublicKey.Exponent);
            if (ipk == null) return null;

            int keyLength = 0;
            PublicKeyCertificate iccKey = IccPinKeyCertificate.BuildAndValidatePublicKey(database, ipk.Modulus, ipk.Exponent);
            if (iccKey == null)
            {
                iccKey = IccPublicKeyCertificate.BuildAndValidatePublicKey(database, database.StaticDataToBeAuthenticated, ipk.Modulus, ipk.Exponent);
                if (iccKey == null) return null;

                keyLength = ((IccPublicKeyCertificate)iccKey).ICCPublicKeyLength;
            }
            else
            {
                keyLength = ((IccPinKeyCertificate)iccKey).ICCPinKeyLength;
            }

            int paddingLength = keyLength - 17;
            byte[] padding = new byte[paddingLength];
            byte[] pinData = Formatting.ConcatArrays(new byte[] { 0x7F }, pinBlock, challenge, padding);

            //apply recovery function
            byte[] encryptedPin = PublicKeyCertificate.DecryptRSA(pinData, iccKey.Modulus, iccKey.Exponent);
            return encryptedPin;

        }
        //U in diagram
        public static bool VerifyOnlinePin(string pin, TERMINAL_VERIFICATION_RESULTS_95_KRN tvr, CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr)
        {
            if (string.IsNullOrEmpty(pin))
            {
                tvr.Value.PINEntryRequiredPINPadPresentButPINWasNotEntered = true;
                cvr.Value.CVMResult = 0x01;//failed

                cvr.UpdateDB();
                tvr.UpdateDB();
                return false;
            }
            //cvm and tvr already set correctly in CVM selection
            return true;
        }

        //X in diagram
        public static bool VerifyOfflinePin(string pin, int pinTryCounter, TERMINAL_VERIFICATION_RESULTS_95_KRN tvr, CARDHOLDER_VERIFICATION_METHOD_CVM_RESULTS_9F34_KRN cvr)
        {
            if (pinTryCounter == 0)
            {
                tvr.Value.PINTryLimitExceeded = true;
                cvr.Value.CVMResult = 0x01;//failed

                cvr.UpdateDB();
                tvr.UpdateDB();
                return false;
            }
            if (string.IsNullOrEmpty(pin))
            {
                tvr.Value.PINEntryRequiredPINPadPresentButPINWasNotEntered = true;
                cvr.Value.CVMResult = 0x01;//failed

                cvr.UpdateDB();
                tvr.UpdateDB();
                return false;
            }

            return true;
        }

        public static byte[] BuildPlainTextPinBlock(string pin)
        {
            string controlNibble = "2";
            string pinLength = Convert.ToString(pin.Length);
            string filler = "F";

            int fillerCount = 16 - 1 - 1 - pin.Length - 1; //16 - control - pin length length - pin length - filler

            StringBuilder fillerBuf = new StringBuilder();
            for (int i = 0; i < fillerCount; i++)
                fillerBuf.Append("F");

            string result = controlNibble + pinLength + pin + fillerBuf + filler;

            return Formatting.HexStringToByteArray(result);
        }
    }
}
