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
using System.Threading.Tasks;
using Windows.Storage;

namespace DCEMV.UWPFileDriver
{
    public class UniversalWindowsFileDriver : IFileDriver
    {
        public string LoadFile(string filename)
        {
            string text = null;
            Task.Run(async () =>
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = (StorageFile)await storageFolder.TryGetItemAsync(filename);
                if (sampleFile != null)
                    text = await FileIO.ReadTextAsync(sampleFile);
            }).Wait();
            return text;
        }

        public void SaveFile(string filename, string text)
        {
            Task.Run(async () =>
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await localFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(sampleFile, text);
            }).Wait();
        }
    }
}
