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

namespace DCEMV.TLVProtocol
{
    public interface IOnlineApprover
    {
        ApproverResponse DoAuth(ApproverRequest request);
        ApproverResponse DoAdvice(ApproverRequest request, bool isOnline);
        ApproverResponse DoReversal(ApproverRequest request, bool isOnline);
    }

    public class ApproverResponse
    {
        public bool IsApproved { get; set; }
        public string ResponseMessage { get; set; }
        public TLV AuthCode_8A { get; set; }
        public TLV IssuerAuthData_91 { get; set; }
        public TLV IssuerScriptTemplate_72 { get; set; }
        public TLV IssuerScriptTemplate_71 { get; set; }
    }
    public class ApproverRequest
    {
        public TLV EMV_Data { get; set; }
        public TCPClientStream TCPClientStream { get; set; }
    }
}