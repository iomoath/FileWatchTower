using System;

namespace SsdeepNET
{
    /// <summary>
    /// Levenshtein distance calculator.
    /// </summary>
    internal static class EditDistance
    {
        //
        // Modified levenshtein distance calculation
        //
        // This program can be used, redistributed or modified under any of
        // Boost Software License 1.0, GPL v2 or GPL v3
        // See the file COPYING for details.
        //
        // Copyright (C) 2014 kikairoya <kikairoya@gmail.com>
        // Copyright (C) 2014 Jesse Kornblum <research@jessekornblum.com>
        //

        const int MaxLength = 64;
        const int InsertCost = 1;
        const int RemoveCost = 1;
        const int ReplaceCost = 2;

        public static int Compute(char[] s1, char[] s2)
        {
            var t0 = new int[MaxLength + 1];
            var t1 = new int[MaxLength + 1];

            for (var i = 0; i <= s2.Length; i++)
                t0[i] = i;

            for (var i = 0; i < s1.Length; i++)
            {
                t1[0] = i + 1;
                for (var j = 0; j < s2.Length; j++)
                {
                    var cost_a = t0[j + 1] + InsertCost;
                    var cost_d = t1[j] + RemoveCost;
                    var cost_r = t0[j] + (s1[i] == s2[j] ? 0 : ReplaceCost);
                    t1[j + 1] = Math.Min(Math.Min(cost_a, cost_d), cost_r);
                }

                var tmp = t0;
                t0 = t1;
                t1 = tmp;
            }
            return t0[s2.Length];
        }
    }
}