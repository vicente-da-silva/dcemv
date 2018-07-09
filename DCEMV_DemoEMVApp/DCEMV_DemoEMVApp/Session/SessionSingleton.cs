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
using System.Linq;

namespace DCEMV.DemoEMVApp
{
    public class SessionSingleton
    {
        private const string CONTACT_DEVICE_ID_KEY = "ContactDeviceId";
        private const string CONTACTLESS_DEVICE_ID_KEY = "ContactlessDeviceId";
       
        public static string ContactDeviceId {
            get
            {
                string deviceid = null;
                if (App.Current.Properties.Keys.ToList().Exists(x=>x == CONTACT_DEVICE_ID_KEY))
                    deviceid = (string)App.Current.Properties[CONTACT_DEVICE_ID_KEY];
                return deviceid;
            }
            set
            {
                App.Current.Properties[CONTACT_DEVICE_ID_KEY] = value;
                App.Current.SavePropertiesAsync();
            }
        }
        public static string ContactlessDeviceId
        {
            get
            {
                string deviceid = null;
                if (App.Current.Properties.Keys.ToList().Exists(x => x == CONTACTLESS_DEVICE_ID_KEY))
                    deviceid = (string)App.Current.Properties[CONTACTLESS_DEVICE_ID_KEY];
                return deviceid;
            }
            set
            {
                App.Current.Properties[CONTACTLESS_DEVICE_ID_KEY] = value;
                App.Current.SavePropertiesAsync();
            }
        }

        private static readonly SessionSingleton instance = new SessionSingleton();
        
        public static SessionSingleton Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
