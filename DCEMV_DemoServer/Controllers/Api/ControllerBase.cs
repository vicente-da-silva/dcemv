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
        private static EMVDESSecurity jcesecmod;
        private string mkACEncrypted = "0D39A43C864D1B40F33998B80BB02C95";
        private string mkACEncryptedCV = "000000";//"6FB1C8";

        private string lmkFilePath = @"secret.lmk";

        public ControllerBase()
        {
            TLVMetaDataSourceSingleton.Instance.DataSource = new EMVTLVMetaDataSource();
        }

        protected bool VerifyCryptogram17(string emvData)
        {
            if (jcesecmod==null)
                jcesecmod = new EMVDESSecurity(lmkFilePath);

            TLV tlv = TLVasJSON.FromJSON(emvData);
            TLV _9F26 = tlv.Children.Get(EMVTagsEnum.APPLICATION_CRYPTOGRAM_9F26_KRN.Tag);
            if (_9F26 == null)
                throw new ValidationException("No Cryptogram found");

            TLV _9F02 = tlv.Children.Get(EMVTagsEnum.AMOUNT_AUTHORISED_NUMERIC_9F02_KRN.Tag);
            if (_9F02 == null)
                throw new ValidationException("No Amount found");

            TLV _9F37 = tlv.Children.Get(EMVTagsEnum.UNPREDICTABLE_NUMBER_9F37_KRN.Tag);
            if (_9F37 == null)
                throw new ValidationException("No Unpredictable Number found");

            TLV _9F36 = tlv.Children.Get(EMVTagsEnum.APPLICATION_TRANSACTION_COUNTER_ATC_9F36_KRN.Tag);
            if (_9F36 == null)
                throw new ValidationException("No ATC found");

            TLV _9F10 = tlv.Children.Get(EMVTagsEnum.ISSUER_APPLICATION_DATA_9F10_KRN.Tag);
            if (_9F10 == null)
                throw new ValidationException("No IAD found");

            TLV _5F34 = tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_SEQUENCE_NUMBER_5F34_KRN.Tag);
            if (_5F34 == null)
                throw new ValidationException("No PSN found");

            byte[] panBCD;
            TLV _5A = tlv.Children.Get(EMVTagsEnum.APPLICATION_PRIMARY_ACCOUNT_NUMBER_PAN_5A_KRN.Tag);
            if (_5A != null)
                panBCD = _5A.Value;
            else
            {
                TLV _57 = tlv.Children.Get(EMVTagsEnum.TRACK_2_EQUIVALENT_DATA_57_KRN.Tag);
                if (_57 == null)
                    throw new ValidationException("No PAN found");
                String panString = Formatting.ByteArrayToHexString(_57.Value);
                panBCD = Formatting.StringToBcd(panString.Split('D')[0], false);
            }

            SecureDESKey imkac = new SecureDESKey(SMAdapter.LENGTH_DES3_2KEY, SMAdapter.TYPE_MK_AC + ":1U", mkACEncrypted, mkACEncryptedCV);
            byte[] data = EMVDESSecurity.PaddingISO9797Method2(Formatting.ConcatArrays(_9F02.Value, _9F37.Value, _9F36.Value, new byte[] { _9F10.Value[4] }));
            return jcesecmod.VerifyARQCImpl(MKDMethod.OPTION_A, SKDMethod.VSDC, imkac,
                Formatting.BcdToString(panBCD),
                Formatting.ByteArrayToHexString(_5F34.Value),
                _9F26.Value, null, null, data);
        }
    }
}
