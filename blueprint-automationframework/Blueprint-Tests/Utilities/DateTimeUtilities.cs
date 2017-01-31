using System;
using System.Globalization;

namespace Utilities
{
    public static class DateTimeUtilities
    {
        /// <summary>
        /// Compares if two DateTimes are within a specified number of milliseconds of each other.
        /// </summary>
        /// <param name="firstTime">The first time to compare.</param>
        /// <param name="secondTime">The second time to compare.</param>
        /// <param name="milliseconds">The +/- range of milliseconds to compare with.</param>
        /// <returns>True if the two DateTimes are within +/- the specified number of seconds of each other.</returns>
        public static bool CompareTimePlusOrMinusMilliseconds(this DateTime firstTime, DateTime secondTime, double milliseconds)
        {
            var timeSpan = new TimeSpan(Math.Abs(firstTime.Ticks - secondTime.Ticks));
            return (timeSpan.TotalMilliseconds <= milliseconds);
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

        /// <summary>
        /// Converts a datetime to a sortable datetime
        /// </summary>
        /// <param name="dateTimeString">A datetime value.</param>
        /// <returns>A sortable datetime value</returns>
        public static string ConvertDateTimeToSortableDateTime(string dateTimeString)
        {
            var dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture);
            return ConvertDateTimeToSortableDateTime(dateTime);
        }
    }
}
