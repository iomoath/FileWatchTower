using System.Xml.Serialization;

namespace WatchTower
{
    public enum SchemaVersion
    {
        V10, // 1.0
    }

    public enum EventName
    {
        Unsupported,

        /// <summary>
        /// FileWatchTower Error
        /// </summary>
        [XmlEnum(Name = "FileWatchtowerError")]
        FileWatchtowerError,

        /// <summary>
        /// FileWatchTower Service configuration change event, 
        /// </summary>
        [XmlEnum(Name = "FileWatchtowerServiceConfigurationChange")]
        FileWatchtowerServiceConfigurationChange,


        /// <summary>
        /// FileWatchTower Service state change event, 
        /// </summary>
        [XmlEnum(Name = "FileWatchtowerServiceStateChange")]
        FileWatchtowerServiceStateChange,

        /// <summary>
        /// File creation event. // ID 11
        /// </summary>
        [XmlEnum(Name = "FileCreate")]
        FileCreate,


        /// <summary>
        /// This event is generated when an executable gets written to disk FileExecutableDetected // ID 29
        /// </summary>
        [XmlEnum(Name = "FileExecutableDetected")]
        FileExecutableDetected,


        /// <summary>
        /// a file's last write time, a file's creation time, a file's last access time were changed. // ID 2
        /// </summary>
        [XmlEnum(Name = "FileCreateTime")]
        FileCreateTime,
    }



    public enum EventLevel
    {
        [XmlEnum(Name = "Informational")] Informational,
        [XmlEnum(Name = "Warning")] Warning,
        [XmlEnum(Name = "Error")] Error,
        [XmlEnum(Name = "Critical")] Critical
    }

    public enum LogOutputFormat
    {
        /// <summary>
        /// NDJSON , Newline Delimited JSON
        /// </summary>
        [XmlEnum(Name = "ndjson")]
        NdJson,

        /// <summary>
        /// JSON
        /// </summary>
        [XmlEnum(Name = "json")]
        Json,

        /// <summary>
        /// XML
        /// </summary>
        [XmlEnum(Name = "xml")]
        Xml,
    }
}
