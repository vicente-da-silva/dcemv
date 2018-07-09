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
using System.Text;

namespace DCEMV.CardReaders.NCIDriver
{
    public class ConnCreditEntry
    {
        public byte ConnID { get; set; }
        public byte NoCredits { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("ConnID: " + ConnID);
            sb.AppendLine("NoCredits: " + NoCredits);
            return sb.ToString();
        }
    }
    public class CoreConnCreditsNotification : CoreNotification
    {
        public ConnCreditEntry[] CreditEntries;

        public CoreConnCreditsNotification() { CreditEntries = new ConnCreditEntry[0]; }

        public CoreConnCreditsNotification(PacketBoundryFlagEnum pbf) : base(pbf, OpcodeCoreIdentifierEnum.CORE_CONN_CREDITS_NTF)
        {
            CreditEntries = new ConnCreditEntry[0];
        }

        public override void deserialize(byte[] packet)
        {
            base.deserialize(packet);

            byte noEntries = payLoad[0];
            CreditEntries = new ConnCreditEntry[noEntries];
            byte pos = 1;
            for (int i = 0; i < noEntries; i++)
            {
                ConnCreditEntry cce = new ConnCreditEntry() { ConnID = payLoad[pos], NoCredits = payLoad[pos+1] };
                pos = (byte)(pos + 2);
                CreditEntries[i] = cce;
            }
        }

        public override byte[] serialize()
        {
            payLoad = new byte[(CreditEntries.Length * 2) + 1];
            payLoad[0] = (byte)CreditEntries.Length;
            byte pos = 1;
            for (int i = 0; i < CreditEntries.Length; i++)
            {
                payLoad[pos] = CreditEntries[i].ConnID;
                payLoad[pos+1] = CreditEntries[i].NoCredits;
                pos = (byte)(pos + 2);
            }
            return base.serialize();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            sb.Append(base.ToString());
            for (int i = 0; i < CreditEntries.Length; i++)
                sb.Append(CreditEntries[i].ToString());
            sb.AppendLine("--------------------------------------------------------------------------------------------------------");
            return sb.ToString();
        }
    }
}
