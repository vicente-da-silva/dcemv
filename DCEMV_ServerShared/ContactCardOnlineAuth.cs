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
using DCEMV.TLVProtocol;
using Newtonsoft.Json;

namespace DCEMV.ServerShared
{
    public enum ContactCardOnlineAuthResponseEnum
    {
        Approved,
        Declined,
        UnableToGoOnline
    }
    public class ContactCardOnlineAuthResponse
    {
        public ContactCardOnlineAuthResponseEnum Response { get; set; }
        public string ResponseMessage { get; set; }
        public TLVasJSON AuthCode_8A { get; set; }
        public TLVasJSON IssuerAuthData_91 { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static ContactCardOnlineAuthResponse FromJsonString(string jsonVal)
        {
            return JsonConvert.DeserializeObject<ContactCardOnlineAuthResponse>(jsonVal);
        }
    }
    public class ContactCardOnlineAuthRequest
    {
        public TLVasJSON EMV_Data { get; set; }

        public string ToJsonString()
        {
            return JsonConvert.SerializeObject(this);
        }
        public static ContactCardOnlineAuthRequest FromJsonString(string jsonVal)
        {
            return JsonConvert.DeserializeObject<ContactCardOnlineAuthRequest>(jsonVal);
        }
    }
}
