using System;

namespace SsdeepNET
{
    /// <summary>
    /// Computes the ssdeep fuzzy hash for the input data.
    /// </summary>
    public interface IFuzzyHash
    {
        /// <summary>
        /// Computes the fuzzy hash value for the specified bytes span.
        /// </summary>
        /// <param name="span">The input to compute the hash for.</param>
        /// <returns>The computed hash.</returns>
        string ComputeHash(ReadOnlySpan<byte> span);

        /// <summary>
        /// Given two hash strings computes a value indicating the degree to which they match.
        /// </summary>
        /// <returns>
        /// A value from 0 to 100 indicating the match score of the two signatures.
        /// </returns>
        int CompareHashes(string x, string y);
    }
}
