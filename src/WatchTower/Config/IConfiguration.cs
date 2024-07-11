namespace WatchTower
{
    public interface IConfiguration
    {
        string SchemaVersionString { get; set; }
        SchemaVersion SchemaVersion { get; set; }

        /// <summary>
        /// Write Event Logs to a local or remote directory.
        /// </summary>
        string LogDirectoryPath { get; set; }
        LogOutputFormat LogFileOutputFormat { get; set; }


        /// <summary>
        /// Write WinEvent logs
        /// </summary>
        bool WriteToWinEventLogs { get; set; }

        LogOutputFormat WinEventLogOutputFormat { get; set; }


        /// <summary>
        /// Output format: JSON
        /// </summary>
        string ApiLogEndpointUrl { get; set; }

        /// <summary>
        /// Extra Headers to embed in the HTTP request. E.g, Authorization token
        /// </summary>
        ApiExtraHeaders[] ApiExtraHeaders { get; set; }

        LogOutputFormat ApiLogOutputFormat { get; set; }


        string HashAlgorithms { get; set; }

        /// <summary>
        /// Check if code-signing certificate has been revoked
        /// </summary>
        bool CheckRevocation { get; set; }


        /// <summary>
        /// Deep analysis of MS Office docs, parse macros
        /// </summary>
        bool AnalyzeMsDocsMacros { get; set; }

        /// <summary>
        /// Deep analysis of .img, .iso
        /// </summary>
        bool AnalyzeDiscImages { get; set; }


        /// <summary>
        /// Deep analysis of .lnk
        /// </summary>
        bool AnalyzeLnkFiles { get; set; }

        /// <summary>
        /// Alternate data streams: Extract Zone Identifier Information
        /// </summary>
        bool ExtractFileNtfsZoneIdentifierInformation { get; set; }

        /// <summary>
        /// The maximum length of the target file in bytes. Default is 16MB. If the target file exceeds the specified value, it will be excluded. A zero value will process all files regardless of their size..
        /// </summary>
        long MaxTargetFileSize { get; set; }
    }
}
