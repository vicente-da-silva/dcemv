using DCEMV.FormattingUtils;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace DCEMV.EMVSecurity.PIN
{
    public class PinFormatter
    {
        private static string SPLIT_PIN_PATTERN = @"\[ :;,/.\]";
        private static short MIN_PIN_LENGTH = 4;
        private static short MAX_PIN_LENGTH = 12;
        private static byte[] fPaddingBlock = Formatting.HexStringToByteArray("FFFFFFFFFFFFFFFF");

        public static byte[] CalculateVISLegacyPinBlockCVN_10_18(String newPin, IKey deaKey)
        {
            byte[] block1 = Formatting.HexStringToByteArray(new String(FormatPINBlock(newPin, 0x0)));
            byte[] block2 = new byte[8];
            Array.Copy(deaKey.GetEncoded(), 4, block2, 4, 4);
            byte[] pinBlock = Formatting.Xor(block1, block2);
            byte length = (byte)pinBlock.Length;
            pinBlock = Formatting.ConcatArrays(new byte[] { length }, pinBlock);
            pinBlock = EMVDESSecurity.PaddingISO9797Method2(pinBlock);
            return pinBlock;
        }

        public static byte[] CalculateMChipPinBlockCVN_10(String newPin)
        {
            return Formatting.HexStringToByteArray(new String(FormatPINBlock(newPin, 0x2)));
        }

        private static String[] SplitPins(String pins)
        {
            String[] pin = new String[2];
            String[] p = Regex.Split(pins, SPLIT_PIN_PATTERN);
            pin[0] = p[0];
            if (p.Length >= 2)
                pin[1] = p[1];
            return pin;
        }
        private static bool IsVSDCPinBlockFormat(byte pinBlockFormat)
        {
            return pinBlockFormat == SMAdapter.FORMAT41 || pinBlockFormat == SMAdapter.FORMAT42;
        }
        private static char[] FormatPINBlock(String pin, int checkDigit)
        {
            char[] block = Formatting.ByteArrayToHexString(fPaddingBlock).ToCharArray();
            char[] pinLenHex = String.Format("{0:X2}", pin.Length).ToCharArray();
            pinLenHex[0] = (char)('0' + checkDigit);

            // pin length then pad with 'F'
            Array.Copy(pinLenHex, 0, block, 0, pinLenHex.Length);
            Array.Copy(pin.ToCharArray(), 0, block, pinLenHex.Length, pin.Length);
            return block;
        }
        private static byte[] CalculatePINBlock(String pin, byte pinBlockFormat, String accountNumber)
        {
            byte[] pinBlock = null;
            String oldPin = null;
            if (pinBlockFormat == SMAdapter.FORMAT42)
            {
                String[] p = SplitPins(pin);
                pin = p[0];
                oldPin = p[1];
                if (oldPin.Length < MIN_PIN_LENGTH || oldPin.Length > MAX_PIN_LENGTH)
                    throw new Exception("Invalid OLD PIN length: " + oldPin.Length);
                if (!Formatting.IsNumeric(oldPin))
                    throw new Exception("Invalid OLD PIN decimal digits: " + oldPin);
            }
            if (pin.Length < MIN_PIN_LENGTH || pin.Length > MAX_PIN_LENGTH)
                throw new Exception("Invalid PIN length: " + pin.Length);
            if (!Formatting.IsNumeric(pin))
                throw new Exception("Invalid PIN decimal digits: " + pin);
            if (IsVSDCPinBlockFormat(pinBlockFormat))
            {
                if (accountNumber.Length != 16)
                    throw new Exception("Invalid UDK-A: " + accountNumber + ". The length of the UDK-A must be 16 hexadecimal digits");
            }
            else if (accountNumber.Length != 12)
                throw new Exception("Invalid Account Number: " + accountNumber + ". The length of the account number must be 12 (the 12 right-most digits of the account number excluding the check digit)");
            switch (pinBlockFormat)
            {
                case SMAdapter.FORMAT00: // same as FORMAT01
                case SMAdapter.FORMAT01:
                    {
                        // Block 1
                        byte[] block1 = Formatting.HexStringToByteArray(new String(FormatPINBlock(pin, 0x0)));

                        // Block 2
                        byte[] block2 = Formatting.HexStringToByteArray("0000" + accountNumber);
                        // pinBlock
                        pinBlock = Formatting.Xor(block1, block2);
                    }
                    break;
                case SMAdapter.FORMAT03:
                    {
                        char[] block = Formatting.ByteArrayToHexString(fPaddingBlock).ToCharArray();
                        Array.Copy(pin.ToCharArray(), 0, block, 0, pin.Length);
                        pinBlock = Formatting.HexStringToByteArray(new String(block));
                    }
                    break;
                case SMAdapter.FORMAT05:
                    {
                        // Block 1
                        char[] block1 = FormatPINBlock(pin, 0x1);

                        // Block rnd
                        byte[] rnd = new byte[8];
                        Random r = new Random();
                        r.NextBytes(rnd);
                        
                        // Block 2
                        char[] block2 = Formatting.ByteArrayToHexString(rnd).ToCharArray();

                        // merge blocks
                        Array.Copy(block1, 0, block2, 0, pin.Length + 2);

                        // pinBlock
                        pinBlock = Formatting.HexStringToByteArray(new String(block2));
                    }
                    break;
                case SMAdapter.FORMAT34:
                    {
                        pinBlock = Formatting.HexStringToByteArray(new String(FormatPINBlock(pin, 0x2)));
                    }
                    break;
                case SMAdapter.FORMAT35:
                    {
                        // Block 1
                        byte[] block1 = Formatting.HexStringToByteArray(new String(FormatPINBlock(pin, 0x2)));

                        // Block 2
                        byte[] block2 = Formatting.HexStringToByteArray("0000" + accountNumber);
                        // pinBlock
                        pinBlock = Formatting.Xor(block1, block2);
                    }
                    break;
                case SMAdapter.FORMAT41:
                    {
                        // Block 1
                        byte[] block1 = Formatting.HexStringToByteArray(new String(FormatPINBlock(pin, 0x0)));

                        // Block 2 - account number should contain Unique DEA Key A (UDK-A)
                        byte[] block2 = Formatting.HexStringToByteArray("00000000" + accountNumber.Substring(accountNumber.Length - 8));
                        // pinBlock
                        pinBlock = Formatting.Xor(block1, block2);
                    }
                    break;
                case SMAdapter.FORMAT42:
                    {
                        // Block 1
                        byte[] block1 = Formatting.HexStringToByteArray(new String(FormatPINBlock(pin, 0x0)));

                        // Block 2 - account number should contain Unique DEA Key A (UDK-A)
                        byte[] block2 = Formatting.HexStringToByteArray("00000000" + accountNumber.Substring(accountNumber.Length - 8));
                        // Block 3 - old pin
                        byte[] block3 = Formatting.HexStringToByteArray(oldPin.PadRight(16, '0'));
                        // pinBlock
                        pinBlock = Formatting.Xor(block1, block2);
                        pinBlock = Formatting.Xor(pinBlock, block3);
                    }
                    break;
                default:
                    throw new Exception("Unsupported PIN format: " + pinBlockFormat);
            }
            return pinBlock;
        }
        private static void ValidatePinBlock(char[] pblock, int checkDigit, int padidx, int offset)

        {
            ValidatePinBlock(pblock, checkDigit, padidx, offset, 'F');
        }
        private static void ValidatePinBlock(char[] pblock, int checkDigit, int padidx, int offset, char padDigit)

        {
            // test pin block check digit
            if (checkDigit >= 0 && pblock[0] - '0' != checkDigit)
                throw new Exception("PIN Block Error - invalid check digit");
            // test pin block pdding
            int i = pblock.Length - 1;
            while (i >= padidx)
                if (pblock[i--] != padDigit && padDigit > 0)
                    throw new Exception("PIN Block Error - invalid padding");
            // test pin block digits
            while (i >= offset)
                if (pblock[i--] >= 'A')
                    throw new Exception("PIN Block Error - illegal pin digit");
            // test pin length
            int pinLength = padidx - offset;
            if (pinLength < MIN_PIN_LENGTH || pinLength > MAX_PIN_LENGTH)
                throw new Exception("PIN Block Error - invalid pin length: " + pinLength);
        }
        private static String CalculatePIN(byte[] pinBlock, byte pinBlockFormat, String accountNumber)
        {
            String pin = null;
            if (IsVSDCPinBlockFormat(pinBlockFormat))
            {
                if (accountNumber.Length != 16)
                    throw new Exception("Invalid UDK-A: " + accountNumber + ". The length of the UDK-A must be 16 hexadecimal digits");
            }
            else if (accountNumber.Length != 12)
                throw new Exception("Invalid Account Number: " + accountNumber + ". The length of the account number must be 12 (the 12 right-most digits of the account number excluding the check digit)");
            switch (pinBlockFormat)
            {
                case SMAdapter.FORMAT00: // same as format 01
                case SMAdapter.FORMAT01:
                    {
                        // Block 2
                        byte[] bl2 = Formatting.HexStringToByteArray("0000" + accountNumber);
                        // get Block1
                        byte[] bl1 = Formatting.Xor(pinBlock, bl2);
                        int pinLength = bl1[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(bl1).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x0;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT03:
                    {
                        String bl1 = Formatting.ByteArrayToHexString(pinBlock);
                        int padidx = bl1.IndexOf('F');
                        if (padidx < 0) padidx = 12;
                        char[] block1 = bl1.ToCharArray();
                        int checkDigit = -0x1;
                        int offset = 0;

                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT05:
                    {
                        // get Block1
                        byte[] bl1 = pinBlock;
                        int pinLength = bl1[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(bl1).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x01;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset, (char)0);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT34:
                    {
                        int pinLength = pinBlock[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(pinBlock).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x2;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT35:
                    {
                        // Block 2
                        byte[] bl2 = Formatting.HexStringToByteArray("0000" + accountNumber);
                        // get Block1
                        byte[] bl1 = Formatting.Xor(pinBlock, bl2);
                        int pinLength = bl1[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(bl1).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x2;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT41:
                    {
                        // Block 2 - account number should contain Unique DEA Key A (UDK-A)
                        byte[] bl2 = Formatting.HexStringToByteArray("00000000"
                                    + accountNumber.Substring(accountNumber.Length - 8));
                        // get Block1
                        byte[] bl1 = Formatting.Xor(pinBlock, bl2);
                        int pinLength = bl1[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(bl1).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x0;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset);
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                case SMAdapter.FORMAT42:
                    {
                        // Block 2 - account number should contain Unique DEA Key A (UDK-A)
                        byte[] bl2 = Formatting.HexStringToByteArray("00000000"
                                    + accountNumber.Substring(accountNumber.Length - 8));
                        // get Block1
                        byte[] bl1 = Formatting.Xor(pinBlock, bl2);
                        int pinLength = bl1[0] & 0x0f;
                        char[] block1 = Formatting.ByteArrayToHexString(bl1).ToCharArray();
                        int offset = 2;
                        int checkDigit = 0x0;
                        int padidx = pinLength + offset;
                        // test pin block
                        ValidatePinBlock(block1, checkDigit, padidx, offset, '0');
                        // get pin
                        pin = Formatting.ByteArrayToHexString(Formatting.copyOfRange(Formatting.ToByteArry(block1), offset, padidx));
                    }
                    break;
                default:
                    throw new Exception("Unsupported PIN Block format: " + pinBlockFormat);
            }
            return pin;
        }
    }
}
