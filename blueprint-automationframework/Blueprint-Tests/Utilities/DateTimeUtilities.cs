using System;
using System.Globalization;

namespace Utilities
{
    public static class DateTimeUtilities
    {
        /// <summary>
        /// Compares if two DateTimes are within a specified number of seconds of each other.
        /// </summary>
        /// <param name="firstTime">The first time to compare.</param>
        /// <param name="secondTime">The second time to compare.</param>
        /// <param name="seconds">The +/- range of seconds to compare with.</param>
        /// <returns>True if the two DateTimes are within +/- the specified number of seconds of each other.</returns>
        public static bool CompareTimePlusOrMinus(this DateTime firstTime, DateTime secondTime, double seconds)
        {
            return ((firstTime.AddSeconds(seconds) > secondTime) && (firstTime < secondTime.AddSeconds(seconds)));
        }

        /// <summary>
        /// Converts a datetime to a sortable datetime
        /// </summary>
        /// <param name="dateTime">A datetime value.</param>
        /// <returns>A sortable datetime value</returns>
        public static string ConvertDateTimeToSortableDateTime(DateTime dateTime)
        {
            return dateTime.ToString(CultureInfo.InvariantCulture.DateTimeFormat.SortableDateTimePattern, CultureInfo.InvariantCulture);
        }
    }
}
