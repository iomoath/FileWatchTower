using Serilog;
using System;
using System.IO;
using System.Text;
using Trinet.Core.IO.Ntfs;

namespace WatchTower
{
    public class NtfsZoneIdentifierParser
    {
        public static string ZoneInformation(string filePath)
        {
            // Source Project: https://github.com/RichardD2/NTFS-Streams

            try
            {
                var file = new FileInfo(filePath);

                if (!file.Exists)
                    return null;

                string zoneInfo;
                AlternateDataStreamInfo s = file.GetAlternateDataStream("Zone.Identifier", FileMode.Open);
                using (TextReader reader = s.OpenText())
                {
                    zoneInfo = reader.ReadToEnd();
                }

                zoneInfo = zoneInfo.Replace("\r\n", ";");
                var parts = zoneInfo.Split(';');
                var sb = new StringBuilder("[ZoneTransfer] ");

                foreach (string p in parts)
                {
                    var part = p.Trim();

                    if (!part.Contains("=") || string.IsNullOrEmpty(part) || string.IsNullOrWhiteSpace(part))
                        continue;

                    sb.Append($"{part}; ");
                }

                var info = sb.ToString().Trim().TrimEnd(';');
                info += "[/ZoneTransfer]";
                return info;
            }
            catch (Exception e)
            {
                Log.Error(e, e.Message);
                return null;
            }
        }
    }
}
