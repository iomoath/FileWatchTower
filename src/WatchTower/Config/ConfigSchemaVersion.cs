using System;

namespace WatchTower
{
    public static class ConfigSchemaVersion
    {
        public static string ToString(SchemaVersion schemaVersion)
        {
            switch (schemaVersion)
            {
                case SchemaVersion.V10:
                    return "1.0";
                default:
                    throw new ArgumentOutOfRangeException(nameof(schemaVersion), schemaVersion, null);
            }
        }

        public static SchemaVersion FromString(string version)
        {
            switch (version)
            {
                case "1.0":
                    return SchemaVersion.V10;
                default:
                    throw new InvalidConfigFileException(version);
            }
        }
    }
}
