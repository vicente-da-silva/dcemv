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
        private static readonly ConfigSingleton instance = new ConfigSingleton();

        //private static string ID_SERVER_URL_ENV_VAR = "ID_SERVER_URL";
        public static long MaxTransactionAmount { get; set; }
        public static long MaxTopUpTransactionAmount { get; set; }

        public static string ThisServerUrl { get; set; }
        public static string IdentityServerUrl { get; set; }

        private ConfigSingleton()
        {
            MaxTransactionAmount = 20000;
            MaxTopUpTransactionAmount = 20000;
            string env = Environment.GetEnvironmentVariable("ID_SERVER_URL");
            if (String.IsNullOrEmpty(env))
                throw new TechnicalException("ID_SERVER_URL env variable not found");
            else
                IdentityServerUrl = env;
            ThisServerUrl = "http://0.0.0.0:44359";
        }

        public static ConfigSingleton Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
