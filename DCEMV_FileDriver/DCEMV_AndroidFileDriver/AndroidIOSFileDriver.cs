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
using System;
using System.IO;

namespace DCEMV.AndroidFileDriver
{
    public class AndroidIOSFileDriver : IFileDriver
    {
        public string LoadFile(string filename)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, filename);
            if (File.Exists(filePath))
                return File.ReadAllText(filePath);
            else
                return null;
        }

        public void SaveFile(string filename, string text)
        {
            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string filePath = Path.Combine(documentsPath, filename);
            File.WriteAllText(filePath, text);
        }
    }
}
