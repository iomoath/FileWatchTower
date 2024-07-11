using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Serilog;

namespace WatchTower
{
    public static class Helpers
    {

        public static bool CreateEventSource()
        {
            try
            {
                var logNames = new List<string>
                {
                    "App",
                    "App2",
                    "App5",
                    "FileWatchTower-App",
                    "FileWatchTower-B",
                    "FileWatchTower-A",
                    "FileWatchTower",
                    "WatchTower",
                    "FileWatchTower-Events",
                    "FileWatchTower-Operational",
                    "FileWatchTower-Events",
                    "FileWatchTower-1",
                    "FileWatchTower5",
                    "XFileWatchTower",
                    "MyNiceApp",
                    "MyNiceTower",
                    "MyApplicationLog",
                    "FileWatchTower-Agent"



                };
                foreach (var logName in logNames)
                {
                    if (EventLog.Exists(logName))
                    {
                        EventLog.Delete(logName);

                        Console.WriteLine("Event log and source deleted successfully.");

                    }
                }

                if (EventLog.SourceExists("FileWatchTower"))
                {
                    EventLog.DeleteEventSource("FileWatchTower");
                    Console.WriteLine("Event source deleted successfully.");
                }


                if (EventLog.SourceExists("FileWatchTower-Events"))
                {
                    EventLog.DeleteEventSource("FileWatchTower-Events");
                    Console.WriteLine("Event source deleted successfully.");
                }
                if (EventLog.SourceExists("FileWatchTower-Agent"))
                {
                    EventLog.DeleteEventSource("FileWatchTower-Agent");
                    Console.WriteLine("Event source deleted successfully.");
                }

                if (!EventLog.SourceExists(GlobalConfigs.WinLogSourceName))
                {
                    EventLog.CreateEventSource(new EventSourceCreationData(GlobalConfigs.WinLogSourceName, GlobalConfigs.WinLogName));

                    //EventLog.CreateEventSource(GlobalConfigs.WinLogSourceName, GlobalConfigs.WinLogName);
                }


                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Log.Error(e, $"Error creating event source: {e.Message}");
                return false;
            }
        }

        public static string SerializeObjectWithoutNulls(object obj)
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.None,
                Converters = new JsonConverter[] { new StringEnumConverter() }
            };

            return JsonConvert.SerializeObject(obj, settings);
        }

        public static byte[] ReadFile(string filePath)
        {
            try
            {
                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    var fileBytes = new byte[fileStream.Length];
                    fileStream.Read(fileBytes, 0, fileBytes.Length);
                    return fileBytes;
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return null;
            }
        }


        public static string ConvertToTimeStr(long input)
        {
            return DateTime.FromFileTime(input).ToString(GlobalConfigs.DateTimeFormat);
        }

        public static DateTime ToUtcDateTime(DateTime dateTime)
        {
            return TimeZoneInfo.ConvertTimeToUtc(dateTime, TimeZoneInfo.Local);
        }

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            // Unix timestamp is seconds past epoch
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            dateTime = dateTime.AddSeconds(unixTimeStamp).ToLocalTime();
            return dateTime;
        }


        public static MemoryStream FromBytesToMemoryStream(byte[] fileBytes)
        {
            return new MemoryStream(fileBytes);
        }

        public static Process GetProcById(int id)
        {
            if (!WinProcessHelper.IsProcessAlive(id))
                return null;

            Process[] processlist = Process.GetProcesses();
            return processlist.FirstOrDefault(pr => pr.Id == id);
        }

        public static string GetComputerName()
        {
            try
            {
                return System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName()).HostName;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            try
            {
                return System.Net.Dns.GetHostEntry(string.Empty).HostName;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return string.Empty;
        }

        public static string ComputeFileHash(string filePath, HashAlgorithm algorithm)
        {
            if (!File.Exists(filePath))
            {
                return null;
            }

            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    using (algorithm)
                    {
                        var hash = algorithm.ComputeHash(stream);
                        return ConvertHashToHexadecimalString(hash);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return null;
        }


        public static string ComputeHash(string input, HashAlgorithm algorithm)
        {
            try
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);

                using (algorithm)
                {
                    var hash = algorithm.ComputeHash(inputBytes);
                    return ConvertHashToHexadecimalString(hash);
                }
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
            }

            return null;
        }


        private static string ConvertHashToHexadecimalString(byte[] hash)
        {
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }
}
