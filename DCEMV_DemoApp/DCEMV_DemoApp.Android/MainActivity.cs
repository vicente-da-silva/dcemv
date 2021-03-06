﻿/*
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

using Android.App;
using Android.Content.PM;
using Android.OS;
using DCEMV.SPDHProtocol;
using DCEMV.ConfigurationManager;
using DCEMV.CardReaders.BTACS1255Driver;
using DCEMV.CardReaders.AndroidNFCDriver;
using DCEMV.DemoApp.Proxies;
using DCEMV.SimulatedPaymentProvider;

namespace DCEMV.DemoApp.Android
{
    [Activity(Label = "DCEMV_DemoApp", Icon = "@drawable/icon", Theme = "@style/splashscreen", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = DCEMV.DemoApp.Android.Resource.Layout.Tabbar;
            ToolbarResource = DCEMV.DemoApp.Android.Resource.Layout.Toolbar;

            base.SetTheme(DCEMV.DemoApp.Android.Resource.Style.MainTheme);
            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            
            //LoadApplication(new App(null,new AndroidNFCCardReader(this), new DCEMVServerOnlineApprover(), new CodeBasedConfigurationProvider(), new TCPClientStreamAndroid()));
            //LoadApplication(new App(null, new ACS1255CardReader(this), new DCEMVServerOnlineApprover(), new CodeBasedConfigurationProvider(), new TCPClientStreamAndroid()));

            LoadApplication(new App(null, new AndroidNFCCardReader(this), new SimulatedApprover(), new CodeBasedConfigurationProvider(), new TCPClientStreamAndroid()));
            //LoadApplication(new App(null, new ACS1255CardReader(this), new SimulatedApprover(), new CodeBasedConfigurationProvider(), new TCPClientStreamAndroid()));
        }
    }
}

