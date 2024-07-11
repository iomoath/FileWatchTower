using System;

namespace WatchTower
{
    public class SchemaVersionAttribute : Attribute
    {
        public string Version { get; }

        public SchemaVersionAttribute(string version)
        {
            Version = version;
        }
    }
}
