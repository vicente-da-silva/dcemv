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
using System.IO;
using System.Xml.Serialization;

namespace DCEMV.Shared
{
    public static class XMLUtil<T>
    {
        public static Logger Logger = new Logger(typeof(XMLUtil<T>));

        public static T Deserialize(string xml)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(xml);
            writer.Flush();
            stream.Position = 0;
            using (stream)
            {
                Logger.Log(Formatting.ByteArrayToASCIIString(stream.ToArray()));
                return (T)serializer.Deserialize(stream);
            }

            //available in .net standard 2.0
        //    using (XmlReader schemaReader = XmlReader.Create("schema.xsd"))
        //    {
        //        XmlSchemaSet schemaSet = new XmlSchemaSet();

        //        //add the schema to the schema set
        //        schemaSet.Add(XmlSchema.Read(schemaReader,
        //        new ValidationEventHandler(
        //            delegate (Object sender, ValidationEventArgs e)
        //            {
        //            }
        //        )));

        //        //Load and validate against the programmatic schema set
        //        XmlDocument xmlDocument = new XmlDocument();
        //        xmlDocument.Schemas = schemaSet;
        //        xmlDocument.Load("something.xml");

        //        xmlDocument.Validate(new ValidationEventHandler(
        //            delegate (Object sender, ValidationEventArgs e)
        //            {
        //    //Report or respond to the error/warning
        //}
        //        ));
        //    }
        }

        public static string Serialize(T input)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(T));
            MemoryStream stream = new MemoryStream();
            string result;
            using (stream)
            {
                serializer.Serialize(stream, input);
                result = Formatting.ByteArrayToASCIIString(stream.ToArray());
            }
            return result;
        }
    }
}
