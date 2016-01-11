using System;
using System.Data;
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

        public static DataTable CreateDataTableInvariant()
        {
            return new DataTable { Locale = CultureInfo.InvariantCulture };
        }

        #endregion Object creation

        #region Format methods

        public static string FormatInvariant(string format, params object[] args)
        {
            return string.Format(CultureInfo.InvariantCulture, format, args);
        }

        #endregion Format methods

        #region Parse methods

        public static int IntParseInvariant(string s)
        {
            return int.Parse(s, CultureInfo.InvariantCulture);
        }

        public static DateTime DateTimeParseExactInvariant(string s, string format)
        {
            return DateTime.ParseExact(s, format, CultureInfo.InvariantCulture);
        }

        #endregion Parse methods

        #region Convert methods

        public static string ToStringInvariant(this IFormattable value, string format = null)
        {
            return value.ToString(format, CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(object value)
        {
            return Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        public static int ToInt32Invariant(object value)
        {
            return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        }

        #endregion Convert methods
    }
}
