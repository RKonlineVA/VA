csharp src\EDI275AttachmentParser\Services\SnipValidator.cs
using System.Linq;
using System.Text.RegularExpressions;

namespace EDI275AttachmentParser.Services
{
    public class SnipValidator : ISnipValidator
    {
        // Default: 9 digits
        private static readonly Regex _snipRegex = new(@"^\d{9}$", RegexOptions.Compiled);

        public bool IsValid(string? snip)
        {
            if (string.IsNullOrWhiteSpace(snip))
                return false;

            var normalized = Normalize(snip);
            return _snipRegex.IsMatch(normalized);
        }

        public string Normalize(string? snip)
        {
            if (string.IsNullOrWhiteSpace(snip))
                return string.Empty;

            // Strip non-digits
            var digits = new string(snip.Where(char.IsDigit).ToArray());
            return digits;
        }
    }
}