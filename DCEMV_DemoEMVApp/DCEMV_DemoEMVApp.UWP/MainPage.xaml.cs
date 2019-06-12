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
using DCEMV.CardReaders.I2CRPi3NXPOM5577Driver;
using DCEMV.CardReaders.NCIDriver;
using DCEMV.CardReaders.WindowsCardEmulatorProxyDriver;
using DCEMV.CardReaders.WindowsDevicesSmartCardsDriver;
using DCEMV.ConfigurationManager;
using DCEMV.SimulatedPaymentProvider;
using DCEMV.SPDHProtocol;

namespace DCEMV.DemoEMVApp.UWP
{
    public sealed partial class MainPage
    {
        public MainPage()
        {
            this.InitializeComponent();

            LoadApplication(new DemoEMVApp.App(
#if Pi
                null,
                new NCICardReader(new I2CRPi3NXPOM5577HAL()),
#else
                //new Win10CardProxy("127.0.0.1",50000),
                new Win10CardReader(), //set to null to disable interface
                new Win10CardReader(), //set to null to disable interface
                //null,
                //null,
#endif
                //new SPDHApprover("192.168.0.100", 6010),
                new SimulatedApprover(),
                new CodeBasedConfigurationProvider(),//FileBasedConfigurationProvider(new UniversalWindowsFileDriver()),
                new TCPClientStreamUWP()));
        }
    }
}
