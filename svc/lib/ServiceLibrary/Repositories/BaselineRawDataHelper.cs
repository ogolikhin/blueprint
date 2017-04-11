using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using ServiceLibrary.Helpers;

namespace BluePrintSys.RC.Service.Business.Baselines.Impl
{
    public static class BaselineRawDataHelper
    {
        public static bool ExtractIsSelead(string rawData)
        {
            bool isSealed;
            if (bool.TryParse(ExtractPropertyValue(rawData, "IsSealed"), out isSealed))
            {
                return isSealed;
            }
            return false;
        }

        public static DateTime? ExtractSnapTime(string rawData)
        {
            var snaptimeValue = ExtractPropertyValue(rawData, "Snaptime");
            if (!string.IsNullOrWhiteSpace(snaptimeValue))
            {
                return DateTime.SpecifyKind(DateTime.Parse(snaptimeValue, CultureInfo.InvariantCulture), DateTimeKind.Utc);
            }
            return null;
        }

        public static ISet<int> ExtractBaselineArtifacts(string rawData)
        {
            var includedArtifactIdsAsString = ExtractPropertyValue(rawData, "IncludedArtifactIdsAsString");
            if (!string.IsNullOrWhiteSpace(includedArtifactIdsAsString))
            {
                var baselineArtifacts = GetIncludedArtifactIds(includedArtifactIdsAsString);
                if (baselineArtifacts != null)
                {
                    return baselineArtifacts;
                }
            }
            return new HashSet<int>();
        }

        private static string ExtractPropertyValue(string rawData, string propertyName)
        {
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var matches = Regex.Matches(rawData, string.Format(CultureInfo.InvariantCulture, "<{0}[^>]*>(.+?)</{0}\\s*>", propertyName), RegexOptions.IgnoreCase);
                if (matches.Count > 0
                    && matches[0].Groups.Count > 1)
                {
                    return matches[0].Groups[1].Value;
                }
            }
            return null;
        }

        private static HashSet<int> GetIncludedArtifactIds(string includedArtifactIdsAsString)
        {
            HashSet<int> includedArtifactIds;
            if (string.IsNullOrEmpty(includedArtifactIdsAsString))
            {
                includedArtifactIds = new HashSet<int>();
            }
            else
            {
                using (var reader = new ComplexObjectReader(includedArtifactIdsAsString))
                {
                    includedArtifactIds = reader.ReadCollection(new HashSet<int>(), r => r.ReadInt32());
                }
            }

            return includedArtifactIds;
        }
    }
}
