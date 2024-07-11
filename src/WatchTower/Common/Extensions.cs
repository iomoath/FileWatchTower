namespace WatchTower
{
    public static class Extensions
    {
        public static string NormalizeLineEndings(this string input, string newLine = "\n")
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Replace Windows and Mac line endings with the specified new line ending
            return input.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", newLine);
        }
    }
}
