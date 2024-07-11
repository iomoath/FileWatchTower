using System.Collections.Generic;
using PeNet;

namespace WatchTower
{
    public class FileInformation
    {
        private byte[] _fileBytes;

        public PeFile PeFile { get; set; }

        /// <summary>
        /// Use only if PeFile is null
        /// </summary>
        public byte[] Bytes
        {
            get => PeFile != null ? PeFile.RawFile.ToArray() : _fileBytes;
            set => _fileBytes = value;
        }

        public bool IsPeFile => PeFile != null;

        public long FileSize
        {
            get
            {
                if (PeFile != null)
                    return PeFile.FileSize;

                return _fileBytes.Length;
            }
        }


        public List<string> Strings { get; set; } = new List<string>();
    }
}
