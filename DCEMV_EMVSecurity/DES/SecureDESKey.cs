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
using DCEMV.FormattingUtils;
using System;
using System.Text.RegularExpressions;

namespace DCEMV.EMVSecurity
{
    public abstract class SecureKey
    {
        protected byte[] keyBytes = null;
        protected short keyLength;
        protected String keyType;
        protected KeyScheme? scheme;

        public byte[] GetKeyBytes()
        {
            return keyBytes;
        }
        public short GetKeyLength()
        {
            return keyLength;
        }
        public String GetKeyType()
        {
            return this.keyType;
        }

        public abstract KeyScheme GetScheme();
    }
    public abstract class SecureVariantKey : SecureKey
    {
        protected Byte? variant;

        public void SetVariant(byte variant)
        {
            this.variant = variant;
        }
        public abstract byte GetVariant();
    }
    /**
     * The Single, double or triple length DES keys encrypted under one of the Local Master Keys of the security module.
    **/
    public class SecureDESKey : SecureVariantKey
    {
        protected static string KEY_TYPE_PATTERN = ("([^:;]*)([:;])?([^:;])?([^:;])?");

        protected byte[] keyCheckValue = null;

        public SecureDESKey(short keyLength, String keyType, String keyHexString, String keyCheckValueHexString)
            : this(keyLength, keyType, Formatting.HexStringToByteArray(keyHexString), Formatting.HexStringToByteArray(keyCheckValueHexString))
        {
        }

        public SecureDESKey(short keyLength, String keyType, byte[] keyBytes, byte[] keyCheckValue)
        {
            SetKeyLength(keyLength);
            SetKeyType(keyType);
            SetKeyBytes(keyBytes);
            SetKeyCheckValue(keyCheckValue);
            GetVariant(); //only for set variant with defaults
            GetScheme();  //only set scheme with defaults
        }

       

        public void SetKeyLength(short keyLength)
        {
            this.keyLength = keyLength;
        }

        public void SetKeyBytes(byte[] keyBytes)
        {
            this.keyBytes = keyBytes;
        }

        public void SetKeyType(String keyType)
        {
            this.keyType = keyType;
        }

        public void SetKeyCheckValue(byte[] keyCheckValue)
        {
            this.keyCheckValue = keyCheckValue;
        }

        public override KeyScheme GetScheme()
        {
            if (scheme != null)
                return scheme.Value;
            /**
             * Some scheme derivation if it hasn't been explicity stated
             */
            switch (keyLength)
            {
                case SMAdapter.LENGTH_DES:
                    scheme = KeyScheme.Z; break;
                case SMAdapter.LENGTH_DES3_2KEY:
                    scheme = KeyScheme.X; break;
                case SMAdapter.LENGTH_DES3_3KEY:
                    scheme = KeyScheme.Y; break;
            }
            Regex rx = new Regex(KEY_TYPE_PATTERN);
            Match m = rx.Match(keyType);
            if (m.Groups[4] != null)
                try
                {
                    scheme = (KeyScheme)Enum.Parse(typeof(KeyScheme), m.Groups[4].Value);
                }
                catch (Exception ex)
                {
                    throw new FormatException("Value " + m.Groups[4] + " is not valid key scheme", ex);
                }
            return scheme.Value;
        }

        public override byte GetVariant()
        {
            if (variant != null)
                return variant.Value;
            /**
             * Some variant derivation if it hasn't been explicity stated
             */
            variant = 0;
            Regex rx = new Regex(KEY_TYPE_PATTERN);
            Match m = rx.Match(keyType);
            if (m.Groups[3] != null)
                try
                {
                    variant = Byte.Parse(m.Groups[3].Value);
                }
                catch (FormatException ex)
                {
                    throw new FormatException("Value " + m.Groups[4] + " is not valid key variant", ex);
                }
            return variant.Value;
        }
    }
}
