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
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace DCEMV.Shared
{
    public abstract class InOutQsBase<T, R>
    {
        protected ConcurrentQueue<T> InQ { get; }
        protected ConcurrentQueue<R> OutQ { get; }

        private DateTime sw;
        private int timeoutMS;
        private int waitTimeMS = 10;

        public InOutQsBase(int timeoutMS)
        {
            this.timeoutMS = timeoutMS;
            sw = DateTime.Now;
            InQ = new ConcurrentQueue<T>();
            OutQ = new ConcurrentQueue<R>();
        }

        public virtual int GetInputQCount()
        {
            return InQ.Count;
        }

        public virtual int GetOutputQCount()
        {
            return OutQ.Count;
        }

        public virtual void EnqueueToInput(T message)
        {
            InQ.Enqueue(message);
        }

        public virtual void EnqueueToOutput(R message)
        {
            OutQ.Enqueue(message);
        }

        public virtual T DequeueFromInput(bool isPeek, bool useTimeout = false)
        {
            sw = DateTime.Now;
            T qItem = default(T); 
            while (1 == 1)
            {
                if (useTimeout)
                {
                    if ((DateTime.Now - sw).TotalMilliseconds > timeoutMS)
                        break;
                }

                if (InQ.Count == 0)
                {
                    Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                    continue;
                }

                if (isPeek)
                {
                    if (!InQ.TryPeek(out qItem))
                    {
                        Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                        continue;
                    }
                    else
                        break;
                }
                else
                {
                    if (!InQ.TryDequeue(out qItem))
                    {
                        Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                        continue;
                    }
                    else
                        break;
                }
            }
            
            return qItem;
        }

        public virtual R DequeueFromOutput(bool isPeek, bool useTimeout = false)
        {
            sw = DateTime.Now;
            R qItem = default(R);
            while (1 == 1)
            {
                if (useTimeout)
                {
                    if ((DateTime.Now - sw).TotalMilliseconds > timeoutMS)
                        break;
                }

                if (OutQ.Count == 0)
                {
                    Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                    continue;
                }

                if (isPeek)
                {
                    if (!OutQ.TryPeek(out qItem))
                    {
                        Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                        continue;
                    }
                    else
                        break;
                }
                else
                {
                    if (!OutQ.TryDequeue(out qItem))
                    {
                        Task.Run(async () => await Task.Delay(waitTimeMS)).Wait();
                        continue;
                    }
                    else
                        break;
                }
            }
            return qItem;
        }
    }
}
