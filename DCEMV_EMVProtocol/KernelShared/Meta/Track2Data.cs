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
using System.Globalization;

namespace DCEMV.EMVProtocol.Kernels
{
  
    public class Track2Data
    {
        private const char fieldSeparator = '=';
        private const char fieldSeparatorMag = 'D';
        private const char postfixMag = 'F';

        public string PAN { get; protected set; }
        public DateTime ExpiryDate { get; protected set; }
        public string ServiceCode { get; protected set; }
        public string DiscretionaryData { get; protected set; }

        public string Track2 { get; protected set; }

        public Track2Data(string track2, bool isMagStripFormat)
        {
            this.Track2 = track2;

            if (isMagStripFormat)
                TransformToMagStripeFormat();

            ExtractData();
        }

        private void TransformToMagStripeFormat()
        {
            Track2 = Track2.Replace(fieldSeparatorMag, fieldSeparator).Replace("F", "");
        }
        private void ExtractData()
        {
            string[] track2Split = Track2.Split(fieldSeparator);

            if (track2Split.Length != 2)
                throw new EMVProtocolException("Cannot extract PAN from Track 2");

            PAN = track2Split[0];

            ExtractAdditionalData(track2Split[1]);
        }
        private void ExtractAdditionalData(string additionalDataStr)
        {
            int index = 0;

            string ed = additionalDataStr.Substring(index, 4);
            try
            {
                
                ExpiryDate = DateTime.ParseExact(ed, "yyMM", CultureInfo.InvariantCulture);
            }
            catch (FormatException e)
            {
                throw new EMVProtocolException("Unable to extract expiry date from track2: " + e.Message);
            }
            index += 4;

            // if there is a field separator here, the service code is missing
            if (fieldSeparator == additionalDataStr[index])
            {
                ServiceCode = null;
                index++;
            }
            else
            {
                string sc = additionalDataStr.Substring(index, 3);
                index += 3;
                ServiceCode = sc;
            }
            string pvv = additionalDataStr.Substring(index, 5);
            index += 5;

            DiscretionaryData = additionalDataStr.Substring(index);
        }
    }
}
