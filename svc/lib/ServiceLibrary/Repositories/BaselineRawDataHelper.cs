using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using ServiceLibrary.Helpers;

namespace BluePrintSys.RC.Service.Business.Baselines.Impl
{
    public static class BaselineRawDataHelper
    {
        public static DateTime? ExtractSnapTime(string rawData)
        {
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var matches = Regex.Matches(rawData, "<Snaptime[^>]*>(.+?)</Snaptime\\s*>", RegexOptions.IgnoreCase);
                if (matches.Count > 0 
                    && matches[0].Groups.Count > 1 
                    && matches[0].Groups[1].Value != null)
                {
                    var dateTime = DateTime.Parse(matches[0].Groups[1].Value, CultureInfo.InvariantCulture);
                    if (dateTime != null)
                    {
                        return dateTime;
                    }
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

        public static ISet<int> ExtractBaselineArtifacts(string rawData)
        {
            if (!string.IsNullOrWhiteSpace(rawData))
            {
                var matches = Regex.Matches(rawData, "<IncludedArtifactIdsAsString[^>]*>(.+?)</IncludedArtifactIdsAsString\\s*>", RegexOptions.IgnoreCase);
                if (matches.Count > 0
                    && matches[0].Groups.Count > 1
                    && matches[0].Groups[1].Value != null)
                {
                    var baselineArtifacts = GetIncludedArtifactIds(matches[0].Groups[1].Value);
                    if (baselineArtifacts != null)
                    {
                        return baselineArtifacts;
                    }
                }
            }
            return new HashSet<int>();
        }
    }
}
