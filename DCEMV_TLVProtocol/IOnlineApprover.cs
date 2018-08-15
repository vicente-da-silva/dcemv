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
using DCEMV_QRDEProtocol;

namespace DCEMV.TLVProtocol
{
    public interface IOnlineApprover
    {
        ApproverResponseBase DoAuth(ApproverRequestBase request);
        ApproverResponseBase DoCheckAuthStatus(ApproverRequestBase request);
        ApproverResponseBase DoAdvice(ApproverRequestBase request, bool isOnline);
        ApproverResponseBase DoReversal(ApproverRequestBase request, bool isOnline);
    }

    public enum QRCodeRequestType
    {
        Poll,
        Process,
    }
    public abstract class ApproverResponseBase
    {
        public bool IsApproved { get; set; }
        public string ResponseMessage { get; set; }
    }
    public abstract class ApproverRequestBase
    {
        public TCPClientStream TCPClientStream { get; set; }
    }

    
    public class QRCodeApproverResponse : ApproverResponseBase
    {

    }
    public class QRCodeApproverRequest : ApproverRequestBase
    {
        public QRCodeRequestType QRCodeRequestType { get; set; }
        public QRDEList QRData { get; set; }
    }

    public class EMVApproverResponse : ApproverResponseBase
    {
        public TLV AuthCode_8A { get; set; }
        public TLV IssuerAuthData_91 { get; set; }
        public TLV IssuerScriptTemplate_72 { get; set; }
        public TLV IssuerScriptTemplate_71 { get; set; }
    }
    public class EMVApproverRequest : ApproverRequestBase
    {
        public TLV EMV_Data { get; set; }
    }
}