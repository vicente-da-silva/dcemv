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
using DCEMV.Shared;
using DCEMV.TLVProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Xamarin.Forms;

namespace DCEMV.DemoEMVApp
{
	public partial class App : Application
	{
        public App(ICardInterfaceManger contactCardInterfaceManger, ICardInterfaceManger contactlessCardInterfaceManger, IOnlineApprover onlineApprover, IConfigurationProvider configProvider, TCPClientStream tcpClientStream)
        {
            InitializeComponent();

            MainPage = new HomeMasterDetailView(contactCardInterfaceManger, contactlessCardInterfaceManger, onlineApprover, configProvider, tcpClientStream);
        }

        protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
