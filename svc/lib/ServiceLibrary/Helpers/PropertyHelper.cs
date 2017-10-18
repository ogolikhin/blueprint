using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers
{
    public static class PropertyHelper
    {
        private static readonly char NewLine = '\n';
        private static readonly char GroupPrefix = 'g';
        private static readonly char EmailSeparator = ';';

        internal const string NbSpace = "&nbsp;";
        internal const string NbTab = "&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;";
        /// <summary>
        /// Breakline tab
        /// </summary>
        public const string BrTag = "<br />";
        /// <summary>
        /// open paragraph tag
        /// </summary>
        public const string PStart = "<p>";
        /// <summary>
        /// close paragraph tag
        /// </summary>
        public const string PEnd = "</p>";
        internal const string Bold = "<b>";
        internal const string BoldClose = "</b>";
        /// <summary>
        /// open html+body tags
        /// </summary>
        public const string HtmlBody = "<html><body>";
        /// <summary>
        /// close html+body tags
        /// </summary>
        public const string HtmlBodyClose = "</body></html>";
        internal const string Html = "<html>";
        internal const string Body = "<body>";
        internal const string HtmlClose = "</html>";
        internal const string BodyClose = "</body>";

        internal const string BodyStartShort = "<body";
        internal const string DivStartShort = "<div";
        internal const string DivClose = "</div>";

        private static string[] _tags = { Bold, BoldClose, HtmlBody, HtmlBodyClose };

        internal static Regex BrRegex = new Regex(@"<\s*/*\s*BrTag\s*/*\s*>", RegexOptions.IgnoreCase);
        internal static Regex TdRegex = new Regex(@"<\s*/*\s*TD\s*/*\s*>", RegexOptions.IgnoreCase);
        internal static Regex TrRegex = new Regex(@"<\s*/*\s*TR\s*/*\s*>", RegexOptions.IgnoreCase);
        internal static Regex ParaRegex = new Regex(@"<\s*/\s*p\s*>", RegexOptions.IgnoreCase);

        // Converts database stored user and group values to objects
        public static List<UserGroup> ParseUserGroups(string userGroups)
        {
            if (string.IsNullOrWhiteSpace(userGroups))
                return null;

            var result = new List<UserGroup>();
            var tokens = userGroups.Split('\n');
            foreach (var token in tokens)
            {
                var isGroup = token.StartsWith("g", StringComparison.Ordinal);
                int id;
                if (int.TryParse(isGroup ? token.TrimStart('g') : token, out id))
                    result.Add(new UserGroup { Id = id, IsGroup = isGroup });
            }

            return result;
        }
        public static string ParseUserGroupsToString(List<UserGroup> userGroups)
        {
            if (userGroups.IsEmpty())
                return null;
            ICollection<string> values = null;
            foreach (UserGroup userGroup in userGroups)
            {
                if (values == null)
                {
                    values = new LinkedList<string>();
                }
                if (userGroup.IsGroup.GetValueOrDefault(false))
                {
                    values.Add(GroupPrefix + userGroup.Id.Value.ToString(NumberFormatInfo.InvariantInfo));
                }
                else
                {
                    values.Add(userGroup.Id.Value.ToString(NumberFormatInfo.InvariantInfo));
                }
            }
            return GetCanonicalSetString(values);
        }

        // Convert the byte array of the number property stored in the database to decimal.
        public static decimal? ToDecimal(byte[] value)
        {
            if (value == null)
            {
                return null;
            }
            int[] bits = { BitConverter.ToInt32(value, 0), BitConverter.ToInt32(value, 4), BitConverter.ToInt32(value, 8), BitConverter.ToInt32(value, 12) };
            return new decimal(bits);
        }
        public static byte[] GetBytes(decimal? value)
        {
            if (value == null)
            {
                return null;
            }
            byte[] bytes = new byte[16];
            int[] bits = Decimal.GetBits((decimal)value);
            Array.Copy(BitConverter.GetBytes(bits[0]), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(bits[1]), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(bits[2]), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(bits[3]), 0, bytes, 12, 4);
            return bytes;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        public static DateTime ParseDateValue(string dateValue, ITimeProvider timeProvider)
        {
            // specific date
            const string dateFormat = WorkflowConstants.Iso8601DateFormat;
            DateTime date;
            if (DateTime.TryParseExact(dateValue, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }

            // today + days
            int days;
            if (int.TryParse(dateValue, out days))
            {
                return timeProvider.Today.AddDays(days);
            }

            throw new FormatException($"Invalid date value: {dateValue}. The date format must be {dateFormat}, or an integer.");
        }

        private static string GetCanonicalSetString(ICollection<string> values)
        {
            if (values == null)
            {
                return null;
            }
            StringBuilder stringBuilder = null;
            foreach (string value in values.Where(v => (v != null)).OrderBy(v => v, StringComparer.Ordinal))
            {
                if (stringBuilder == null)
                {
                    stringBuilder = new StringBuilder(values.Count << 3);
                    stringBuilder.Append(NewLine);
                }
                stringBuilder.Append(value);
                stringBuilder.Append(NewLine);
            }
            return stringBuilder?.ToString();
        }

        public static IEnumerable<string> ParseEmails(string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return Enumerable.Empty<string>();
            }
            var propValue = ConvertHtmlToPlainText(propertyValue).Replace(Environment.NewLine, " ");
            return propValue.Split(new []{ EmailSeparator}, StringSplitOptions.RemoveEmptyEntries);
        }

        /// <summary>
        /// converts given html string to plain text.
        /// </summary>
        /// <param name="html"></param>
        /// <returns></returns>
        public static string ConvertHtmlToPlainText(string html)
        {
            if (html == null)
            {
                return null;
            }
            if (!ContainsHtmlTags(html))
            {
                return html;
            }
            // Convert all <Br> to new lines. We use regex to ensure that we do not miss any break tags
            html = BrRegex.Replace(html, BrTag);
            html = TrRegex.Replace(html, BrTag);
            html = ParaRegex.Replace(html, BrTag);
            html = TdRegex.Replace(html, NbTab);
            var plainText = new StringBuilder(html);
            plainText.Replace(BrTag, "\n");
            plainText.Replace(NbSpace, " ");
            plainText.Replace("&#x200b;", "");
            // Bug 68488: Resolve conflict. Remove all html tags like <html>, <div>, <span>
            return HttpUtility.HtmlDecode(new Regex("<[^>]*>", RegexOptions.IgnoreCase).Replace(plainText.ToString(), string.Empty).TrimEnd('\n'));
        }

        /// <summary>
        /// searches given string htmlString to see whether it contains any html tags.
        /// </summary>
        /// <param name="htmlString"></param>
        /// <returns></returns>
        public static bool ContainsHtmlTags(string htmlString)
        {
            if (string.IsNullOrWhiteSpace(htmlString))
            {
                return false;
            }
            string value = htmlString.Trim();
            if (value.StartsWith(Html, StringComparison.OrdinalIgnoreCase) && value.EndsWith(HtmlClose, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
    }
}