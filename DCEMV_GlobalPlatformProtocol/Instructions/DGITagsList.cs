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
using System.Collections.Generic;

namespace DCEMV.GlobalPlatformProtocol
{
    public class DGITagsList
    {
        private static List<DGIMeta> EnumList = new List<DGIMeta>();
        
        public static DGIMeta DGI_8010 = new DGIMeta("8010", "PIN", false);
        public static DGIMeta DGI_9010 = new DGIMeta("9010", "PIN Counters", false);
        public static DGIMeta DGI_8000 = new DGIMeta("8000", "3DES Key Contact", false);
        public static DGIMeta DGI_9000 = new DGIMeta("9000", "3DES Key KCV Contact", false);
        public static DGIMeta DGI_8001 = new DGIMeta("8001", "3DES Key Contactless", false);
        public static DGIMeta DGI_9001 = new DGIMeta("9001", "3DES Key KCV Contactless", false);


        public static DGIMeta DGI_8201 = new DGIMeta("8201", "(Q-1)modP", false);
        public static DGIMeta DGI_8202 = new DGIMeta("8202", "Dmod(Q-1)", false);
        public static DGIMeta DGI_8203 = new DGIMeta("8203", "Dmod(P-1)", false);
        public static DGIMeta DGI_8204 = new DGIMeta("8204", "Prime Q", false);
        public static DGIMeta DGI_8205 = new DGIMeta("8205", "Prime P", false);


        public static DGIMeta DGI_D021 = new DGIMeta("D021", "PDE no1  related info", false);
        public static DGIMeta DGI_D022 = new DGIMeta("D022", "PDE no2  related info", false);
        public static DGIMeta DGI_D023 = new DGIMeta("D023", "PDE no3  related info", false);
        public static DGIMeta DGI_D024 = new DGIMeta("D024", "PDE no4  related info", false);
        public static DGIMeta DGI_D025 = new DGIMeta("D025", "PDE no5  related info", false);
        public static DGIMeta DGI_D026 = new DGIMeta("D026", "PDE no6  related info", false);
        public static DGIMeta DGI_D027 = new DGIMeta("D027", "PDE no7  related info", false);
        public static DGIMeta DGI_D028 = new DGIMeta("D028", "PDE no8  related info", false);
        public static DGIMeta DGI_D029 = new DGIMeta("D029", "PDE no9  related info", false);
        public static DGIMeta DGI_D02A = new DGIMeta("D02A", "PDE no10 related info", false);

        public static DGIMeta DGI_D002 = new DGIMeta("D002", "Card Internal Data Elements", true);
        public static DGIMeta DGI_D003 = new DGIMeta("D003", "Security limits", true);
        public static DGIMeta DGI_D005 = new DGIMeta("D005", "Profile Selection Table", true);
        public static DGIMeta DGI_D007 = new DGIMeta("D007", "PDOL Length", true);
        public static DGIMeta DGI_9102 = new DGIMeta("9102", "SELECT Command Response(FCI) Data for contact", true);
        public static DGIMeta DGI_9103 = new DGIMeta("9103", "SELECT Command Response(FCI) Data for contactless", true);

