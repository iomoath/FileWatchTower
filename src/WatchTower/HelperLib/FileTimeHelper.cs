using System;
using System.IO;

namespace WatchTower
{
    public static class FileTimeHelper
    {
        private const int PeHeaderOffset = 60;
        private const int LinkerTimestampOffset = 8;

        public static DateTime? GetLinkerTime(string filePath, TimeZoneInfo target = null)
        {
            var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            var buffer = new byte[2048];

            try
            {

                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    stream.Read(buffer, 0, 2048);
                }

                var offset = BitConverter.ToInt32(buffer, PeHeaderOffset);
                var secondsSince1970 = BitConverter.ToInt32(buffer, offset + LinkerTimestampOffset);

                var linkTimeUtc = epoch.AddSeconds(secondsSince1970);

                var tz = target ?? TimeZoneInfo.Local;
                var localTime = TimeZoneInfo.ConvertTimeFromUtc(linkTimeUtc, tz);

                return localTime;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
