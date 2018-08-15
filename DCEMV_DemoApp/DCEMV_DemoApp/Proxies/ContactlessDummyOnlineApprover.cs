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
using System;
using System.Collections.Generic;
using System.Text;

namespace DCEMV.DemoApp.Proxies
{
    public class ContactlessDummyOnlineApprover : IOnlineApprover
    {
        public ApproverResponseBase DoAdvice(ApproverRequestBase request, bool isOnline)
        {
            return new EMVApproverResponse()
            {
                IsApproved = true,
                ResponseMessage = "Approved Advice By ContactlessDummyOnlineApprover"
            };
        }

        public ApproverResponseBase DoAuth(ApproverRequestBase request)
        {
            return new EMVApproverResponse()
            {
                IsApproved = true,
                ResponseMessage = "Approved Auth By ContactlessDummyOnlineApprover"
            };
        }

        public ApproverResponseBase DoReversal(ApproverRequestBase request, bool isOnline)
        {
            return new EMVApproverResponse()
            {
                IsApproved = true,
                ResponseMessage = "Approved Reversal By ContactlessDummyOnlineApprover"
            };
        }

        public ApproverResponseBase DoCheckAuthStatus(ApproverRequestBase request)
        {
            return new EMVApproverResponse()
            {
                IsApproved = true,
                ResponseMessage = "Approved Reversal By ContactlessDummyOnlineApprover"
            };
        }
    }
}
