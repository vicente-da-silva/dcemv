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
namespace DCEMV.EMVSecurity
{
    public enum MKDMethod
    {

        /**
         * Uses PAN, PAN Sequence Number, IMK, Triple DES
         * Described in EMV v4.2 Book 2, Annex A1.4.1
         */
        OPTION_A

        /**
        * Uses PAN, PAN Sequence Number, IMK, Triple DES and SHA-1
        * and decimalisation of hex digits.
        * Described in EMV v4.2 Book 2, Annex A1.4.2
        * NOTE: For PAN with length less or equals 16 it works as {@code OPTION_A}
        */
        , OPTION_B

    }
    public enum KeyScheme
    {

        /**
         * Encryption of a single length DES key using X9.17 methods.
         * <p>
         * Used for encryption of keys under a variant LMK..
         */
        Z,

        /**
         * Encryption of a double length key using X9.17 methods.
         */
        X,

        /**
         * Encryption of a double length DES key using the variant method.
         * <p>
         * Used for encryption of keys under a variant LMK.
         */
        U,

        /**
         * Encryption of a triple length key using X9.17 methods.
         */
        Y,

        /**
         * Encryption of a triple length DES key using the variant method.
         * <p>
         * Used for encryption of keys under a variant LMK.
         */
        T
    }
    public enum SKDMethod
    {

        /**
        * Visa Smart Debit/Credit or UKIS in England
        * <br>
        * Described in Visa Integrated Circuit Card
        * Specification (VIS) Version 1.5 - May 2009, section B.4
        */
        VSDC

            /**
            * MasterCard Proprietary SKD method
            */
            , MCHIP
            /**
            * American Express
            */
            , AEPIS_V40

            /**
            * EMV Common Session Key Derivation Method
            * Described in EMV v4.2 Book 2 - June 2008, Annex A1.3
            */
            , EMV_CSKD

            /**
            * EMV2000 Session Key Method
            * Described in EMV 2000 v4.0 Book 2 - December 2000, Annex A1.3
            */
            , EMV2000_SKM

    }
    public enum CipherMode
    {

        /**
         * Electronic Code Book.
         */
        ECB,

        /**
         * Cipher-block chaining.
         */
        CBC,

        /**
         * Cipher feedback, self-synchronizing with 8 bit shift register.
         */
        CFB8,

        /**
         * Cipher feedback, self-synchronizing with 64 bit shift register.
         */
        CFB64

    }
    public enum CipherDirection
    {
        ENCRYPT_MODE,
        DECRYPT_MODE,
    }
    public enum DigestMode
    {
        SHA1,
    }
    public enum ARPCMethod
    {

        /**
        * Method for the generation of an 8-byte ARPC consists of applying
        * the Triple-DES algorithm:
        * <li>the 8-byte ARQC
        * <li>the 2-byte Authorisation Response Code (ARC)
        */
        METHOD_1

       /**
       * Method For the generation of a 4-byte ARPC consists of applying
       * the MAC algorithm:
       * <li>the 4-byte ARQC
       * <li>the 4-byte binary Card Status Update (CSU)
       * <li>the 0-8 byte binary Proprietary Authentication Data
       */
       , METHOD_2

    }
}
