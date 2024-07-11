using System.Xml.Serialization;

namespace WatchTower
{
    public class ApiExtraHeaders
    {
        [XmlElement("ApiExtraHeader")]
        public string[] Headers { get; set; }
    }
}
