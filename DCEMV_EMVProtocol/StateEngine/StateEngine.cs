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

namespace DCEMV.EMVProtocol.Kernels
{
    public abstract class StateEngine
    {
        public static Logger Logger = new Logger(typeof(StateEngine));

        protected void DoStateChange(SignalsEnum eventVal)
        {
            ActionsEnum action = MapSignalToAction(eventVal);
            Logger.Log("Executing New Action: " + action + " From Signal Enum: " + eventVal);
            ExecuteAction(action);
        }

        protected abstract void ExecuteAction(ActionsEnum action);

        protected abstract ActionsEnum MapSignalToAction(SignalsEnum signal);
    }
}
