using System.Xml.Linq;

namespace WatchTower
{
    public interface IConfigParser<out T> where T : BaseConfiguration
    {
        T Parse(string xmlString);
        T Parse(XDocument document);
        T ParseFromFile(string xmlConfigFilePath);
    }
}
