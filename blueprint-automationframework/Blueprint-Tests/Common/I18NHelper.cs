using System;
using System.Globalization;
using System.IO;

namespace Common
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

        #endregion Parse methods

        #region Compare methods

        public static bool StartsWithOrdinal(this string s, string value)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.StartsWith(value, StringComparison.Ordinal);
        }

        /// <summary>
        /// Determines whether this string ends with the specified string.
        /// </summary>
        /// <param name="s">The string whose contents to check.</param>
        /// <param name="value">The string to search for.</param>
        /// <returns>True if this string ends with the specified value.</returns>
        public static bool EndsWithOrdinal(this string s, string value)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.EndsWith(value, StringComparison.Ordinal);
        }

        public static bool EqualsOrdinalIgnoreCase(this string s, string value)
        {
            if (s == null)
            {
                throw new ArgumentNullException("s");
            }
            return s.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        #endregion Compare methods

        #region Convert methods

        public static string ToStringInvariant(this IFormattable value, string format = null)
        {
            if (value == null)
            {
                throw new ArgumentNullException("value");
            }
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(object value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static int ToInt32Invariant(this object value)
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        #endregion Convert methods
    }
}
