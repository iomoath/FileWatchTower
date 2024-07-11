namespace SsdeepNET
{
    internal static class Constants
    {
        public const int RollingWindow = 7;
        public const int MinBlocksize = 3;
        public const int NumBlockhashes = 31;
        public const int SpamSumLength = 64;
        public const int MaxResultLength = 2 * SpamSumLength + 20;
        public const int SequencesToEliminateLength = 3;
    }
}
