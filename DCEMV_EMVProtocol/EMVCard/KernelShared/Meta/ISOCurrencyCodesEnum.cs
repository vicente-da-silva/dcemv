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
using DCEMV.Shared;
using System.Collections.Generic;

namespace DCEMV.EMVProtocol.Kernels
{

    public static class ISOCurrencyCodesEnum
    {
        public static List<EnumBase> EnumList = new List<EnumBase>();

        public static EMVCurrencyCode AED = new EMVCurrencyCode(784, "United Arab Emirates dirham", 2);
        public static EMVCurrencyCode AFN = new EMVCurrencyCode(971, "Afghan afghani", 2);
        public static EMVCurrencyCode ALL = new EMVCurrencyCode(8, "Albanian lek", 2);
        public static EMVCurrencyCode AMD = new EMVCurrencyCode(51, "Armenian dram", 2);
        public static EMVCurrencyCode ANG = new EMVCurrencyCode(532, "Netherlands Antillean guilder", 2);
        public static EMVCurrencyCode AOA = new EMVCurrencyCode(973, "Angolan kwanza", 2);
        public static EMVCurrencyCode ARS = new EMVCurrencyCode(32, "Argentine peso", 2);
        public static EMVCurrencyCode AUD = new EMVCurrencyCode(36, "Australian dollar", 2);
        public static EMVCurrencyCode AWG = new EMVCurrencyCode(533, "Aruban florin", 2);
        public static EMVCurrencyCode AZN = new EMVCurrencyCode(944, "Azerbaijani manat", 2);
        public static EMVCurrencyCode BAM = new EMVCurrencyCode(977, "Bosnia and Herzegovina convertible mark", 2);
        public static EMVCurrencyCode BBD = new EMVCurrencyCode(52, "Barbados dollar", 2);
        public static EMVCurrencyCode BDT = new EMVCurrencyCode(50, "Bangladeshi taka", 2);
        public static EMVCurrencyCode BGN = new EMVCurrencyCode(975, "Bulgarian lev", 2);
        public static EMVCurrencyCode BHD = new EMVCurrencyCode(48, "Bahraini dinar", 3);
        public static EMVCurrencyCode BIF = new EMVCurrencyCode(108, "Burundian franc", 0);
        public static EMVCurrencyCode BMD = new EMVCurrencyCode(60, "Bermudian dollar", 2);
        public static EMVCurrencyCode BND = new EMVCurrencyCode(96, "Brunei dollar", 2);
        public static EMVCurrencyCode BOB = new EMVCurrencyCode(68, "Boliviano", 2);
        public static EMVCurrencyCode BOV = new EMVCurrencyCode(984, "Bolivian Mvdol (funds code)", 2);
        public static EMVCurrencyCode BRL = new EMVCurrencyCode(986, "Brazilian real", 2);
        public static EMVCurrencyCode BSD = new EMVCurrencyCode(44, "Bahamian dollar", 2);
        public static EMVCurrencyCode BTN = new EMVCurrencyCode(64, "Bhutanese ngultrum", 2);
        public static EMVCurrencyCode BWP = new EMVCurrencyCode(72, "Botswana pula", 2);
        public static EMVCurrencyCode BYR = new EMVCurrencyCode(974, "Belarusian ruble", 0);
        public static EMVCurrencyCode BZD = new EMVCurrencyCode(84, "Belize dollar", 2);
        public static EMVCurrencyCode CAD = new EMVCurrencyCode(124, "Canadian dollar", 2);
        public static EMVCurrencyCode CDF = new EMVCurrencyCode(976, "Congolese franc", 2);
        public static EMVCurrencyCode CHE = new EMVCurrencyCode(947, "WIR Euro (complementary currency)", 2);
        public static EMVCurrencyCode CHF = new EMVCurrencyCode(756, "Swiss franc", 2);
        public static EMVCurrencyCode CHW = new EMVCurrencyCode(948, "WIR Franc (complementary currency)", 2);
        public static EMVCurrencyCode CLF = new EMVCurrencyCode(990, "Unidad de Fomento (funds code)", 0);
        public static EMVCurrencyCode CLP = new EMVCurrencyCode(152, "Chilean peso", 0);
        public static EMVCurrencyCode CNY = new EMVCurrencyCode(156, "Chinese yuan", 2);
        public static EMVCurrencyCode COP = new EMVCurrencyCode(170, "Colombian peso", 2);
        public static EMVCurrencyCode COU = new EMVCurrencyCode(970, "Unidad de Valor Real(UVR) (funds code)[7]", 4);
        public static EMVCurrencyCode CRC = new EMVCurrencyCode(188, "Costa Rican colon", 2);
        public static EMVCurrencyCode CUC = new EMVCurrencyCode(931, "Cuban convertible peso", 2);
        public static EMVCurrencyCode CUP = new EMVCurrencyCode(192, "Cuban peso", 2);
        public static EMVCurrencyCode CVE = new EMVCurrencyCode(132, "Cape Verde escudo", 0);
        public static EMVCurrencyCode CZK = new EMVCurrencyCode(203, "Czech koruna", 2);
        public static EMVCurrencyCode DJF = new EMVCurrencyCode(262, "Djiboutian franc", 0);
        public static EMVCurrencyCode DKK = new EMVCurrencyCode(208, "Danish krone", 2);
        public static EMVCurrencyCode DOP = new EMVCurrencyCode(214, "Dominican peso", 2);
        public static EMVCurrencyCode DZD = new EMVCurrencyCode(12, "Algerian dinar", 2);
        public static EMVCurrencyCode EGP = new EMVCurrencyCode(818, "Egyptian pound", 2);
        public static EMVCurrencyCode ERN = new EMVCurrencyCode(232, "Eritrean nakfa", 2);
        public static EMVCurrencyCode ETB = new EMVCurrencyCode(230, "Ethiopian birr", 2);
        public static EMVCurrencyCode EUR = new EMVCurrencyCode(978, "Euro", 2);
        public static EMVCurrencyCode FJD = new EMVCurrencyCode(242, "Fiji dollar", 2);
        public static EMVCurrencyCode FKP = new EMVCurrencyCode(238, "Falkland Islands pound", 2);
        public static EMVCurrencyCode GBP = new EMVCurrencyCode(826, "Pound sterling", 2);
        public static EMVCurrencyCode GEL = new EMVCurrencyCode(981, "Georgian lari", 2);
        public static EMVCurrencyCode GHS = new EMVCurrencyCode(936, "Ghanaian cedi", 2);
        public static EMVCurrencyCode GIP = new EMVCurrencyCode(292, "Gibraltar pound", 2);
        public static EMVCurrencyCode GMD = new EMVCurrencyCode(270, "Gambian dalasi", 2);
        public static EMVCurrencyCode GNF = new EMVCurrencyCode(324, "Guinean franc", 0);
        public static EMVCurrencyCode GTQ = new EMVCurrencyCode(320, "Guatemalan quetzal", 2);
        public static EMVCurrencyCode GYD = new EMVCurrencyCode(328, "Guyanese dollar", 2);
        public static EMVCurrencyCode HKD = new EMVCurrencyCode(344, "Hong Kong dollar", 2);
        public static EMVCurrencyCode HNL = new EMVCurrencyCode(340, "Honduran lempira", 2);
        public static EMVCurrencyCode HRK = new EMVCurrencyCode(191, "Croatian kuna", 2);
        public static EMVCurrencyCode HTG = new EMVCurrencyCode(332, "Haitian gourde", 2);
        public static EMVCurrencyCode HUF = new EMVCurrencyCode(348, "Hungarian forint", 2);
        public static EMVCurrencyCode IDR = new EMVCurrencyCode(360, "Indonesian rupiah", 2);
        public static EMVCurrencyCode ILS = new EMVCurrencyCode(376, "Israeli new shekel", 2);
        public static EMVCurrencyCode INR = new EMVCurrencyCode(356, "Indian rupee", 2);
        public static EMVCurrencyCode IQD = new EMVCurrencyCode(368, "Iraqi dinar", 3);
        public static EMVCurrencyCode IRR = new EMVCurrencyCode(364, "Iranian rial", 2);
        public static EMVCurrencyCode ISK = new EMVCurrencyCode(352, "Icelandic króna", 0);
        public static EMVCurrencyCode JMD = new EMVCurrencyCode(388, "Jamaican dollar", 2);
        public static EMVCurrencyCode JOD = new EMVCurrencyCode(400, "Jordanian dinar", 3);
        public static EMVCurrencyCode JPY = new EMVCurrencyCode(392, "Japanese yen", 0);
        public static EMVCurrencyCode KES = new EMVCurrencyCode(404, "Kenyan shilling", 2);
        public static EMVCurrencyCode KGS = new EMVCurrencyCode(417, "Kyrgyzstani som", 2);
        public static EMVCurrencyCode KHR = new EMVCurrencyCode(116, "Cambodian riel", 2);
        public static EMVCurrencyCode KMF = new EMVCurrencyCode(174, "Comoro franc", 0);
        public static EMVCurrencyCode KPW = new EMVCurrencyCode(408, "North Korean won", 2);
        public static EMVCurrencyCode KRW = new EMVCurrencyCode(410, "South Korean won", 0);
        public static EMVCurrencyCode KWD = new EMVCurrencyCode(414, "Kuwaiti dinar", 3);
        public static EMVCurrencyCode KYD = new EMVCurrencyCode(136, "Cayman Islands dollar", 2);
        public static EMVCurrencyCode KZT = new EMVCurrencyCode(398, "Kazakhstani tenge", 2);
        public static EMVCurrencyCode LAK = new EMVCurrencyCode(418, "Lao kip", 2);
        public static EMVCurrencyCode LBP = new EMVCurrencyCode(422, "Lebanese pound", 2);
        public static EMVCurrencyCode LKR = new EMVCurrencyCode(144, "Sri Lankan rupee", 2);
        public static EMVCurrencyCode LRD = new EMVCurrencyCode(430, "Liberian dollar", 2);
        public static EMVCurrencyCode LSL = new EMVCurrencyCode(426, "Lesotho loti", 2);
        public static EMVCurrencyCode LTL = new EMVCurrencyCode(440, "Lithuanian litas", 2);
        public static EMVCurrencyCode LYD = new EMVCurrencyCode(434, "Libyan dinar", 3);
        public static EMVCurrencyCode MAD = new EMVCurrencyCode(504, "Moroccan dirham", 2);
        public static EMVCurrencyCode MDL = new EMVCurrencyCode(498, "Moldovan leu", 2);
        public static EMVCurrencyCode MGA = new EMVCurrencyCode(969, "Malagasy ariary", 2);
        public static EMVCurrencyCode MKD = new EMVCurrencyCode(807, "Macedonian denar", 2);
        public static EMVCurrencyCode MMK = new EMVCurrencyCode(104, "Myanma kyat", 2);
        public static EMVCurrencyCode MNT = new EMVCurrencyCode(496, "Mongolian tugrik", 2);
        public static EMVCurrencyCode MOP = new EMVCurrencyCode(446, "Macanese pataca", 2);
        public static EMVCurrencyCode MRO = new EMVCurrencyCode(478, "Mauritanian ouguiya", 2);
        public static EMVCurrencyCode MUR = new EMVCurrencyCode(480, "Mauritian rupee", 2);
        public static EMVCurrencyCode MVR = new EMVCurrencyCode(462, "Maldivian rufiyaa", 2);
        public static EMVCurrencyCode MWK = new EMVCurrencyCode(454, "Malawian kwacha", 2);
        public static EMVCurrencyCode MXN = new EMVCurrencyCode(484, "Mexican peso", 2);
        public static EMVCurrencyCode MXV = new EMVCurrencyCode(979, "Mexican Unidad de Inversion (UDI) (funds code)", 2);
        public static EMVCurrencyCode MYR = new EMVCurrencyCode(458, "Malaysian ringgit", 2);
        public static EMVCurrencyCode MZN = new EMVCurrencyCode(943, "Mozambican metical", 2);
        public static EMVCurrencyCode NAD = new EMVCurrencyCode(516, "Namibian dollar", 2);
        public static EMVCurrencyCode NGN = new EMVCurrencyCode(566, "Nigerian naira", 2);
        public static EMVCurrencyCode NIO = new EMVCurrencyCode(558, "Nicaraguan córdoba", 2);
        public static EMVCurrencyCode NOK = new EMVCurrencyCode(578, "Norwegian krone", 2);
        public static EMVCurrencyCode NPR = new EMVCurrencyCode(524, "Nepalese rupee", 2);
        public static EMVCurrencyCode NZD = new EMVCurrencyCode(554, "New Zealand dollar", 2);
        public static EMVCurrencyCode OMR = new EMVCurrencyCode(512, "Omani rial", 3);
        public static EMVCurrencyCode PAB = new EMVCurrencyCode(590, "Panamanian balboa", 2);
        public static EMVCurrencyCode PEN = new EMVCurrencyCode(604, "Peruvian nuevo sol", 2);
        public static EMVCurrencyCode PGK = new EMVCurrencyCode(598, "Papua New Guinean kina", 2);
        public static EMVCurrencyCode PHP = new EMVCurrencyCode(608, "Philippine peso", 2);
        public static EMVCurrencyCode PKR = new EMVCurrencyCode(586, "Pakistani rupee", 2);
        public static EMVCurrencyCode PLN = new EMVCurrencyCode(985, "Polish złoty", 2);
        public static EMVCurrencyCode PYG = new EMVCurrencyCode(600, "Paraguayan guaraní", 0);
        public static EMVCurrencyCode QAR = new EMVCurrencyCode(634, "Qatari riyal", 2);
        public static EMVCurrencyCode RON = new EMVCurrencyCode(946, "Romanian new leu", 2);
        public static EMVCurrencyCode RSD = new EMVCurrencyCode(941, "Serbian dinar", 2);
        public static EMVCurrencyCode RUB = new EMVCurrencyCode(643, "Russian ruble", 2);
        public static EMVCurrencyCode RWF = new EMVCurrencyCode(646, "Rwandan franc", 0);
        public static EMVCurrencyCode SAR = new EMVCurrencyCode(682, "Saudi riyal", 2);
        public static EMVCurrencyCode SBD = new EMVCurrencyCode(90, "Solomon Islands dollar", 2);
        public static EMVCurrencyCode SCR = new EMVCurrencyCode(690, "Seychelles rupee", 2);
        public static EMVCurrencyCode SDG = new EMVCurrencyCode(938, "Sudanese pound", 2);
        public static EMVCurrencyCode SEK = new EMVCurrencyCode(752, "Swedish krona/kronor", 2);
        public static EMVCurrencyCode SGD = new EMVCurrencyCode(702, "Singapore dollar", 2);
        public static EMVCurrencyCode SHP = new EMVCurrencyCode(654, "Saint Helena pound", 2);
        public static EMVCurrencyCode SLL = new EMVCurrencyCode(694, "Sierra Leonean leone", 2);
        public static EMVCurrencyCode SOS = new EMVCurrencyCode(706, "Somali shilling", 2);
        public static EMVCurrencyCode SRD = new EMVCurrencyCode(968, "Surinamese dollar", 2);
        public static EMVCurrencyCode SSP = new EMVCurrencyCode(728, "South Sudanese pound", 2);
        public static EMVCurrencyCode STD = new EMVCurrencyCode(678, "São Tomé and Príncipe dobra", 2);
        public static EMVCurrencyCode SYP = new EMVCurrencyCode(760, "Syrian pound", 2);
        public static EMVCurrencyCode SZL = new EMVCurrencyCode(748, "Swazi lilangeni", 2);
        public static EMVCurrencyCode THB = new EMVCurrencyCode(764, "Thai baht", 2);
        public static EMVCurrencyCode TJS = new EMVCurrencyCode(972, "Tajikistani somoni", 2);
        public static EMVCurrencyCode TMT = new EMVCurrencyCode(934, "Turkmenistani manat", 2);
        public static EMVCurrencyCode TND = new EMVCurrencyCode(788, "Tunisian dinar", 3);
        public static EMVCurrencyCode TOP = new EMVCurrencyCode(776, "Tongan paʻanga", 2);
        public static EMVCurrencyCode TRY = new EMVCurrencyCode(949, "Turkish lira", 2);
        public static EMVCurrencyCode TTD = new EMVCurrencyCode(780, "Trinidad and Tobago dollar", 2);
        public static EMVCurrencyCode TWD = new EMVCurrencyCode(901, "New Taiwan dollar", 2);
        public static EMVCurrencyCode TZS = new EMVCurrencyCode(834, "Tanzanian shilling", 2);
        public static EMVCurrencyCode UAH = new EMVCurrencyCode(980, "Ukrainian hryvnia", 2);
        public static EMVCurrencyCode UGX = new EMVCurrencyCode(800, "Ugandan shilling", 0);
        public static EMVCurrencyCode USD = new EMVCurrencyCode(840, "United States dollar", 2);
        public static EMVCurrencyCode USN = new EMVCurrencyCode(997, "United States dollar (next day) (funds code)", 2);
        public static EMVCurrencyCode USS = new EMVCurrencyCode(998, "United States dollar (same day) (funds code)[10]", 2);
        public static EMVCurrencyCode UYI = new EMVCurrencyCode(940, "Uruguay Peso en Unidades Indexadas (URUIURUI) (funds code)", 0);
        public static EMVCurrencyCode UYU = new EMVCurrencyCode(858, "Uruguayan peso", 2);
        public static EMVCurrencyCode UZS = new EMVCurrencyCode(860, "Uzbekistan som", 2);
        public static EMVCurrencyCode VEF = new EMVCurrencyCode(937, "Venezuelan bolívar", 2);
        public static EMVCurrencyCode VND = new EMVCurrencyCode(704, "Vietnamese dong", 0);
        public static EMVCurrencyCode VUV = new EMVCurrencyCode(548, "Vanuatu vatu", 0);
        public static EMVCurrencyCode WST = new EMVCurrencyCode(882, "Samoan tala", 2);
        public static EMVCurrencyCode XAF = new EMVCurrencyCode(950, "CFA franc BEAC", 0);
        public static EMVCurrencyCode XAG = new EMVCurrencyCode(961, "Silver (one troy ounce)", 0);
        public static EMVCurrencyCode XAU = new EMVCurrencyCode(959, "Gold (one troy ounce)", 0);
        public static EMVCurrencyCode XBA = new EMVCurrencyCode(955, "European Composite Unit(EURCO) (bond market unit)", 0);
        public static EMVCurrencyCode XBB = new EMVCurrencyCode(956, "European Monetary Unit(E.M.U.-6) (bond market unit)", 0);
        public static EMVCurrencyCode XBC = new EMVCurrencyCode(957, "European Unit of Account 9(E.U.A.-9) (bond market unit)", 0);
        public static EMVCurrencyCode XBD = new EMVCurrencyCode(958, "European Unit of Account 17(E.U.A.-17) (bond market unit)", 0);
        public static EMVCurrencyCode XCD = new EMVCurrencyCode(951, "East Caribbean dollar", 2);
        public static EMVCurrencyCode XDR = new EMVCurrencyCode(960, "Special drawing rights", 0);
        //public static EMVCurrencyCode XFU = new EMVCurrencyCode(Nil, "UIC franc(special settlement currency)", 0);
        public static EMVCurrencyCode XOF = new EMVCurrencyCode(952, "CFA franc BCEAO", 0);
        public static EMVCurrencyCode XPD = new EMVCurrencyCode(964, "Palladium (onetroy ounce)", 0);
        public static EMVCurrencyCode XPF = new EMVCurrencyCode(953, "CFP franc (franc Pacifique)", 0);
        public static EMVCurrencyCode XPT = new EMVCurrencyCode(962, "Platinum (onetroy ounce)", 0);
        public static EMVCurrencyCode XTS = new EMVCurrencyCode(963, "Code reserved for testing purposes", 0);
        public static EMVCurrencyCode XXX = new EMVCurrencyCode(999, "No currency", 0);
        public static EMVCurrencyCode YER = new EMVCurrencyCode(886, "Yemeni rial", 2);
        public static EMVCurrencyCode ZAR = new EMVCurrencyCode(710, "South African rand", 2);
        public static EMVCurrencyCode ZMW = new EMVCurrencyCode(967, "Zambian kwacha", 2);
        public static EMVCurrencyCode ZWL = new EMVCurrencyCode(932, "Zimbabwe dollar", 2);


