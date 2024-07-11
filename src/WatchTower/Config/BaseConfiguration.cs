using System.Xml.Serialization;

namespace WatchTower
{
    public abstract class BaseConfiguration
    {
        [XmlAttribute("schemaversion")]
        public string SchemaVersionString
        {
            get => ConfigSchemaVersion.ToString(SchemaVersion);
            set => SchemaVersion = ConfigSchemaVersion.FromString(value);
        }

        [XmlIgnore]
        public SchemaVersion SchemaVersion { get; set; }


        [XmlElement("WriteToWinEventLogs")]
        public bool WriteToWinEventLogs { get; set; } = true;

        [XmlElement("WinEventLogOutputFormat")]
        public LogOutputFormat WinEventLogOutputFormat { get; set; } = LogOutputFormat.Xml;

        [XmlElement("ApiLogEndpointUrl")]
        public string ApiLogEndpointUrl { get; set; }

        [XmlElement("ApiExtraHeaders")]
        public ApiExtraHeaders[] ApiExtraHeaders { get; set; }

        [XmlElement("ApiLogOutputFormat")]
        public LogOutputFormat ApiLogOutputFormat { get; set; } = LogOutputFormat.Json;

        [XmlElement("LogDirectoryPath")]
        public string LogDirectoryPath { get; set; }

        [XmlElement("LogFileOutputFormat")]
        public LogOutputFormat LogFileOutputFormat { get; set; } = LogOutputFormat.NdJson;


        [XmlElement("HashAlgorithms")] public string HashAlgorithms { get; set; }

        /// <summary>
        /// Check if code-signing certificate has been revoked
        /// </summary>
        [XmlElement("CheckRevocation")]
        public bool CheckRevocation { get; set; }


        /// <summary>
        /// Deep analysis of MS Office docs, parse macros .. // NOT IMPLEMENTED
        /// </summary>
        [XmlElement("AnalyzeMsDocsMacros")]
        public bool AnalyzeMsDocsMacros { get; set; }

        /// <summary>
        /// Deep analysis of .img, .iso
        /// </summary>
        [XmlElement("AnalyzeDiscImages")]
        public bool AnalyzeDiscImages { get; set; }


        /// <summary>
        /// Deep analysis of .lnk
        /// </summary>
        [XmlElement("AnalyzeLnkFiles")]
        public bool AnalyzeLnkFiles { get; set; }

        /// <summary>
        /// Alternate data streams: Extract Zone Identifier Information
        /// </summary>
        [XmlElement("ExtractFileNtfsZoneIdentifierInformation")]
        public bool ExtractFileNtfsZoneIdentifierInformation { get; set; }

        /// <summary>
        /// The maximum length of the target file in bytes. Default is 16MB. If the target file exceeds the specified value, it will be excluded. A zero value will process all files regardless of their size..
        /// </summary>
        [XmlElement("MaxTargetFileSize")]
        public long MaxTargetFileSize { get; set; } = 16777216;
    }
}
