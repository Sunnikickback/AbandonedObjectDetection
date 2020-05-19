using System;
using System.IO;
using System.Xml.Serialization;

namespace ClassLibrary
{
    public class XMLManager
    {
        public static void XMLWriter(object obj, string fileName)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(obj.GetType());
            StreamWriter sw = new StreamWriter(fileName);
            xmlSerializer.Serialize(sw, obj);
            sw.Close();
        }

        public static Data XMLReader(string filename)
        {
            Data data = new Data();
            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(Data));
                FileStream sr = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                data = (Data)xmlSerializer.Deserialize(sr);
                sr.Close();
            }
            catch(Exception)
            {
                ;
            }
            return data;
        }
    }
}
