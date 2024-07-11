using System.Xml.Serialization;

namespace WatchTower
{
    [SchemaVersion("1.0"), XmlRoot("FileWatchTower")]
    public class Configuration : BaseConfiguration, IConfiguration
    {
        // Version specific attributes
    }
}
