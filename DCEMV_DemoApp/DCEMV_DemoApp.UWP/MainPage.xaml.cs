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
using DCEMV.ConfigurationManager;
using DCEMV.CardReaders.WindowsCardEmulatorProxyDriver;
using DCEMV.DemoApp.Proxies;
using DCEMV.SPDHProtocol;
using DCEMV.CardReaders.WindowsDevicesSmartCardsDriver;
using DCEMV.CardReaders.NCIDriver;
using DCEMV.CardReaders.I2CRPi3NXPOM5577Driver;
using System.Globalization;
using DCEMV.SimulatedPaymentProvider;

namespace DCEMV.DemoApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            CultureInfo.CurrentUICulture = new CultureInfo("en-ZA");
            
            LoadApplication(new DemoApp.App(
                   null,
#if Pi
                   new NCICardReader(new I2CRPi3NXPOM5577HAL()),
#else
                   new Win10CardProxy("localhost", 50000),
#endif
                   //new SPDHApprover("192.168.1.107", 6010), 
                   //new DCEMVServerOnlineApprover(),
                   new SimulatedApprover(), 
                   new CodeBasedConfigurationProvider(),//FileBasedConfigurationProvider(new UniversalWindowsFileDriver()),
                   new TCPClientStreamUWP()));

        }
    }
}
