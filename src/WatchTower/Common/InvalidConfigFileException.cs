namespace WatchTower
{
    public class InvalidConfigFileException : System.Exception
    {
        public InvalidConfigFileException(string schemaVersion)
            : this(schemaVersion, string.Empty)
        {
        }

        public InvalidConfigFileException(string schemaVersion, string exceptionMessage)
            : base(BuildMessage(schemaVersion, exceptionMessage))
        {
            SchemaVersion = schemaVersion ?? string.Empty;
        }

        private static string BuildMessage(string schemaVersion, string exceptionMessage)
        {
            string message = "The provided configuration is invalid or unsupported.";

            if (!string.IsNullOrEmpty(schemaVersion) && !string.IsNullOrWhiteSpace(schemaVersion))
            {
                message += $" schemaversion=\"{schemaVersion}\".";
            }

            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                message += $" {exceptionMessage}";
            }

            return message;
        }

        public string SchemaVersion { get; }
    }
}
