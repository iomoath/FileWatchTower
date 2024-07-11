namespace SsdeepNET
{
    internal class Roll
    {
        public Roll() { }

        private byte[] _window = new byte[Constants.RollingWindow];
        private uint _h1 = 0;
        private uint _h2 = 0;
        private uint _h3 = 0;
        private uint _n = 0;

        public uint Sum => _h1 + _h2 + _h3;

        /// <summary>
        /// A rolling hash, based on the Adler checksum. By using a rolling hash
        /// we can perform auto resynchronisation after inserts/deletes.
        /// </summary>
        /// <remarks>
        /// Internally, h1 is the sum of the bytes in the window and h2
        /// is the sum of the bytes times the index.<br /><br />
        /// 
        /// h3 is a shift/xor based rolling hash, and is mostly needed to ensure that
        /// we can cope with large blocksize values.
        /// </remarks>
        public void Hash(byte c)
        {
            _h2 -= _h1;
            _h2 += Constants.RollingWindow * (uint)c;

            _h1 += c;
            _h1 -= _window[_n % Constants.RollingWindow];

            _window[_n % Constants.RollingWindow] = c;
            _n++;

            // The original spamsum AND'ed this value with 0xFFFFFFFF which
            // in theory should have no effect. This AND has been removed
            // for performance (jk).
            _h3 <<= 5;
            _h3 ^= c;
        }
    }
}