        public static DGIMeta DGI_D010 = new DGIMeta("D010", "PRO associated to profile 0", true);
        public static DGIMeta DGI_D011 = new DGIMeta("D011", "PRO associated to profile 1", true);
        public static DGIMeta DGI_D012 = new DGIMeta("D012", "PRO associated to profile 2", true);
        public static DGIMeta DGI_D013 = new DGIMeta("D013", "PRO associated to profile 3", true);
        public static DGIMeta DGI_D014 = new DGIMeta("D014", "PRO associated to profile 4", true);
        public static DGIMeta DGI_D015 = new DGIMeta("D015", "PRO associated to profile 5", true);
        public static DGIMeta DGI_D016 = new DGIMeta("D016", "PRO associated to profile 6", true);
        public static DGIMeta DGI_D017 = new DGIMeta("D017", "PRO associated to profile 7", true);
        public static DGIMeta DGI_D018 = new DGIMeta("D018", "PRO associated to profile 8", true);
        public static DGIMeta DGI_D019 = new DGIMeta("D019", "PRO associated to profile 9", true);
        public static DGIMeta DGI_D01A = new DGIMeta("D01A", "PRO associated to profile A", true);
        public static DGIMeta DGI_D01B = new DGIMeta("D01B", "PRO associated to profile B", true);
        public static DGIMeta DGI_D01C = new DGIMeta("D01C", "PRO associated to profile C", true);
        public static DGIMeta DGI_D01D = new DGIMeta("D01D", "PRO associated to profile D", true);
        public static DGIMeta DGI_D01E = new DGIMeta("D01E", "PRO associated to profile E", true);
        public static DGIMeta DGI_D01F = new DGIMeta("D01F", "PRO associated to profile F", true);

        public static DGIMeta DGI_SFI_REC = new DGIMeta("xxxx", "DGI by SFI and REC", true);
        public static DGIMeta DGI_UNKNOWN = new DGIMeta("FFFF", "Unknown", false);

        static DGITagsList()
        {
            EnumList.Add(DGI_8010);
            EnumList.Add(DGI_9010);
            EnumList.Add(DGI_8000);
            EnumList.Add(DGI_9000);
            EnumList.Add(DGI_8001);
            EnumList.Add(DGI_9001);

            EnumList.Add(DGI_8201);
            EnumList.Add(DGI_8202);
            EnumList.Add(DGI_8203);
            EnumList.Add(DGI_8204);
            EnumList.Add(DGI_8205);

            EnumList.Add(DGI_D021);
            EnumList.Add(DGI_D022);
            EnumList.Add(DGI_D023);
            EnumList.Add(DGI_D024);
            EnumList.Add(DGI_D025);
            EnumList.Add(DGI_D026);
            EnumList.Add(DGI_D027);
            EnumList.Add(DGI_D028);
            EnumList.Add(DGI_D029);
            EnumList.Add(DGI_D02A);

            EnumList.Add(DGI_D002);
            EnumList.Add(DGI_D003);
            EnumList.Add(DGI_D005);
            EnumList.Add(DGI_D007);
            EnumList.Add(DGI_9102);
            EnumList.Add(DGI_9103);

            EnumList.Add(DGI_D010);
            EnumList.Add(DGI_D011);
            EnumList.Add(DGI_D012);
            EnumList.Add(DGI_D013);
            EnumList.Add(DGI_D014);
            EnumList.Add(DGI_D015);
            EnumList.Add(DGI_D016);
            EnumList.Add(DGI_D017);
            EnumList.Add(DGI_D018);
            EnumList.Add(DGI_D019);
            EnumList.Add(DGI_D01A);
            EnumList.Add(DGI_D01B);
            EnumList.Add(DGI_D01C);
            EnumList.Add(DGI_D01D);
            EnumList.Add(DGI_D01E);
            EnumList.Add(DGI_D01F);

            EnumList.Add(DGI_SFI_REC);
            EnumList.Add(DGI_UNKNOWN);
        }

        public static DGIMeta GetMeta(string tag, byte template)
        {
            DGIMeta meta = EnumList.Find(x => x.Tag == tag);
            if (meta == null)
            {
                if(template == 0x70)
                {
                    return DGI_SFI_REC;
                }
                else
                    return DGI_UNKNOWN;
            }
            return meta;
        }
    }

    public class DGIMeta
    {
        public DGIMeta(string tag, string description, bool isTLVFormatted)
        {
            Tag = tag;
            Description = description;
            IsTLVFormatted = isTLVFormatted;
        }

        public string Tag { get; set; }
        public string Description { get; set; }
        public bool IsTLVFormatted { get; set; }
    }
}
