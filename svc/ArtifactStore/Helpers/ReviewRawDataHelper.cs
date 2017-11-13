﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace ArtifactStore.Helpers
{
    internal static class ReviewRawDataHelper
    {
        public static IEnumerable<int> ExtractReviewReviewers(string rawData)
        {
            var reviewers = new List<int>();

            if (string.IsNullOrWhiteSpace(rawData))
            {
                return reviewers;
            }

            var matches = Regex.Matches(rawData, "<UserId[^>]*>(.+?)</UserId\\s*>", RegexOptions.IgnoreCase);

            foreach (Match match in matches)
            {
                if (match.Groups.Count > 1)
                {
                    reviewers.Add(Convert.ToInt32(match.Groups[1].Value, new CultureInfo("en-CA", true)));
                }
            }

            return reviewers;
        }

        public static int ExtractReviewStatus(string rawData)
        {
            var status = 0;

            if (string.IsNullOrWhiteSpace(rawData))
            {
                return status;
            }

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

            return status;
        }

        public static DateTime? ExtractReviewEndDate(string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData))
            {
                return null;
            }

            var match = Regex.Match(rawData, "<EndDate[^>]*>(.+?)</EndDate\\s*>", RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            var dateStr = match.Groups[1].Value;
            DateTime date;

            if (!DateTime.TryParse(dateStr, out date))
            {
                return null;
            }

            return date;
        }

        public static bool? ExtractBooleanProperty(string elementName, string rawData)
        {
            if (string.IsNullOrWhiteSpace(rawData))
            {
                return null;
            }

            var match = Regex.Match(rawData, string.Format(CultureInfo.InvariantCulture, "<{0}[^>]*>(.+?)</{0}\\s*>", elementName), RegexOptions.IgnoreCase);
            if (!match.Success)
            {
                return null;
            }

            var valueString = match.Groups[1].Value;
            bool value;

            if (!bool.TryParse(valueString, out value))
            {
                return null;
            }

            return value;
        }

        /// <summary>
        /// Extracts serialized string from raw data object
        /// </summary>
        /// <param name="rawData"></param>
        /// <typeparam name="TRawData"></typeparam>
        /// <returns></returns>
        public static string GetStoreData<TRawData>(TRawData rawData)
        {
            var serializer = new DataContractSerializer(typeof(TRawData));

            var result = new StringBuilder();
            using (var xmlWriter = XmlWriter.Create(result))
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
        /// <typeparam name="TRawData"></typeparam>
        public static bool TryRestoreData<TRawData>(string rawDataValue, out TRawData rawDataObject) where TRawData : class
        {
            rawDataObject = null;

            if (string.IsNullOrEmpty(rawDataValue))
            {
                return false;
            }

            try
            {
                rawDataObject = RestoreData<TRawData>(rawDataValue);
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
        /// <typeparam name="TRawData"></typeparam>
        public static TRawData RestoreData<TRawData>(string rawDataValue) where TRawData : class
        {
            if (string.IsNullOrEmpty(rawDataValue))
            {
                return null;
            }

            var serializer = new DataContractSerializer(typeof(TRawData));

            using (var xmlReaderCreate = XmlReader.Create(new StringReader(rawDataValue)))
            {
                return serializer.ReadObject(xmlReaderCreate) as TRawData;
            }
        }
    }
}
