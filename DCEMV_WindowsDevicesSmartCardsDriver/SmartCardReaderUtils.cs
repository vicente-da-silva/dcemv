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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.SmartCards;

namespace DCEMV.CardReaders.WindowsDevicesSmartCardsDriver
{
    public class SmartCardReaderUtils
    {
        /// <summary>
        /// Checks whether the device supports smart card reader mode
        /// </summary>
        /// <returns>None</returns>
        public static async Task<DeviceInformation> GetFirstSmartCardReaderInfo(SmartCardReaderKind readerKind = SmartCardReaderKind.Any)
        {
            // Check if the SmartCardConnection API exists on this currently running SKU of Windows
            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.SmartCards.SmartCardConnection"))
            {
                // This SKU of Windows does not support NFC card reading, only desktop and phones/mobile devices support NFC card reading
                return null;
            }

            // Device selector string is from SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc) except that we remove the conditional
            // about the interface being enabled, since we want to get the device object regardless of whether the user turned it off in the CPL
            string query = "System.Devices.InterfaceClassGuid:=\"{DEEBE6AD-9E01-47E2-A3B2-A66AA2C036C9}\"";
            if (readerKind != SmartCardReaderKind.Any)
            {
                query += " AND System.Devices.SmartCards.ReaderKind:=" + (int)readerKind;
            }

            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(query);

            // There is a bug on some devices that were updated to WP8.1 where an NFC SmartCardReader is
            // enumerated despite that the device does not support it. As a workaround, we can do an additonal check
            // to ensure the device truly does support it.
            var workaroundDetect = await DeviceInformation.FindAllAsync("System.Devices.InterfaceClassGuid:=\"{50DD5230-BA8A-11D1-BF5D-0000F805F530}\"");

            if (workaroundDetect.Count == 0 || devices.Count == 0)
            {
                // Not supported
                return null;
            }

            // Smart card reader supported, but may be disabled
            return (from d in devices
                    where d.IsEnabled
                    && d.Id.ToLower().Contains("contactless") 
                    orderby d.IsDefault descending
                    select d).First();
        }

        public static async Task<List<string>> GetAllSmartCardReaderInfo(SmartCardReaderKind readerKind = SmartCardReaderKind.Any)
        {
            // Check if the SmartCardConnection API exists on this currently running SKU of Windows
            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Devices.SmartCards.SmartCardConnection"))
            {
                // This SKU of Windows does not support NFC card reading, only desktop and phones/mobile devices support NFC card reading
                return new List<string>();
            }

            // Device selector string is from SmartCardReader.GetDeviceSelector(SmartCardReaderKind.Nfc) except that we remove the conditional
            // about the interface being enabled, since we want to get the device object regardless of whether the user turned it off in the CPL
            string query = "System.Devices.InterfaceClassGuid:=\"{DEEBE6AD-9E01-47E2-A3B2-A66AA2C036C9}\"";
            if (readerKind != SmartCardReaderKind.Any)
            {
                query += " AND System.Devices.SmartCards.ReaderKind:=" + (int)readerKind;
            }

            DeviceInformationCollection devices = await DeviceInformation.FindAllAsync(query);

            // There is a bug on some devices that were updated to WP8.1 where an NFC SmartCardReader is
            // enumerated despite that the device does not support it. As a workaround, we can do an additonal check
            // to ensure the device truly does support it.
            var workaroundDetect = await DeviceInformation.FindAllAsync("System.Devices.InterfaceClassGuid:=\"{50DD5230-BA8A-11D1-BF5D-0000F805F530}\"");

            if (workaroundDetect.Count == 0 || devices.Count == 0)
            {
                // Not supported
                return new List<string>();
            }

            // Smart card reader supported, but may be disabled
            return (from d in devices
                    where d.IsEnabled
                    orderby d.IsDefault descending
                    select d.Id).ToList();
        }
    }
}
