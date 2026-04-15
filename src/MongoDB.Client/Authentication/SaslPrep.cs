using System.Globalization;
using System.Text;
using MongoDB.Client.Exceptions;

namespace MongoDB.Client.Authentication
{
    internal static class SaslPrep
    {
        public static string Prepare(string value)
        {
            if (value.Length == 0)
            {
                return value;
            }

            var mapped = new StringBuilder(value.Length);
            foreach (var rune in value.EnumerateRunes())
            {
                if (ShouldMapToNothing(rune))
                {
                    continue;
                }

                mapped.Append(ShouldMapToSpace(rune) ? " " : rune.ToString());
            }

            var mappedString = mapped.ToString();
            EnsureNoUnicode32UnassignedCodePoints(mappedString);
            var normalized = ApplyUnicode32NormalizationCorrections(mappedString).Normalize(NormalizationForm.FormKC);
            EnsureNoProhibitedCharacters(normalized);
            EnsureBidirectionalRules(normalized);
            return normalized;
        }

        private static bool ShouldMapToNothing(Rune rune)
        {
            return rune.Value is 0x00AD
                or 0x034F
                or 0x1806
                or 0x180B
                or 0x180C
                or 0x180D
                or 0x200C
                or 0x200D
                or 0x200B
                or 0x2060
                or 0xFE00
                or 0xFE01
                or 0xFE02
                or 0xFE03
                or 0xFE04
                or 0xFE05
                or 0xFE06
                or 0xFE07
                or 0xFE08
                or 0xFE09
                or 0xFE0A
                or 0xFE0B
                or 0xFE0C
                or 0xFE0D
                or 0xFE0E
                or 0xFE0F
                or 0xFEFF;
        }

        private static bool ShouldMapToSpace(Rune rune)
        {
            return rune.Value is 0x00A0
                or 0x1680
                or 0x2000
                or 0x2001
                or 0x2002
                or 0x2003
                or 0x2004
                or 0x2005
                or 0x2006
                or 0x2007
                or 0x2008
                or 0x2009
                or 0x200A
                or 0x202F
                or 0x205F
                or 0x3000;
        }

        private static void EnsureNoProhibitedCharacters(string value)
        {
            foreach (var rune in value.EnumerateRunes())
            {
                var category = Rune.GetUnicodeCategory(rune);
                if (category is UnicodeCategory.Control
                    or UnicodeCategory.Surrogate
                    or UnicodeCategory.PrivateUse
                    or UnicodeCategory.OtherNotAssigned
                    or UnicodeCategory.LineSeparator
                    or UnicodeCategory.ParagraphSeparator)
                {
                    ThrowHelper.MongoAuthentificationException(
                        $"SCRAM-SHA-256 password contains a prohibited Unicode code point U+{rune.Value:X4}.",
                        0);
                }

                if (rune.Value is >= 0xFFF9 and <= 0xFFFD
                    or >= 0x2FF0 and <= 0x2FFB
                    or >= 0x0340 and <= 0x0341
                    or 0x06DD
                    or 0x070F
                    or 0x180E
                    or >= 0x200E and <= 0x200F
                    or >= 0x202A and <= 0x202E
                    or >= 0x2061 and <= 0x2063
                    or >= 0x206A and <= 0x206F
                    or >= 0x1D173 and <= 0x1D17A
                    or 0xE0001
                    or >= 0xE0020 and <= 0xE007F)
                {
                    ThrowHelper.MongoAuthentificationException(
                        $"SCRAM-SHA-256 password contains a prohibited Unicode code point U+{rune.Value:X4}.",
                        0);
                }
            }
        }

        private static void EnsureNoUnicode32UnassignedCodePoints(string value)
        {
            foreach (var rune in value.EnumerateRunes())
            {
                if (Unicode32AssignedCodePoints.WasUnassignedInUnicode32(rune))
                {
                    ThrowHelper.MongoAuthentificationException(
                        $"SCRAM-SHA-256 password contains a prohibited Unicode code point U+{rune.Value:X4}.",
                        0);
                }
            }
        }

        private static string ApplyUnicode32NormalizationCorrections(string value)
        {
            var corrected = new StringBuilder(value.Length);
            foreach (var rune in value.EnumerateRunes())
            {
                corrected.Append(Unicode32NormalizationCorrections.TryGetReplacement(rune, out var replacement)
                    ? replacement.ToString()
                    : rune.ToString());
            }

            return corrected.ToString();
        }

        private static void EnsureBidirectionalRules(string value)
        {
            var containsRandALCat = false;
            var containsLCat = false;
            Rune? firstRune = null;
            Rune lastRune = default;

            foreach (var rune in value.EnumerateRunes())
            {
                firstRune ??= rune;
                lastRune = rune;

                if (Unicode32BidirectionalTables.IsRandALCat(rune))
                {
                    containsRandALCat = true;
                }

                if (Unicode32BidirectionalTables.IsLCat(rune))
                {
                    containsLCat = true;
                }
            }

            if (!containsRandALCat)
            {
                return;
            }

            if (containsLCat)
            {
                ThrowHelper.MongoAuthentificationException(
                    "SCRAM-SHA-256 password violates SASLprep bidirectional rules: RandALCat and LCat characters must not be mixed.",
                    0);
            }

            if (firstRune is null
                || !Unicode32BidirectionalTables.IsRandALCat(firstRune.Value)
                || !Unicode32BidirectionalTables.IsRandALCat(lastRune))
            {
                ThrowHelper.MongoAuthentificationException(
                    "SCRAM-SHA-256 password violates SASLprep bidirectional rules: a password containing RandALCat characters must start and end with RandALCat characters.",
                    0);
            }
        }
    }
}
