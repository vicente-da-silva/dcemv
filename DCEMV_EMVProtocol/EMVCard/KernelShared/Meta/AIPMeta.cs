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
using DCEMV.FormattingUtils;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Kernels
{
    public class AIPMeta 
    {
        private static BitDescription[] FirstByteDescriptions = {
                                            new BitDescription("CDA supported","CDA is not supported"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("Issuer authentication is supported","Issuer authentication is not supported"),
                                            new BitDescription("Terminal risk management is to be performed","Terminal risk management is not to be performed"),
                                            new BitDescription("Cardholder verification is supported","Cardholder verification is not supported"),
                                            new BitDescription("DDA supported","DDA is not supported"),
                                            new BitDescription("SDA supported","DDA is not supported"),
                                            new BitDescription("RFU","RFU"),
                                           };

        private static BitDescription[] SecondByteDescriptionsMC = {
                                            new BitDescription("M/Chip Profile is supported", "Only MagStrip profile is supported"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                        };

        private static BitDescription[] SecondByteDescriptionsVisa = {
                                            new BitDescription("MSD is supported", "MSD is not supported"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("fDDA supported", "fDDA is not supported"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                            new BitDescription("RFU","RFU"),
                                        };

        private static List<string> GetInterpretation(byte b, BitDescription[] descriptions)
        {
            List<string> result = new List<string>();
            for (int i = 0; i < 8; i++)
                if (Formatting.IsBitSet(b, i))
                    result.Add(descriptions[i].DescriptionForSet);
                else
                    result.Add(descriptions[i].DescriptionForNotSet);

            return result;
        }

        public static List<string> GetInterpretationFistByte(byte b)
        {
            return GetInterpretation(b, FirstByteDescriptions);
        }

        public static List<string> GetInterpretationSecondByteMC(byte b)
        {
            return GetInterpretation(b, SecondByteDescriptionsMC);
        }

        public static List<string> GetInterpretationSecondByteVisa(byte b)
        {
            return GetInterpretation(b, SecondByteDescriptionsVisa);
        }
    }
}
