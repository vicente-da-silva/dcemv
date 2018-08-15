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
using DataFormatters;
using DCEMV.FormattingUtils;
using System;

namespace DCEMV.EMVProtocol.Kernels
{
    public enum DataKernelID
    {
        KRN,
        K1,
        K2,
        K3,
        K4,
        K5,
        K6,
        K7,
        GP,
    }
    
    public enum DataTemplate
    {
        None,
        _BF0C,
        _77,
        _70_OR_77,
        _A5,
        _6F,
        _70,
        _BF0C_OR_70,
        _77_OR_80,
        _61,
        _61_OR_A5,
        _BF0C_OR_73,
        _71_OR_72,
    }
    //public enum DataSource
    //{
    //    Terminal,
    //    Card,
    //    Unknown,
    //    POS,
    //    Issuer,
    //    Issuer_Terminal,
    //    Kernel5,
    //    Terminal_Reader,
    //    Card_Terminal,
    //    DataExchange,
    //}
    
    public class EMVTagMeta
    {
        public string Tag { get; }
        public DataKernelID KernelId { get; }
        public DataFormatterBase DataFormatter { get; }
        public DataTemplate Template { get; }
        public string Name { get; }
        public string Description { get; }
        public bool IsSensitive { get; }
        public UpdatePermissionEnum[] Permissions { get; set; }

        public DateTime FormatAsDateTime(byte[] value)
        {
            if (DataFormatter.Format != DataFormats._DATE_YYMMDD && DataFormatter.Format != DataFormats._TIME_HHMMSS)
                throw new Exception("Cannot format value as DateTime");

            if (DataFormatter.Format == DataFormats._DATE_YYMMDD)
                return DateTime.ParseExact(Formatting.BcdToString(value), "yyMMdd", System.Globalization.CultureInfo.InvariantCulture);

            if (DataFormatter.Format == DataFormats._TIME_HHMMSS)
                return DateTime.ParseExact(Formatting.BcdToString(value), "HHmmss", System.Globalization.CultureInfo.InvariantCulture);

            throw new Exception("Cannot format value as DateTime");
        }
        public byte[] FormatFromDateTime(DateTime value)
        {

            if (DataFormatter.Format != DataFormats._DATE_YYMMDD && DataFormatter.Format != DataFormats._TIME_HHMMSS)
                throw new Exception("Cannot format value as DateTime");

            if (DataFormatter.Format == DataFormats._DATE_YYMMDD)
                return Formatting.StringToBcd(string.Format("{0:yyMMdd}", value), false);

            if (DataFormatter.Format == DataFormats._TIME_HHMMSS)
                return Formatting.StringToBcd(string.Format("{0:HHmmss}", value), false);

            throw new Exception("Cannot format value as DateTime");
        }

        public byte[] InitValue()
        {
            if (DataFormatter is DataFormatterLengthFixed)
                return new byte[(DataFormatter as DataFormatterLengthFixed).Max];

            if (DataFormatter is DataFormatterLengthRange)
                return new byte[(DataFormatter as DataFormatterLengthRange).Max];

            throw new Exception("Cannot init value with set NumberLengthBase");
        }

        

        public EMVTagMeta(
            DataKernelID kernelId,
            string tagName,
            string name ,
            DataTemplate template,
            DataFormatterBase dataFormatter,
            UpdatePermissionEnum[] permissions,
            string description)
        {
            Permissions = permissions;
            Tag = tagName;
            KernelId = kernelId;
            DataFormatter = dataFormatter;
            Template = template;
            Name = name;
            Description = description;
        }
    }
}
