using System;
using System.IO;
using System.Text;
using System.Xml.Linq;
using Serilog;

namespace WatchTower
{
    public class ConfigParser : IConfigParser<Configuration>
    {
        public Configuration Parse(string xmlString)
        {
            var configObj = XmlSerializerDeserializer<Configuration>.DeserializeData(xmlString);
            if (!ValidateConfiguration(configObj))
                return null;

            return configObj;
        }

        public Configuration Parse(XDocument document)
        {
            var configObj = XmlSerializerDeserializer<Configuration>.DeserializeData(document);

            if (!ValidateConfiguration(configObj))
                return null;

            return configObj;
        }

        public Configuration ParseFromFile(string xmlConfigFilePath)
        {
            var configXml = ReadFileAllText(xmlConfigFilePath);
            var configObj = XmlSerializerDeserializer<Configuration>.DeserializeData(configXml);

            if (!ValidateConfiguration(configObj))
                return null;

            return configObj;
        }

        private static string ReadFileAllText(string filePath)
        {
            try
            {
                using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var tr = new StreamReader(fs, Encoding.UTF8))
                {
                    return tr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return null;
            }
        }

        private bool ValidateConfiguration(Configuration config)
        {
            return true;
        }

    }
}
