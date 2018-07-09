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
namespace Pcsc.Common
{
    /// <summary>
    /// ICC Device class
    /// </summary>
    public enum DeviceClass : byte
    {
        Unknown = 0x00,
        StorageClass = 0x01,  // for PCSC class, there will be subcategory to identify the physical icc
        Iso14443P4 = 0x02,
        MifareDesfire = 0x03,
    }
}
