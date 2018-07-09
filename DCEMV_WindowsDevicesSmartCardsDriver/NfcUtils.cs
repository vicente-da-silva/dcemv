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

namespace DCEMV.CardReaders.WindowsDevicesSmartCardsDriver
{
    public class NfcUtils
    {
        public static async void LaunchNfcPaymentsSettingsPage()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-nfctransactions:"));
        }

        public static async void LaunchNfcProximitySettingsPage()
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-settings-proximity:"));
        }

        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException();
            }
            byte[] bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < hexString.Length; i += 2)
            {
                bytes[i / 2] = byte.Parse(hexString.Substring(i, 2), System.Globalization.NumberStyles.HexNumber);
            }
            return bytes;
        }
    }
}
