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
using System.Linq;

namespace DCEMV.PersoApp
{
    public class SessionSingleton
    {
        private const string DEVICE_ID_KEY = "DeviceId";
        public String SecurityDomainAID { get; set; }
        public String SecurityDomainMasterKey { get; set; }

        public static string DeviceId { get
            {
                string deviceid = null;
                if (App.Current.Properties.Keys.ToList().Exists(x=>x == DEVICE_ID_KEY))
                    deviceid = (string)App.Current.Properties[DEVICE_ID_KEY];
                return deviceid;
            }
            set
            {
                App.Current.Properties[DEVICE_ID_KEY] = value;
                App.Current.SavePropertiesAsync();
            }
        }

        private static readonly SessionSingleton instance = new SessionSingleton();

        private SessionSingleton()
        {
            SecurityDomainAID = "A000000018434D00";
            SecurityDomainMasterKey = "12345678901234567890123456789012";
        }

        public static SessionSingleton Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
