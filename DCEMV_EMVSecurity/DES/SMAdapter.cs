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
using System;

namespace DCEMV.EMVSecurity
{
    public class SMAdapter
    {
        /**
         * DES Key Length <code>LENGTH_DES</code> = 64.
         */
        public const short LENGTH_DES = 64;
        /**
         * Triple DES (2 keys) <code>LENGTH_DES3_2KEY</code> = 128.
         */
        public const short LENGTH_DES3_2KEY = 128;
        /**
         * Triple DES (3 keys) <code>LENGTH_DES3_3KEY</code> = 192.
         */
        public const short LENGTH_DES3_3KEY = 192;

        /**
         * ZMK: Zone Master Key 
         * 
         * is a DES (or Triple-DES) key-encryption key which is distributed manually in order that further keys can be exchanged automatically.
         */
        public const String TYPE_ZMK = "ZMK";

        /**
         * ZPK: Zone PIN Key.
         *
         * is a DES (or Triple-DES) data-encrypting key which is distributed automatically and is used to encrypt PINs for transfer between
         * communicating parties (e.g. between acquirers and issuers).
         */
        public const String TYPE_ZPK = "ZPK";

        /**
         * TMK: Terminal Master Key.
         *
         * is a  DES (or Triple-DES) key-encrypting key which is distributed manually, or automatically under a previously installed TMK. It is
         * used to distribute data-encrypting keys, whithin a local network, to an ATM or POS terminal or similar.
         */
        public const String TYPE_TMK = "TMK";

        /**
         * TPK: Terminal PIN Key.
         *
         * is a  DES (or Triple-DES) data-encrypting key which is used to encrypt PINs for transmission, within a local network,
         * between the terminal and the terminal data acquirer.
         */
        public const String TYPE_TPK = "TPK";

        /**
         * TAK: Terminal Authentication Key.
         *
         * is a  DES (or Triple-DES) data-encrypting key which is used to generate and verify a Message Authentication Code (MAC) when data
         * is transmitted, within a local network, between the terminal and the terminal data acquirer.
         */
        public const String TYPE_TAK = "TAK";

        /**
         * PVK: PIN Verification Key.
         * is a  DES (or Triple-DES) data-encrypting key which is used to generate and verify PIN verification data and thus verify the
         * authenticity of a PIN.
         */
        public const String TYPE_PVK = "PVK";

        /**
         * CVK: Card Verification Key.
         *
         * is similar for PVK but for card information instead of PIN
         */
        public const String TYPE_CVK = "CVK";

        /**
         * BDK: Base Derivation Key.
         * is a  Triple-DES key-encryption key used to derive transaction keys in DUKPT (see ANSI X9.24)
         */
        public const String TYPE_BDK = "BDK";

        /**
         * ZAK: Zone Authentication Key.
         *
         * a  DES (or Triple-DES) data-encrypting key that is distributed automatically, and is used to generate and verify a Message
         * Authentication Code (MAC) when data is transmitted between communicating parties (e.g. between acquirers and issuers)
         */
        public const String TYPE_ZAK = "ZAK";

        /**
         * MK-AC: Issuer Master Key for generating and verifying Application Cryptograms.
         */
        public const String TYPE_MK_AC = "MK-AC";

        /**
         * MK-SMI: Issuer Master Key for Secure Messaging Integrity.
         *
         * is a Triple-DES key which is used to generating Message Authrntication Codes (MAC) for scripts send to EMV chip cards.
         */
        public const String TYPE_MK_SMI = "MK-SMI";

        /**
         * MK-SMC: Issuer Master Key for Secure Messaging Confidentiality.
         *
         * is a Triple-DES data-encrypting key which is used to encrypt data (e.g. PIN block) in scripts send to EMV chip cards.
         */
        public const String TYPE_MK_SMC = "MK-SMC";

        /**
         * MK-CVC3: Issuer Master Key for generating and verifying Card Verification Code 3 (CVC3).
         */
        public const String TYPE_MK_CVC3 = "MK-CVC3";

        /**
         * MK-DAC Issuer Master Key for generating and verifying Data Authentication Codes.
         */
        public const String TYPE_MK_DAC = "MK-DAC";

        /**
         * MK-DN: Issuer Master Key for generating and verifying Dynamic Numbers.
         */
        public const String TYPE_MK_DN = "MK-DN";

        /**
         * ZEK: Zone Encryption Key.
         */
        public const String TYPE_ZEK = "ZEK";

        /**
         * DEK: Data Encryption Key.
         */
        public const String TYPE_DEK = "DEK";

        /**
         * RSA: Private Key.
         */
        public const String TYPE_RSA_SK = "RSA_SK";

        /**
         * HMAC: Hash Message Authentication Code <i>(with key usage)</i>.
         */
        public const String TYPE_HMAC = "HMAC";

        /**
         * RSA: Public Key.
         */
        public const String TYPE_RSA_PK = "RSA_PK";

        /**
         * PIN Block Format adopted by ANSI (ANSI X9.8) and is one of two formats supported by the ISO (ISO 95641 - format 0).
         */
        public const byte FORMAT01 = (byte)01;

        /**
         * PIN Block Format 02 supports Douctel ATMs.
         */
        public const byte FORMAT02 = (byte)02;

        /**
             * PIN Block Format 03 is the Diabold Pin Block format.
             */
        public const byte FORMAT03 = (byte)03;

        /**
         * PIN Block Format 04 is the PIN block format adopted by the PLUS network.
         */
        public const byte FORMAT04 = (byte)04;

        /**
         * PIN Block Format 05 is the ISO 9564-1 Format 1 PIN Block.
         */
        public const byte FORMAT05 = (byte)05;

        /**
         * PIN Block Format 34 is the standard EMV PIN block format. Is only avaliable as output of EMV PIN change commands.
         */
        public const byte FORMAT34 = (byte)34;

        /**
         * PIN Block Format 35 is the required by Europay/MasterCard for their Pay Now & Pay Later products.
         */
        public const byte FORMAT35 = (byte)35;

        /**
         * PIN Block Format 41 is the Visa format for PIN change without using the current PIN.
         */
        public const byte FORMAT41 = (byte)41;

        /**
         * PIN Block Format 42 is the Visa format for PIN change using the current (old) PIN.
         */
        public const byte FORMAT42 = (byte)42;

        /**
         * Proprietary PIN Block format.
         * <p>
         * Most Security Modules use a proprietary PIN Block format
         * when encrypting the PIN under the LMK of the Security Module
         * hence this format (FORMAT00).
         *
         * <p>
         * This is not a standard format, every Security Module would
         * interpret FORMAT00 differently.
         *
         * So, no interchange would accept PIN Blocks from other interchanges
         * using this format. It is useful only when working with PIN's inside
         * your own interchange.
         * </p>
         */
        public const byte FORMAT00 = (byte)00;
    }
}
