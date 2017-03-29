using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

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
    }
}
