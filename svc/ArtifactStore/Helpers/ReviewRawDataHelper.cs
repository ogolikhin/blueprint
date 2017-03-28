using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ArtifactStore.Helpers
{
    internal static class ReviewRawDataHelper
    {
        public static IEnumerable<int> ExtractReviewReviewers(string rawData)
        {
            List<int> reviewers = new List<int>();
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var matches = Regex.Matches(rawData, "<UserId[^>]*>(.+?)</UserId\\s*>", RegexOptions.IgnoreCase);
                foreach (Match match in matches)
                {
                    if (match.Groups.Count > 1)
                    {
                        reviewers.Add(Convert.ToInt32(match.Groups[1].Value, new CultureInfo("en-CA", true)));
                    }
                }
            }
            return reviewers;
        }

        public static int ExtractReviewStatus(string rawData)
        {
            int status = 0;
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var matches = Regex.Matches(rawData, "<Status[^>]*>(.+?)</Status\\s*>", RegexOptions.IgnoreCase);
                if (matches.Count > 0 && matches[0].Groups.Count > 1)
                {
                    if (string.Compare(matches[0].Groups[1].Value, "Active", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        status = 1;
                    }
                    else if (string.Compare(matches[0].Groups[1].Value, "Closed", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        status = 2;
                    }
                }
            }
            return status;
        }

        public static DateTime? ExtractReviewEndDate(string rawData)
        {
            DateTime? endDate = null;
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var match = Regex.Match(rawData, "<EndDate[^>]*>(.+?)</EndDate\\s*>", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string dateStr = match.Groups[1].Value;
                    DateTime date;
                    var successfulParse = DateTime.TryParse(dateStr, out date);

                    if (successfulParse)
                    {
                        endDate = date;
                    }
                }
            }
            return endDate;
        }
    }
}