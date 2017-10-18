// Copyright (c) Microsoft Corporation. All rights reserved. 

using System;
using System.Text;
using System.Xml;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using ServiceLibrary.Helpers;
using System.Collections.Generic;

namespace Logging.Database.Utility
{
    internal static class EventEntryUtil
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2202:Do not dispose objects multiple times", Justification = "Opt out for closing output")]
        internal static string XmlSerializePayload(EventEntry entry)
        {
            try
            {
                var settings = new XmlWriterSettings()
                {
                    OmitXmlDeclaration = true // Do not add xml declaration
                };

                var writer = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(writer, settings))
                {
                    XmlWritePayload(xmlWriter, entry);
                    xmlWriter.Flush();
                    return writer.ToString();
                }
            }
            catch (Exception e)
            {
                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(e.ToString());

                return I18NHelper.FormatInvariant("<Error>{0}</Error>", I18NHelper.FormatInvariant("Cannot serialize to XML format the payload: {0}", e.Message));
            }
        }

        internal static void XmlWritePayload(XmlWriter writer, EventEntry entry)
        {
            writer.WriteStartElement("Payload");

            var eventSchema = entry.Schema;

            for (int i = 0; i < entry.Payload.Count; i++)
            {
                if (entry.Payload[i] != null)
                {
                    XmlWriteProperty(writer, eventSchema.Payload[i], entry.Payload[i]);
                }
            }

            writer.WriteEndElement();
        }

        private static void XmlWriteProperty(XmlWriter writer, string propertyName, object value)
        {
            try
            {
                // skip these columns as they are written to their own column in the DB
                var skip = new List<string>() { "IpAddress", "Source", "Message", "UserName", "SessionId", "OccurredAt", "ActionName", "CorrelationId", "Duration" };
                if (!skip.Contains(propertyName))
                {
                    writer.WriteElementString(propertyName, SanitizeXml(value));
                }
            }
            catch (Exception e)
            {
                SemanticLoggingEventSource.Log.CustomSinkUnhandledFault(e.ToString());

                // We are in Error state so abort the write operation
                throw new InvalidOperationException(I18NHelper.FormatInvariant("Cannot serialize to XML format the payload: {0}", e.Message), e);
            }
        }

        private static string SanitizeXml(object value)
        {
            var valueType = value.GetType();
            if (valueType == typeof(Guid))
            {
                return XmlConvert.ToString((Guid)value);
            }

            return valueType.IsEnum ? ((Enum)value).ToString("D") : value.ToString();
        }

        internal static string GetPayloadValue(EventEntry entry, string payloadItem)
        {
            var eventSchema = entry.Schema;

            for (int i = 0; i < entry.Payload.Count; i++)
            {
                if (eventSchema.Payload[i].EqualsOrdinalIgnoreCase(payloadItem))
                {
                    return entry.Payload[i] == null ? string.Empty : entry.Payload[i].ToString();
                }
            }

            return "";
        }

        // internal static DateTimeOffset GetTimestamp(EventEntry entry, string payloadItem)
        // {
        //    var eventSchema = entry.Schema;

        // for (int i = 0; i < entry.Payload.Count; i++)
        //    {
        //        if (eventSchema.Payload[i].EqualsOrdinalIgnoreCase(payloadItem))
        //        {
        //            return entry.Payload[i] == null ? entry.Timestamp : I18NHelper.DateTimeOffsetParseInvariant(entry.Payload[i].ToString());
        //        }
        //    }

        // return entry.Timestamp;
        // }
    }
}
