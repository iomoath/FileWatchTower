using System;
using System.IO;

namespace SsdeepNET
{
    /// <summary>
    /// <see cref="FuzzyHash"/> extensions.
    /// </summary>
    internal static class FuzzyHashExtensions
    {
        /// <summary>
        /// Computes the fuzzy hash for the specified <see cref="Stream"/> object.
        /// </summary>
        /// <param name="fuzzyHash"><see cref="IFuzzyHash"/> instance.</param>
        /// <param name="inputStream">The input to compute the hash for.</param>
        /// <returns>The computed hash.</returns>
        public static string ComputeHash(this IFuzzyHash fuzzyHash, Stream inputStream)
        {
            if (fuzzyHash is null)
                throw new ArgumentNullException(nameof(fuzzyHash));
            if (inputStream is null)
                throw new ArgumentNullException(nameof(inputStream));

            if (fuzzyHash is FuzzyHash fh)
            {
                return ComputeHash(fh, inputStream);
            }
            else
            {
                byte[] data;
                using (var ms = new MemoryStream())
                {
                    inputStream.CopyTo(ms);
                    data = ms.ToArray();
                }

                return fuzzyHash.ComputeHash(data);
            }
        }

        /// <summary>
        /// Computes the fuzzy hash for the specified <see cref="Stream"/> object.
        /// </summary>
        /// <param name="fuzzyHash"><see cref="FuzzyHash"/> instance.</param>
        /// <param name="inputStream">The input to compute the hash for.</param>
        /// <returns>The computed hash.</returns>
        public static string ComputeHash(this FuzzyHash fuzzyHash, Stream inputStream)
        {
            if (fuzzyHash is null)
                throw new ArgumentNullException(nameof(fuzzyHash));
            if (inputStream is null)
                throw new ArgumentNullException(nameof(inputStream));

            const int bufferSize = 4096;

            byte[] buffer = new byte[bufferSize];
            int bytesRead;
            do
            {
                bytesRead = inputStream.Read(buffer, 0, bufferSize);
                if (bytesRead > 0)
                    fuzzyHash.AsHashAlgorithm().TransformBlock(buffer, 0, bytesRead, buffer, 0);
            } while (bytesRead > 0);

            return fuzzyHash.ComputeHash(Array.Empty<byte>());
        }

        /// <summary>
        /// Computes the fuzzy hash of the <paramref name="buffer"/>.
        /// </summary>
        public static string ComputeHash(this IFuzzyHash fuzzyHash, byte[] buffer) =>
            ComputeHash(fuzzyHash, buffer, 0, buffer?.Length ?? 0);

        /// <summary>
        /// Computes the fuzzy hash of the first <paramref name="length"/> bytes of the <paramref name="buffer"/>.
        /// </summary>
        public static string ComputeHash(this IFuzzyHash fuzzyHash, byte[] buffer, int length) =>
            ComputeHash(fuzzyHash, buffer, 0, length);

        /// <summary>
        /// Computes the fuzzy hash of the <paramref name="buffer"/>.
        /// </summary>
        public static string ComputeHash(this IFuzzyHash fuzzyHash, byte[] buffer, int offset, int length)
        {
            if (fuzzyHash is null)
                throw new ArgumentNullException(nameof(fuzzyHash));
            if (buffer is null)
                throw new ArgumentNullException(nameof(buffer));
            if (offset >= buffer.Length)
                throw new ArgumentException("Invalid offset.", nameof(offset));
            if (offset + length > buffer.Length)
                throw new ArgumentException("Invalid length.", nameof(length));

            var span = new ReadOnlySpan<byte>(buffer, offset, length);
            return fuzzyHash.ComputeHash(span);
        }
    }
}
