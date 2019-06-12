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

namespace DCEMV.TerminalCommon
{
    public enum TxFlow
    {
        In,
        Out,
    }
    public class TxFlowToObjectConverter<T> : IValueConverter
    {
        public T In { set; get; }

        public T Out { set; get; }
        
        public object Convert(object value, Type targetType,
                              object parameter, CultureInfo culture)
        {
            switch ((TxFlow)value)
            {
                case TxFlow.In:
                    return In;
                case TxFlow.Out:
                    return Out;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, CultureInfo culture)
        {
            if (((T)value).Equals(Out))
                return TxFlow.Out;

            if (((T)value).Equals(In))
                return TxFlow.In;

            return null;
        }
    }

}
