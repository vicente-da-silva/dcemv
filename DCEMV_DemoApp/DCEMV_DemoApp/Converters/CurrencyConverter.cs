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
using Xamarin.Forms;

namespace DCEMV.DemoApp
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (value is string)
                if (String.IsNullOrEmpty((string)value))
                    return null;
            
            decimal d = System.Convert.ToDecimal(value) / 100;
            return d.ToString("C");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;
            if (Decimal.TryParse(System.Convert.ToString(value), NumberStyles.Currency, culture, out decimal result))
                return (int)(Math.Round(result * 100, 2, MidpointRounding.AwayFromZero));
            return null;
        }
    }
}
