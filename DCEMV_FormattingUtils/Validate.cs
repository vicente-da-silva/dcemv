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
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace DCEMV.FormattingUtils
{
    public static class Validate
    {
        public static bool PasswordValidation(string password)
        {
            /**
            * The Password must meet the following requirements: 
            * 1.) at least 1 upper case character 
            * 2.) at least 1 lower case character 
            * 3.) at least 1 numerical character 
            * 4.) at least 1 special character 
            * It also enforces a min and max length 
            * This regular expression match can be used for validating strong password. 
            **/
            if (password == null)
                return false;

            const string regex = @"^(?=^.{6,10}$)((?=.*\d)(?=.*[A-Z])(?=.*[a-z])(?=.*[^A-Za-z0-9]))^.*";
            return (Regex.IsMatch(password, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool PhoneNumberValidation(string phoneNumber)
        {
            if (phoneNumber == null)
                return false;

            const string regex = @"^\d{10}$";
            return (Regex.IsMatch(phoneNumber, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool CardSerialNumberValidation(string cardSerialNumber)
        {
            if (cardSerialNumber == null)
                return false;

            const string regex = @"^[0-9A-F]{16}$";
            return (Regex.IsMatch(cardSerialNumber, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool AlphaNumericValidation(string alphnumeric)
        {
            if (alphnumeric == null)
                return false;

            const string regex = @"^[0-9A-F]$";
            return (Regex.IsMatch(alphnumeric, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool AmountValidation(string amount)
        {
            if (amount == null)
                return false;

            const string regex = @"^(\d*\.\d{1,2}|\d+)$";
            return (Regex.IsMatch(amount, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static string AmountReplace(string amount)
        {
            if (amount.Count(x => x == '.') > 1)
            {
                int index = amount.IndexOf('.') + 1;
                string afterPoint = amount.Substring(index).Replace(".", "");
                amount = amount.Substring(0, index) + afterPoint;
            }

            if (amount.Count(x => x == '.') == 1)
            {
                int index = amount.IndexOf('.') + 1;
                string afterPoint = amount.Substring(index);
                if (afterPoint.Length > 2)
                {
                    afterPoint = afterPoint.Substring(0, 2);
                    amount = amount.Substring(0, index) + afterPoint;
                }
            }

            const string regex = "[^0-9 .]";
            return Regex.Replace(amount, regex, "");
        }
        public static int AmountToCents(string amount)
        {
            Decimal.TryParse(amount, NumberStyles.AllowDecimalPoint, null, out decimal val);
            return (int)(Math.Round(val, 2, MidpointRounding.AwayFromZero) * 100);
        }
        public static int AmountToCents(long amount)
        {
            Decimal.TryParse(Convert.ToString(amount), NumberStyles.AllowDecimalPoint, null, out decimal val);
            return (int)(Math.Round(val, 2, MidpointRounding.AwayFromZero) * 100);
        }
        public static string CentsToAmount(long amount)
        {
            Decimal.TryParse(Convert.ToString(amount), NumberStyles.AllowDecimalPoint, null, out decimal val);
            return Convert.ToString(val / 100);
        }
        public static bool NumberValidation(string number)
        {
            if (String.IsNullOrEmpty(number))
                return false;

            const string regex = @"^[0-9]{1,30}$";
            return (Regex.IsMatch(number, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool NameValidation(string amount)
        {
            if (String.IsNullOrEmpty(amount))
                return false;

            const string regex = @"^[a-zA-Z'.\s]{1,20}$";
            return (Regex.IsMatch(amount, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool EmailValidation(string email)
        {
            if (String.IsNullOrEmpty(email))
                return false;

            const string regex = @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                    @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-\w]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$";

            return (Regex.IsMatch(email, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
        public static bool GuidValidation(string guid)
        {
            if (String.IsNullOrEmpty(guid))
                return false;

            //const string regex = @"^[{(]?[0-9A-F]{8}[-]?([0-9A-F]{4}[-]?){3}[0-9A-F]{12}[)}]?$";
            const string regex = @"^[{(]?[0-9A-F]{32}[)}]?$";

            return (Regex.IsMatch(guid, regex, RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250)));
        }
    }
}