        static ISOCurrencyCodesEnum()
        {

            EnumList.Add(AED);
            EnumList.Add(AFN);
            EnumList.Add(ALL);
            EnumList.Add(AMD);
            EnumList.Add(ANG);
            EnumList.Add(AOA);
            EnumList.Add(ARS);
            EnumList.Add(AUD);
            EnumList.Add(AWG);
            EnumList.Add(AZN);
            EnumList.Add(BAM);
            EnumList.Add(BBD);
            EnumList.Add(BDT);
            EnumList.Add(BGN);
            EnumList.Add(BHD);
            EnumList.Add(BIF);
            EnumList.Add(BMD);
            EnumList.Add(BND);
            EnumList.Add(BOB);
            EnumList.Add(BOV);
            EnumList.Add(BRL);
            EnumList.Add(BSD);
            EnumList.Add(BTN);
            EnumList.Add(BWP);
            EnumList.Add(BYR);
            EnumList.Add(BZD);
            EnumList.Add(CAD);
            EnumList.Add(CDF);
            EnumList.Add(CHE);
            EnumList.Add(CHF);
            EnumList.Add(CHW);
            EnumList.Add(CLF);
            EnumList.Add(CLP);
            EnumList.Add(CNY);
            EnumList.Add(COP);
            EnumList.Add(COU);
            EnumList.Add(CRC);
            EnumList.Add(CUC);
            EnumList.Add(CUP);
            EnumList.Add(CVE);
            EnumList.Add(CZK);
            EnumList.Add(DJF);
            EnumList.Add(DKK);
            EnumList.Add(DOP);
            EnumList.Add(DZD);
            EnumList.Add(EGP);
            EnumList.Add(ERN);
            EnumList.Add(ETB);
            EnumList.Add(EUR);
            EnumList.Add(FJD);
            EnumList.Add(FKP);
            EnumList.Add(GBP);
            EnumList.Add(GEL);
            EnumList.Add(GHS);
            EnumList.Add(GIP);
            EnumList.Add(GMD);
            EnumList.Add(GNF);
            EnumList.Add(GTQ);
            EnumList.Add(GYD);
            EnumList.Add(HKD);
            EnumList.Add(HNL);
            EnumList.Add(HRK);
            EnumList.Add(HTG);
            EnumList.Add(HUF);
            EnumList.Add(IDR);
            EnumList.Add(ILS);
            EnumList.Add(INR);
            EnumList.Add(IQD);
            EnumList.Add(IRR);
            EnumList.Add(ISK);
            EnumList.Add(JMD);
            EnumList.Add(JOD);
            EnumList.Add(JPY);
            EnumList.Add(KES);
            EnumList.Add(KGS);
            EnumList.Add(KHR);
            EnumList.Add(KMF);
            EnumList.Add(KPW);
            EnumList.Add(KRW);
            EnumList.Add(KWD);
            EnumList.Add(KYD);
            EnumList.Add(KZT);
            EnumList.Add(LAK);
            EnumList.Add(LBP);
            EnumList.Add(LKR);
            EnumList.Add(LRD);
            EnumList.Add(LSL);
            EnumList.Add(LTL);
            EnumList.Add(LYD);
            EnumList.Add(MAD);
            EnumList.Add(MDL);
            EnumList.Add(MGA);
            EnumList.Add(MKD);
            EnumList.Add(MMK);
            EnumList.Add(MNT);
            EnumList.Add(MOP);
            EnumList.Add(MRO);
            EnumList.Add(MUR);
            EnumList.Add(MVR);
            EnumList.Add(MWK);
            EnumList.Add(MXN);
            EnumList.Add(MXV);
            EnumList.Add(MYR);
            EnumList.Add(MZN);
            EnumList.Add(NAD);
            EnumList.Add(NGN);
            EnumList.Add(NIO);
            EnumList.Add(NOK);
            EnumList.Add(NPR);
            EnumList.Add(NZD);
            EnumList.Add(OMR);
            EnumList.Add(PAB);
            EnumList.Add(PEN);
            EnumList.Add(PGK);
            EnumList.Add(PHP);
            EnumList.Add(PKR);
            EnumList.Add(PLN);
            EnumList.Add(PYG);
            EnumList.Add(QAR);
            EnumList.Add(RON);
            EnumList.Add(RSD);
            EnumList.Add(RUB);
            EnumList.Add(RWF);
            EnumList.Add(SAR);
            EnumList.Add(SBD);
            EnumList.Add(SCR);
            EnumList.Add(SDG);
            EnumList.Add(SEK);
            EnumList.Add(SGD);
            EnumList.Add(SHP);
            EnumList.Add(SLL);
            EnumList.Add(SOS);
            EnumList.Add(SRD);
            EnumList.Add(SSP);
            EnumList.Add(STD);
            EnumList.Add(SYP);
            EnumList.Add(SZL);
            EnumList.Add(THB);
            EnumList.Add(TJS);
            EnumList.Add(TMT);
            EnumList.Add(TND);
            EnumList.Add(TOP);
            EnumList.Add(TRY);
            EnumList.Add(TTD);
            EnumList.Add(TWD);
            EnumList.Add(TZS);
            EnumList.Add(UAH);
            EnumList.Add(UGX);
            EnumList.Add(USD);
            EnumList.Add(USN);
            EnumList.Add(USS);
            EnumList.Add(UYI);
            EnumList.Add(UYU);
            EnumList.Add(UZS);
            EnumList.Add(VEF);
            EnumList.Add(VND);
            EnumList.Add(VUV);
            EnumList.Add(WST);
            EnumList.Add(XAF);
            EnumList.Add(XAG);
            EnumList.Add(XAU);
            EnumList.Add(XBA);
            EnumList.Add(XBB);
            EnumList.Add(XBC);
            EnumList.Add(XBD);
            EnumList.Add(XCD);
            EnumList.Add(XDR);
            //EnumList.Add(XFU);
            EnumList.Add(XOF);
            EnumList.Add(XPD);
            EnumList.Add(XPF);
            EnumList.Add(XPT);
            EnumList.Add(XTS);
            EnumList.Add(XXX);
            EnumList.Add(YER);
            EnumList.Add(ZAR);
            EnumList.Add(ZMW);
            EnumList.Add(ZWL);
        }
    }

    public class EMVCurrencyCode : EnumBase
    {
        
        public string CurrencyName { get; }
        public int Digits { get; }

        public EMVCurrencyCode(int code, string currencyName, int digits)
        {
            this.Code = code;
            this.CurrencyName = currencyName;
            this.Digits = digits;
        }
    }
}
