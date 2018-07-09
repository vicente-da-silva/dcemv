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

using Newtonsoft.Json;

namespace DCEMV.ServerShared
{
    public class Card
    {
        public string CardSerialNumberId { get; set; }
        public string FreindlyName { get; set; }
        public long DailySpendLimit { get; set; }
        public long MonthlySpendLimit { get; set; }
        public CardState CardState { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static Card FromJsonString(string json)
        {
            return JsonConvert.DeserializeObject<Card>(json);
        }
    }
}
