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
using DCEMV.FormattingUtils;
using Org.BouncyCastle.Crypto.Digests;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace DCEMV.GlobalPlatformProtocol
{
    public class Manifest
    {

    }
    public class Attributes
    {

    }
    public class CapFile
    {
        public static String[] componentNames = {
            "Header", "Directory", "Import", "Applet", "Class", "Method", "StaticField", "Export",
            "ConstantPool", "RefLocation", "Descriptor", "Debug" };

        private Dictionary<String, byte[]> capComponents = new Dictionary<String, byte[]>();

        private String packageName = null;
        private AID packageAID = null;
        private byte major_version = 0;
        private byte minor_version = 0;
        private List<AID> appletAIDs = new List<AID>();
        private List<byte[]> dapBlocks = new List<byte[]>();
        private List<byte[]> loadTokens = new List<byte[]>();
        private List<byte[]> installTokens = new List<byte[]>();
        //private Manifest manifest = null;

        public CapFile(MemoryStream inval)
            : this(inval, null)
        {

        }
        private byte[] getEntry(Dictionary<String, byte[]> entries, String key)
        {
            if (entries.ContainsKey(key))
                return entries[key];
            else
                return null;
        }
        private CapFile(MemoryStream inval, String packageName)
        {
            ZipArchive zip = new ZipArchive(inval);
            Dictionary<String, byte[]> entries = getEntries(zip);
            if (packageName != null)
            {
                packageName = packageName.Replace('.', '/') + "/javacard/";
            }
            else
            {
                String lookFor = "Header.cap";
                foreach(String s in entries.Keys)
                {
                    if (s.EndsWith(lookFor))
                    {
                        packageName = s.Substring(0, s.LastIndexOf(lookFor));
                        break;
                    }
                }
            }

            // Parse manifest
            //byte[] mf = entries["META-INF/MANIFEST.MF"];
            //entries.Remove("META-INF/MANIFEST.MF");
            //if (mf != null)
            //{
            //    //ByteArrayInputStream mfi = new ByteArrayInputStream(mf);
            //    //manifest = new Manifest(mfi);
            //}

            // Avoid a possible NPE
            if (packageName == null)
            {
                throw new Exception("Could not figure out the package name of the applet!");
            }


            this.packageName = packageName.Substring(0, packageName.LastIndexOf("/javacard/")).Replace('/', '.');
            foreach (String name in componentNames)
            {
                String fullName = packageName + name + ".cap";
                byte[] contents = getEntry(entries,fullName);
                capComponents.Add(name, contents);
            }
            // FIXME: Not existing and not used ZIP elements
            List<List<byte[]>> tables = new List<List<byte[]>>();
            tables.Add(dapBlocks);
            tables.Add(loadTokens);
            tables.Add(installTokens);
            String[] names = { "dap", "lt", "it" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = 0;
                while (true)
                {
                    String fullName = "meta-inf/" + packageName.Replace('/', '-') + names[i] + (index + 1);
                    byte[] contents = getEntry(entries,fullName);
                    if (contents == null)
                    {
                        break;
                    }
                    tables[i].Add(contents);
                    index++;
                }
            }

            zip.Dispose();
		    inval.Dispose();

            // Parse package.
            // See JCVM 2.2 spec section 6.3 for offsets.
            byte[] header = capComponents["Header"];
            major_version = header[10];
            minor_version = header[11];
            packageAID = new AID(header, 13, header[12]);

            // Parse applets
            // See JCVM 2.2 spec section 6.5 for offsets.
            byte[] applet = capComponents["Applet"];
            if (applet != null)
            {
                int offset = 4;
                for (int j = 0; j < (applet[3] & 0xFF); j++)
                {
                    int len = applet[offset++];
                    appletAIDs.Add(new AID(applet, offset, len));
                    // Skip install_method_offset
                    offset += len + 2;
                }
            }
        }

        private Dictionary<String, byte[]> getEntries(ZipArchive inval)
        {
            Dictionary<String, byte[]> result = new Dictionary<String, byte[]>();
            foreach (ZipArchiveEntry entry in inval.Entries)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    entry.Open().CopyTo(ms);
                    result.Add(entry.FullName, ms.ToArray());
                }
            }
            return result;
        }

        public AID getPackageAID()
        {
            return packageAID;
        }

        public List<AID> getAppletAIDs()
        {
            List<AID> result = new List<AID>();
            result.AddRange(appletAIDs);
            return result;
        }

        public String getPackageName()
        {
            return packageName;
        }

        public int getCodeLength(bool includeDebug)
        {
            int result = 0;
            foreach (String name in componentNames)
            {
                if (!includeDebug && (name == "Debug" || name == "Descriptor"))
                {
                    continue;
                }
                byte[] data = capComponents[name];
                if (data != null)
                {
                    result += data.Length;
                }
            }
            return result;
        }

        private byte[] createHeader(bool includeDebug)
        {
            int len = getCodeLength(includeDebug);
            ByteArrayOutputStream bo = new ByteArrayOutputStream();
            // TODO: DAP blocks.
            bo.Write((byte)0xC4);
            // FIXME: usual length encoding.
            if (len < 0x80)
            {
                bo.Write((byte)len);
            }
            else if (len <= 0xFF)
            {
                bo.Write((byte)0x81);
                bo.Write((byte)len);
            }
            else if (len <= 0xFFFF)
            {
                bo.Write((byte)0x82);
                bo.Write((byte)((len & 0xFF00) >> 8));
                bo.Write((byte)(len & 0xFF));
            }
            else
            {
                bo.Write((byte)0x83);
                bo.Write((byte)((len & 0xFF0000) >> 16));
                bo.Write((byte)((len & 0xFF00) >> 8));
                bo.Write((byte)(len & 0xFF));
            }
            return bo.ToByteArray();
        }

        public List<byte[]> getLoadBlocks(bool includeDebug, bool separateComponents, int blockSize)
        {
            List<byte[]> blocks = new List<byte[]>();

            if (!separateComponents)
            {
                ByteArrayOutputStream bo = new ByteArrayOutputStream();
                try
                {
                    // TODO: DAP blocks.
                    // See GP 2.1.1 Table 9-40
                    bo.Write(createHeader(includeDebug));
                    bo.Write(getRawCode(includeDebug));
                }
                catch (IOException ioe)
                {
                    throw new Exception(ioe.Message);
                }
                blocks = Formatting.SplitArray(bo.ToByteArray(), blockSize);
            }
            else
            {
                foreach (String name in componentNames)
                {
                    if (!includeDebug && (name == "Debug" || name == "Descriptor"))
                    {
                        continue;
                    }

                    byte[] currentComponent = capComponents[name];
                    if (currentComponent == null)
                    {
                        continue;
                    }
                    if (name == "Header")
                    {
                        ByteArrayOutputStream bo = new ByteArrayOutputStream();
                        try
                        {
                            bo.Write(createHeader(includeDebug));
                            bo.Write(currentComponent);
                        }
                        catch (IOException ioe)
                        {
                            throw new Exception(ioe.Message);
                        }
                        currentComponent = bo.ToByteArray();
                    }
                    blocks = Formatting.SplitArray(currentComponent, blockSize);
                }
            }
            return blocks;
        }

        private byte[] getRawCode(bool includeDebug)
        {
            byte[] result = new byte[getCodeLength(includeDebug)];
            int offset = 0;
            foreach (String name in componentNames)
            {
                if (!includeDebug && (name == "Debug" || name == "Descriptor"))
                {
                    continue;
                }
                byte[] currentComponent = capComponents[name];
                if (currentComponent == null)
                {
                    continue;
                }
                Array.Copy(currentComponent, 0, result, offset, currentComponent.Length);
                offset += currentComponent.Length;
            }
            return result;
        }

        public byte[] getLoadFileDataHash(String hash, bool includeDebug)
        {
            try
            {
                GeneralDigest digester;
                if (hash == "SHA-256")
                    digester = new Sha256Digest();
                if (hash == "SHA-1")
                    digester = new Sha1Digest();
                else
                    throw new Exception("Unknown hash algorithm");

                byte[] retValue = new byte[digester.GetDigestSize()];
                byte[] inpuVal = getRawCode(includeDebug);
                digester.BlockUpdate(inpuVal, 0, inpuVal.Length);
                digester.DoFinal(retValue, 0);
                return retValue;
            }
            catch (Exception e)
            {
                throw new Exception("Not possible", e);
            }
        }

        //public void dump(PrintStream outVal)
        //{
        //    // Print information about CAP. First try manifest.
        //    if (manifest != null)
        //    {
        //        Attributes mains = manifest.getMainAttributes();

        //        // iterate all packages
        //        Dictionary<String, Attributes> ent = manifest.getEntries();
        //        if (ent.keySet().size() > 1)
        //        {
        //            throw new Exception("Too many elments in CAP");
        //        }
        //        Attributes caps = ent[ent.Keys.toArray()[0]];
        //        // Generic
        //        String jdk_name = mains.getValue("Created-By");
        //        // JC specific
        //        String cap_version = caps.getValue("Java-Card-CAP-File-Version");
        //        String cap_creation_time = caps.getValue("Java-Card-CAP-Creation-Time");
        //        String converter_version = caps.getValue("Java-Card-Converter-Version");
        //        String converter_provider = caps.getValue("Java-Card-Converter-Provider");
        //        String package_name = caps.getValue("Java-Card-Package-Name");
        //        String package_version = caps.getValue("Java-Card-Package-Version");
        //        String package_aid = caps.getValue("Java-Card-Package-AID");


        //        int num_applets = 0;
        //        int num_imports = 0;
        //        // Count applets and imports
        //        foreach (Object e in caps.keySet())
        //        {
        //            Attributes.Name an = (Attributes.Name)e;
        //            String s = an.toString();
        //            if (s.StartsWith("Java-Card-Applet-") && s.EndsWith("-Name"))
        //            {
        //                num_applets++;
        //            }
        //            else if (s.StartsWith("Java-Card-Imported-Package-") && s.EndsWith("-AID"))
        //            {
        //                num_imports++;
        //            }
        //            else
        //            {
        //                continue;
        //            }
        //        }
        //        //out.println("CAP file (v" + cap_version + ") generated on " + cap_creation_time);
        //        //out.println("By " + converter_provider + " converter " + converter_version + " with JDK " + jdk_name);
        //        String hexpkgaid = Formatting.ByteArrayToHexString(Formatting.HexStringToByteArray(package_aid));
        //        //out.println("Package: " + package_name + " v" + package_version + " with AID " + hexpkgaid);

        //        for (int i = 1; i <= num_applets; i++)
        //        {
        //            String applet_name = caps.getValue("Java-Card-Applet-" + i + "-Name");
        //            String applet_aid = caps.getValue("Java-Card-Applet-" + i + "-AID");
        //            String hexaid = Formatting.ByteArrayToHexString(Formatting.HexStringToByteArray(applet_aid));
        //            //out.println("Applet: " + applet_name + " with AID " + hexaid);
        //        }
        //        for (int i = 1; i <= num_imports; i++)
        //        {
        //            String import_aid = caps.getValue("Java-Card-Imported-Package-" + i + "-AID");
        //            String import_version = caps.getValue("Java-Card-Imported-Package-" + i + "-Version");
        //            String hexaid = Formatting.ByteArrayToHexString(Formatting.HexStringToByteArray(import_aid));
        //            //out.println("Import: " + hexaid + " v" + import_version);

        //        }
        //    }
        //    else
        //    {
        //        String pkg_version = major_version + "." + minor_version;
        //        //out.println("No manifest in CAP. Information from Header and Applet components:");
        //        //out.println("Package: " + packageName + " v" + pkg_version + " with AID " + packageAID);
        //        foreach (AID applet in appletAIDs)
        //        {
        //            //out.println("Applet: AID " + applet);
        //        }
        //    }
        //    //out.println("Total code size: " + getCodeLength(false) + " bytes (" + getCodeLength(true) + " with debug)");
        //    //out.println("SHA256 (code): " + HexUtils.bin2hex(getLoadFileDataHash("SHA-256", false)));
        //    //out.println("SHA1   (code): " + HexUtils.bin2hex(getLoadFileDataHash("SHA-1", false)));
        //}
    }
}
