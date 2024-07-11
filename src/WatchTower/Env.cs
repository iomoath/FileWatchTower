using System.IO;
using System.Reflection;

namespace WatchTower
{
    internal class Env
    {
        public static readonly string LogsDirectoryPath = Path.Combine(AssemblyDirectory, "logs");
        public static readonly string LogFilePath = Path.Combine(LogsDirectoryPath, "log-.log");


        public static string AssemblyDirectory
        {
            get
            {
                var currentAssembly = Assembly.GetExecutingAssembly();
                var assemblyLocation = currentAssembly.Location;
                return Path.GetDirectoryName(assemblyLocation);
            }
        }
    }
}
