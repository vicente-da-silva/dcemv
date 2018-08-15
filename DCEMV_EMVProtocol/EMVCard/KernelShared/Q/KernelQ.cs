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
using System.Linq;

namespace DCEMV.EMVProtocol.Kernels
{
    public class KernelQ : InOutQsBase<KernelRequest, KernelResponseBase>
    {
        public KernelQ(int timeoutMS) : base(timeoutMS)
        {
        }

        public override int GetOutputQCount()
        {
            //the states call this method to check if there are pending requests to the terminal,
            //if there are they will wait for the response from the terminal, to the kernels input q, UI responses to the terminal
            //should not be included in the count as they do not result in the terminal posting a response to the rquest 
            //in the input q of the kernel, we want to ignore there messages as they are one way.
            return OutQ.Where(x => !(x is KernelUIResponse)).Count();
        }
    }
}
