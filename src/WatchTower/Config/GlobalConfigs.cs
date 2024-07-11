namespace WatchTower
{
    public static class GlobalConfigs
    {
        public const string ConfigSchemaRootName = "FileWatchTower";

        // Name of the source for win event logging
        public const string WinLogSourceName = "FileWatchTower Agent";

        // Name of the log for win event logging
        public const string WinLogName = "FileWatchTower-Events";

        public const string DateTimeFormat = "yyyy-MM-dd h:mm:ss tt";
        public const int MaxPostProcessorWorkerThreads = 2;
        public const string DefaultUserAgent = "FileWatchTower/1.0";
    }
}
