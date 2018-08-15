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

namespace DCEMV.DemoServer
{
    public class ConfigSingleton
    {
        public static long MaxTransactionAmount { get { return 20000; } }
        public static long MaxTopUpTransactionAmount { get { return 20000; } }
        public static string ThisServerUrl { get { return "http://0.0.0.0:44359"; }  }
        public static string IdentityServerUrl { get { return Environment.GetEnvironmentVariable("ID_SERVER_URL"); } }

        public static ConfigSingleton Instance { get; } = new ConfigSingleton();
    }
}
