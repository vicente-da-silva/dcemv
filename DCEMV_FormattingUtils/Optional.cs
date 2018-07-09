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
using System.Collections;
using System.Collections.Generic;

namespace DCEMV.FormattingUtils
{
    public class Optional<T> : IEnumerable<T>
    {
        private readonly T[] data;

        private Optional(T[] data)
        {
            this.data = data;
        }

        public bool IsPresent()
        {
            if (data.Length == 0)
                return false;
            else
                return true;
        }

        public T Get()
        {
            if (IsPresent())
                return data[0];
            else
                throw new Exception("Called Optional Get on empty Optional");
        }

        public static Optional<T> Create(T element)
        {
            if(element == null)
                return new Optional<T>(new T[0]);
            else
                return new Optional<T>(new[] { element });
        }

        public static Optional<T> CreateEmpty()
        {
            return new Optional<T>(new T[0]);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return ((IEnumerable<T>)this.data).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
