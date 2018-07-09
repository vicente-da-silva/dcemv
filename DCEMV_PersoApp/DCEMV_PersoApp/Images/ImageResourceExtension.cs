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
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace DCEMV.PersoApp
{
    [ContentProperty("Source")]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
                return null;

            return ProvideImageSource(Source);
        }

        public static ImageSource ProvideImageSource(string source)
        {
            string namespaceString = "DCEMV.PersoApp";
            if (Device.RuntimePlatform == Device.UWP)
            {
                return ImageSource.FromResource(namespaceString + ".UWP.Images." + source);
            }
            else if (Device.RuntimePlatform == Device.Android)
            {
                return ImageSource.FromResource(namespaceString + ".Android.Images." + source);
            }
            else if (Device.RuntimePlatform == Device.iOS)
            {
                return ImageSource.FromResource(namespaceString + ".iOS.Images." + source);
            }
            return null;
        }
    }
}
