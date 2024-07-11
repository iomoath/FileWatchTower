using System;
using System.IO;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml;

namespace WatchTower
{
    public static class XmlSerializerDeserializer<T> where T : class
    {
        private static readonly XmlSerializer XmlSerializer = new XmlSerializer(typeof(T));
        private static readonly XmlSerializerNamespaces XmlSerializerNamespaces = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true };

        public static string SerializeData(T data)
        {
            var xmlSerializer = new XmlSerializer(data.GetType());
            using (var stream = new StringWriter())
            using (var writer = XmlWriter.Create(stream, XmlWriterSettings))
            {
                xmlSerializer.Serialize(writer, data, XmlSerializerNamespaces);
                return stream.ToString();
            }
        }

        public static T DeserializeData(string dataXml)
        {
            using (var sr = new StringReader(dataXml))
            {
                return (T)XmlSerializer.Deserialize(sr);
            }
        }

        public static T DeserializeData(XDocument document)
        {
            using (var reader = document.Root.CreateReader())
            {
                return (T)XmlSerializer.Deserialize(reader);
            }
        }


        public static T DeserializeData(string dataXml, Type type)
        {
            if (string.IsNullOrEmpty(dataXml))
                return null;

            var xmlSerializer = new XmlSerializer(type);
            using (var sr = new StringReader(dataXml))
            {
                return (T)xmlSerializer.Deserialize(sr);
            }
        }

        public static T DeserializeData(XDocument document, Type type)
        {
            if (document.Root == null)
                return null;

            var xmlSerializer = new XmlSerializer(type);
            using (var reader = document.Root.CreateReader())
            {
                return (T)xmlSerializer.Deserialize(reader);
            }
        }
    }
}
