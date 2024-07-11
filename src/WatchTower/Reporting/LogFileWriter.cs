using System;
using System.IO;
using System.Linq;
using Serilog;

namespace WatchTower
{

    public sealed class LogFileWriter : IDisposable
    {
        private static readonly Lazy<LogFileWriter>

            LazyInstance = new Lazy<LogFileWriter>(() => new LogFileWriter());

        private static string _directory;
        private StreamWriter _streamWriter;
        private const int MaxFileSizeInBytes = 256 * 1024 * 1024; // 256 MB
        private static readonly object FileLock = new object();
        public static LogFileWriter Instance => LazyInstance.Value;

        private LogFileWriter()
        {
        }

        public void SetDirectory(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
            Directory.CreateDirectory(_directory);
            OpenNextFile();
        }

        public void WriteLog(string message)
        {
            if (string.IsNullOrEmpty(_directory))
            {
                throw new InvalidOperationException("Directory not set.");
            }

            lock (FileLock)
            {
                try
                {
                    if (_streamWriter == null || _streamWriter.BaseStream.Length >= MaxFileSizeInBytes)
                    {
                        OpenNextFile();
                    }

                    _streamWriter.WriteLine(message);
                    _streamWriter.Flush();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, ex.Message);
                }
            }
        }

        private void OpenNextFile()
        {
            _streamWriter?.Dispose();

            var existingFiles = Directory.GetFiles(_directory, "*.log")
                .Select(f => new FileInfo(f))
                .OrderByDescending(fi => fi.LastWriteTime);

            var mostRecentFile = existingFiles.FirstOrDefault();

            if (mostRecentFile != null && mostRecentFile.Length < MaxFileSizeInBytes)
            {
                _streamWriter = new StreamWriter(new FileStream(mostRecentFile.FullName, FileMode.Append, FileAccess.Write, FileShare.Read));
            }
            else
            {
                var newFileName = GenerateNewFileName();
                _streamWriter = new StreamWriter(new FileStream(newFileName, FileMode.Create, FileAccess.Write, FileShare.Read));
            }
        }

        private string GenerateNewFileName()
        {
            string fileName;
            do
            {
                var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                fileName = Path.Combine(_directory, $"{timestamp}.log");
            }
            while (File.Exists(fileName));

            return fileName;
        }

        public void Dispose()
        {
            _streamWriter?.Dispose();
        }
    }
}
