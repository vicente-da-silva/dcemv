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
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DCEMV.EMVSecurity
{
    public class Properties
    {
        private Dictionary<string, string> properties = new Dictionary<string, string>();
        public void Load(string fileName)
        {
            foreach (string line in File.ReadAllLines(fileName))
            {
                if ((!string.IsNullOrEmpty(line)) &&
                    (!line.StartsWith(";")) &&
                    (!line.StartsWith("#")) &&
                    (!line.StartsWith("'")) &&
                    (line.Contains("=")))
                {
                    int index = line.IndexOf('=');
                    string key = line.Substring(0, index).Trim();

                    string value = line.Substring(index + 1).Trim();
                    int commentTagIndex = value.IndexOf('#');
                    if (commentTagIndex != -1)
                    {
                        value = value.Substring(0, commentTagIndex);
                    }
                    if ((value.StartsWith("\"") && value.EndsWith("\"")) ||
                        (value.StartsWith("'") && value.EndsWith("'")))
                    {
                        value = value.Substring(1, value.Length - 2);
                    }
                    properties.Add(key, value);
                }
            }
        }

        public void Save(string fileName)
        {
            File.WriteAllLines(fileName, properties.Keys.ToList().Select(x => { return x + "=" + properties[x]; }));
        }

        public string GetProperty(string key)
        {
            return properties[key];
        }

        public void SetProperty(string key, string value)
        {
            properties.Add(key, value);
        }
    }
}
