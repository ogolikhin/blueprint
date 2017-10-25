using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Text;
using System.IO;

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

        /// <summary>
        /// Extracts serialized string from raw data object
        /// </summary>
        /// <param name="rawData"></param>
        /// <typeparam name="RawDataType"></typeparam>
        /// <returns></returns>
        public static string GetStoreData<RawDataType>(RawDataType rawData)
        {
            var serializer = new DataContractSerializer(typeof(RawDataType));

            var result = new StringBuilder();
            using (XmlWriter xmlWriter = XmlWriter.Create(result))
            {
                serializer.WriteObject(xmlWriter, rawData);
            }
            return result.ToString();
        }

        /// <summary>
        /// Extracts data from serialized raw data
        /// Does not throw exceptions
        /// </summary>
        /// <param name="rawDataValue"></param>
        /// <param name="rawDataObject"></param>
        /// <typeparam name="RawDataType"></typeparam>
        public static bool TryRestoreData<RawDataType>(string rawDataValue, out RawDataType rawDataObject) where RawDataType : class
        {
            rawDataObject = null;
            if (string.IsNullOrEmpty(rawDataValue))
            {
                return false;
            }
            try
            {
                rawDataObject = RestoreData<RawDataType>(rawDataValue);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Extracts data from serialized raw data
        /// </summary>
        /// <param name="rawDataValue"></param>
        /// <typeparam name="RawDataType"></typeparam>
        public static RawDataType RestoreData<RawDataType>(string rawDataValue) where RawDataType : class
        {
            if (!string.IsNullOrEmpty(rawDataValue))
            {
                var serializer = new DataContractSerializer(typeof(RawDataType));

                using (XmlReader xmlReaderCreate = XmlReader.Create(new StringReader(rawDataValue)))
                {
                    return serializer.ReadObject(xmlReaderCreate) as RawDataType;
                }
            }
            return null;
        }
    }
}
