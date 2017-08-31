using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using ServiceLibrary.Models;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers
{
    public static class PropertyHelper
    {
        private static readonly char NewLine = '\n';
        private static readonly char GroupPrefix = 'g';
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
        
        public static DateTime ParseDateValue(string dateValue, ITimeProvider timeProvider)
        {
            //specific date
            const string dateFormat = WorkflowConstants.Iso8601DateFormat;
            DateTime date;
            if (DateTime.TryParseExact(dateValue, dateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
            {
                return date;
            }

            //today + days
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
    }
}