using System;

namespace SsdeepNET
{
    internal static class BufferUtilities
    {
        public static int EliminateSequences<T>(T[] src, int srcOffset, T[] dst, int dstOffset, int length, int window)
            where T : IEquatable<T>
        {
            if (src == null)
                throw new ArgumentNullException(nameof(src));

            if (dst != null)
                for (int i = 0; i < window; i++)
                    dst[dstOffset + i] = src[srcOffset + i];

            if (length <= window)
                return length;

            int j = window;
            for (int i = j; i < length; i++)
            {
                var current = src[srcOffset + i];
                bool duplicate = true;
                for (int w = 1; w <= window && duplicate; w++)
                {
                    if (!current.Equals(src[srcOffset + i - w]))
                    {
                        duplicate = false;
                        break;
                    }
                }

                if (!duplicate)
                {
                    if (dst != null)
                        dst[dstOffset + j] = src[srcOffset + i];
                    j++;
                }
            }

            return j;
        }
    }
}
