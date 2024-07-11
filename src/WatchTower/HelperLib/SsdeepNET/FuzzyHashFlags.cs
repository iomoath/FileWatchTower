using System;

namespace SsdeepNET
{
    /// <summary>
    /// Fuzzy hash flags.
    /// </summary>
    [Flags]
    public enum FuzzyHashFlags
    {
        /// <summary>
        /// None.
        /// </summary>
        None = 0,

        /// <summary>
        /// Eliminate sequences of more than three identical characters.
        /// </summary>
        EliminateSequences = 1 << 0,

        /// <summary>
        /// Do not truncate the second part of hash to 32 characters.
        /// </summary>
        DoNotTruncate = 1 << 1,
    }
}
