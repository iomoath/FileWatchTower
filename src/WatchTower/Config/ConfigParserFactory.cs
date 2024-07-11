using System.Xml.Linq;
using Serilog;

namespace WatchTower
{
    public static class ConfigParserFactory
    {
        public static IConfigParser<T> GetParser<T>(string configXml) where T : BaseConfiguration
        {
            var schemaVersion = GetSchemaVersion(configXml);

            if (typeof(T) == typeof(Configuration) && schemaVersion == SchemaVersion.V10)
            {
                return (IConfigParser<T>)new ConfigParser();
            }

            throw new InvalidConfigFileException(ConfigSchemaVersion.ToString(schemaVersion));
        }


        public static SchemaVersion GetSchemaVersion(XDocument document)
        {
            var ver = GetSchemaVersionString(document);
            return ConfigSchemaVersion.FromString(ver);
        }

        public static SchemaVersion GetSchemaVersion(string xml)
        {
            var ver = GetSchemaVersionString(xml);
            return ConfigSchemaVersion.FromString(ver);
        }

        public static string GetSchemaVersionString(string xml)
        {
            var schemaVersion = string.Empty;

            try
            {
                var document = XDocument.Parse(xml);
                var element = document.Element(GlobalConfigs.ConfigSchemaRootName);

                if (element != null)
                    return GetSchemaVersionString(element);

                throw new InvalidConfigFileException(schemaVersion);

            }
            catch (System.Exception e)
            {
                Log.Error(e, e.Message);
                throw new InvalidConfigFileException(schemaVersion, e.Message);
            }
        }

        public static string GetSchemaVersionString(XDocument document)
        {
            var schemaVersion = string.Empty;

            try
            {
                var element = document.Element(GlobalConfigs.ConfigSchemaRootName);
                if (element != null)
                    return GetSchemaVersionString(element);

                throw new InvalidConfigFileException(schemaVersion);


            }
            catch (System.Exception e)
            {
                Log.Error(e, e.Message);
                throw new InvalidConfigFileException(schemaVersion, e.Message);
            }
        }


        public static string GetSchemaVersionString(XElement element)
        {
            return element.Attribute("schemaversion")?.Value;
        }

    }
}
