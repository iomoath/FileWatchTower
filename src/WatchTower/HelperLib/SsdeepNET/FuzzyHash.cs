using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace SsdeepNET
{
    /// <summary>
    /// Computes the ssdeep fuzzy hash for the input data.
    /// </summary>
    public sealed class FuzzyHash : IFuzzyHash
    {
        readonly FuzzyHashAlgorithm _engine;

        public FuzzyHash(FuzzyHashFlags hashMode = FuzzyHashFlags.None)
        {
            _engine = new FuzzyHashAlgorithm(hashMode);
        }

        /// <summary>
        /// Gets the fuzzy hash flags.
        /// </summary>
        public FuzzyHashFlags Flags => _engine.Flags;

        /// <summary>
        /// Returns the compatible <see cref="HashAlgorithm"/> instance.
        /// </summary>
        public HashAlgorithm AsHashAlgorithm() => _engine;

        /// <inheritdoc />
        public string ComputeHash(ReadOnlySpan<byte> span)
        {
            _engine.HashCore(span);
            var hash = _engine.HashFinalCore();
            _engine.Initialize();
            if (hash.Array != null)
                return Encoding.ASCII.GetString(hash.Array, hash.Offset, hash.Count);
            return null;
        }

        /// <inheritdoc />
        public int CompareHashes(string x, string y)
        {
            if (x is null)
                throw new ArgumentNullException(nameof(x));
            if (y is null)
                throw new ArgumentNullException(nameof(y));

            // Each spamsum is prefixed by its block size.
            var colon1Pos = x.IndexOf(':');
            var colon2Pos = y.IndexOf(':');
            if (colon1Pos == -1 || colon2Pos == -1 ||
                !int.TryParse(x.Substring(0, colon1Pos), out var blockSize1) ||
                !int.TryParse(y.Substring(0, colon2Pos), out var blockSize2) ||
                blockSize1 < 0 || blockSize2 < 0)
            {
                throw new ArgumentException("Badly formed input.");
            }

            // If the blocksizes don't match then we are comparing apples to oranges.
            // This isn't an 'error' per se. We could have two valid signatures, but they can't be compared.
            if (blockSize1 != blockSize2 && blockSize1 != blockSize2 * 2 && blockSize2 != blockSize1 * 2)
            {
                return 0;
            }

            var colon12Pos = x.IndexOf(':', colon1Pos + 1);
            var colon22Pos = y.IndexOf(':', colon2Pos + 1);
            if (colon12Pos == -1 || colon22Pos == -1)
            {
                throw new ArgumentException("Badly formed input.");
            }

            // Chop the second string at the comma--just before the filename.
            // If the strings don't have a comma (i.e. don't have a filename)
            // that's ok. It's not an error. This function can be called on
            // signatures which don't have filenames attached.
            // We also don't have to advance past the comma however. We don't care
            // about the filename
            var comma1Pos = x.IndexOf(',', colon12Pos + 1);
            var comma2Pos = y.IndexOf(',', colon22Pos + 1);

            var s1_1 = x.ToCharArray(colon1Pos + 1, colon12Pos - colon1Pos - 1);
            var s2_1 = y.ToCharArray(colon2Pos + 1, colon22Pos - colon2Pos - 1);

            var s1_2 = x.ToCharArray(colon12Pos + 1, comma1Pos == -1 ? x.Length - colon12Pos - 1 : comma1Pos - colon12Pos - 1);
            var s2_2 = y.ToCharArray(colon22Pos + 1, comma2Pos == -1 ? y.Length - colon22Pos - 1 : comma2Pos - colon22Pos - 1);

            if (s1_1.Length is 0 || s2_1.Length is 0 || s1_2.Length is 0 || s2_2.Length is 0)
            {
                throw new ArgumentException("Badly formed input.");
            }

            // There is very little information content is sequences of the same character like 'LLLLL'.
            // Eliminate any sequences longer than 3. This is especially important when combined
            // with the has_common_substring() test below.
            s1_1 = EliminateSequences(s1_1);
            s2_1 = EliminateSequences(s2_1);
            s1_2 = EliminateSequences(s1_2);
            s2_2 = EliminateSequences(s2_2);

            // Now that we know the strings are both well formed, are they identical?
            // We could save ourselves some work here.
            if (blockSize1 == blockSize2 && s1_1.Length == s2_1.Length)
            {
                bool matched = true;
                for (int i = 0; i < s1_1.Length; i++)
                {
                    if (s1_1[i] != s2_1[i])
                    {
                        matched = false;
                        break;
                    }
                }

                if (matched)
                {
                    return 100;
                }
            }

            // Each signature has a string for two block sizes. We now choose how to combine the two block sizes.
            // We checked above that they have at least one block size in common.
            if (blockSize1 == blockSize2)
            {
                return Math.Max(
                    ScoreStrings(s1_1, s2_1, blockSize1),
                    ScoreStrings(s1_2, s2_2, blockSize1 * 2));
            }
            else if (blockSize1 == blockSize2 * 2)
            {
                return ScoreStrings(s1_1, s2_2, blockSize1);
            }
            else
            {
                return ScoreStrings(s1_2, s2_1, blockSize2);
            }
        }

        /// <summary>
        /// Eliminate sequences of longer than 3 identical characters.
        /// These sequences contain very little information so they tend to just bias
        /// the result unfairly.
        /// </summary>
        private static char[] EliminateSequences(char[] str)
        {
            var newLength = BufferUtilities.EliminateSequences(str, 0, null, 0, str.Length, Constants.SequencesToEliminateLength);
            if (newLength == str.Length)
                return str;

            var newStr = new char[newLength];
            BufferUtilities.EliminateSequences(str, 0, newStr, 0, str.Length, Constants.SequencesToEliminateLength);
            return newStr;
        }

        /// <summary>
        /// This is the low level string scoring algorithm. It takes two strings
        /// and scores them on a scale of 0-100 where 0 is a terrible match and
        /// 100 is a great match. The block_size is used to cope with very small
        /// messages.
        /// </summary>
        private static int ScoreStrings(char[] s1, char[] s2, int blockSize)
        {
            var len1 = s1.Length;
            var len2 = s2.Length;

            if (len1 > Constants.SpamSumLength || len2 > Constants.SpamSumLength)
            {
                // Not a real spamsum signature?
                return 0;
            }

            // The two strings must have a common substring of length
            // ROLLING_WINDOW to be candidates.
            if (!HasCommonSubstring(s1, s2))
                return 0;

            // Compute the edit distance between the two strings. The edit distance gives
            // us a pretty good idea of how closely related the two strings are.
            var score = EditDistance.Compute(s1, s2);

            // Scale the edit distance by the lengths of the two
            // strings. This changes the score to be a measure of the
            // proportion of the message that has changed rather than an
            // absolute quantity. It also copes with the variability of
            // the string lengths.
            score = (score * Constants.SpamSumLength) / (len1 + len2);

            // At this stage the score occurs roughly on a 0-64 scale,
            // with 0 being a good match and 64 being a complete
            // mismatch.

            // Rescale to a 0-100 scale (friendlier to humans).
            score = (100 * score) / 64;

            // It is possible to get a score above 100 here, but it is a
            // really terrible match.
            if (score >= 100)
                return 0;

            // Now re-scale on a 0-100 scale with 0 being a poor match and
            // 100 being a excellent match.
            score = 100 - score;

            // When the blocksize is small we don't want to exaggerate the match size.
            var matchSize = blockSize / Constants.MinBlocksize * Math.Min(len1, len2);
            if (score > matchSize)
                score = matchSize;
            return score;
        }

        /// <summary>
        /// We only accept a match if we have at least one common substring in
        /// the signature of length ROLLING_WINDOW. This dramatically drops the
        /// false positive rate for low score thresholds while having
        /// negligable affect on the rate of spam detection.
        /// </summary>
        /// <returns>True if the two strings do have a common substring, false otherwise.</returns>
        private static bool HasCommonSubstring(char[] s1, char[] s2)
        {
            var hashes = new List<uint>(Constants.SpamSumLength);

            // There are many possible algorithms for common substring
            // detection. In this case I am re-using the rolling hash code
            // to act as a filter for possible substring matches.

            // First compute the windowed rolling hash at each offset in
            // the first string.
            var state = new Roll();

            for (int i = 0; i < s1.Length; i++)
            {
                state.Hash((byte)s1[i]);
                hashes.Add(state.Sum);
            }

            state = new Roll();

            // Now for each offset in the second string compute the
            // rolling hash and compare it to all of the rolling hashes
            // for the first string. If one matches then we have a
            // candidate substring match. We then confirm that match with
            // a direct string comparison.
            for (int i = 0; i < s2.Length; i++)
            {
                state.Hash((byte)s2[i]);
                uint h = state.Sum;
                if (i < Constants.RollingWindow - 1)
                    continue;
                for (int j = Constants.RollingWindow - 1; j < hashes.Count; j++)
                {
                    if (hashes[j] != 0 && hashes[j] == h)
                    {
                        // We have a potential match - confirm it.
                        var s2StartPos = i - Constants.RollingWindow + 1;
                        int len = 0;
                        while (len + s2StartPos < s2.Length)
                            len++;
                        if (len < Constants.RollingWindow)
                            continue;

                        var matched = true;
                        var s1StartPos = j - Constants.RollingWindow + 1;
                        for (int pos = 0; pos < Constants.RollingWindow; pos++)
                        {
                            var s1char = s1[s1StartPos + pos];
                            var s2char = s2[s2StartPos + pos];
                            if (s1char != s2char)
                            {
                                matched = false;
                                break;
                            }
                        }

                        if (matched)
                            return true;
                    }
                }
            }

            return false;
        }
    }
}
