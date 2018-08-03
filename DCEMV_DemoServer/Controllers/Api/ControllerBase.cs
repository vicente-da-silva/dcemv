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
using DCEMV.EMVProtocol.Kernels;
using DCEMV.EMVSecurity;
using DCEMV.FormattingUtils;
using Microsoft.AspNetCore.Mvc;
using System;
using DCEMV.TLVProtocol;

namespace DCEMV.DemoServer.Controllers.Api
{
    public class ControllerBase : Controller
    {
        protected static EMVDESSecurity jcesecmod;
        protected static string mkACEncrypted = "0D39A43C864D1B40F33998B80BB02C95";
        protected static string mkACEncryptedCV = "000000";//"6FB1C8";
        protected static string lmkFilePath = @"secret.lmk";

        public ControllerBase()
        {
            TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();
        }
    }
}
