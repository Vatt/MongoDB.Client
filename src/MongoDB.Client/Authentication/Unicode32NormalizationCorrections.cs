using System.Text;

namespace MongoDB.Client.Authentication
{
    internal static class Unicode32NormalizationCorrections
    {
        public static bool TryGetReplacement(Rune rune, out Rune replacement)
        {
            replacement = rune.Value switch
            {
                0x2F868 => new Rune(0x2136A),
                0x2F874 => new Rune(0x5F33),
                0x2F91F => new Rune(0x43AB),
                0x2F95F => new Rune(0x7AAE),
                0x2F9BF => new Rune(0x4D57),
                _ => default
            };

            return replacement.Value != 0;
        }
    }
}
