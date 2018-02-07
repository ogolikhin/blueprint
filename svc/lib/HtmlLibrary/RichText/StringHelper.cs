using System;
using System.Linq;

namespace HtmlLibrary.RichText
{
    public static class StringHelper
    {
        public const string SpaceConstant = " ";

        public static string ReplaceNewLines(this string text, string newLineReplacement)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var toReplace = new[] { Environment.NewLine, "\r\n", "\n", "\r" };

            return ReplaceStrings(text, toReplace, newLineReplacement);
        }

        private static string ReplaceStrings(this string text, string[] toReplace,
            string newLineReplacement = SpaceConstant)
        {
            if (string.IsNullOrWhiteSpace(text) || toReplace == null || !toReplace.Any())
            {
                return text;
            }

            var strings = text.Split(toReplace, StringSplitOptions.RemoveEmptyEntries);

            return strings.Aggregate(string.Empty, (current, str) => current + (str + newLineReplacement));
        }
    }
}
