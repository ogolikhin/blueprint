using System;
using System.Globalization;
using System.IO;

namespace ServiceLibrary.Helpers
{
    /// <summary>
    /// Contains wrapper methods useful for resolving Globalization Code Analysis warnings, such as CA1305.
    /// </summary>
    public static class I18NHelper
    {
        #region Object creation

        public static StringWriter CreateStringWriterInvariant()
        {
            return new StringWriter(CultureInfo.InvariantCulture);
        }

        #endregion Object creation

        #region Format methods

        public static string FormatInvariant(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        #endregion Format methods

        #region Parse methods

        public static int Int32ParseInvariant(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        public static DateTime DateTimeParseExactInvariant(string s, string format)
        {
            return DateTime.ParseExact(s, format, CultureInfo.InvariantCulture);
        }

        public static DateTimeOffset DateTimeOffsetParseInvariant(string s)
        {
            return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
        }

        public static string DateTimeParseToIso8601Invariant(DateTime dateTime)
        {
            return dateTime.ToString("o", CultureInfo.InvariantCulture);
        }

        #endregion Parse methods

        #region Compare methods

        public static bool EndsWithOrdinal(this string s, string value)
        {
            ThrowIf.ArgumentNull(s, nameof(s));
            return s.EndsWith(value, StringComparison.Ordinal);
        }

        public static bool EqualsOrdinalIgnoreCase(this string s, string value)
        {
            ThrowIf.ArgumentNull(s, nameof(s));
            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Compare methods

        #region Convert methods

        public static string ToStringInvariant(this IFormattable value, string format = null)
        {
            ThrowIf.ArgumentNull(value, nameof(value));
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static int ToInt32(this string value, int defValue = default(int))
        {
            int result;
            if (Int32.TryParse(value, out result))
                return result;
            return defValue;
        }


        #endregion Convert methods
    }
}
