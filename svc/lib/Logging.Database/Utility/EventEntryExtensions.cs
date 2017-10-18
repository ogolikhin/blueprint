// Copyright (c) Microsoft Corporation. All rights reserved. 

using System;
using System.Data;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.SqlServer.Server;
using ServiceLibrary.Helpers;

namespace Logging.Database.Utility
{
    /// <summary>
    /// Extensions for <see cref="EventEntry"/>.
    /// </summary>
    internal static class EventEntryExtensions
    {
        internal static SqlMetaData[] SqlMetaData;
        internal static string[] Fields;

        static EventEntryExtensions()
        {
            if (SqlMetaData != null)
            {
                return;
            }

            SqlMetaData = new[]
            {
                new SqlMetaData("InstanceName", SqlDbType.NVarChar, 1000),
                new SqlMetaData("ProviderId", SqlDbType.UniqueIdentifier),
                new SqlMetaData("ProviderName", SqlDbType.NVarChar, 500),
                new SqlMetaData("EventId", SqlDbType.Int),
                new SqlMetaData("EventKeywords", SqlDbType.BigInt),
                new SqlMetaData("Level", SqlDbType.Int),
                new SqlMetaData("Opcode", SqlDbType.Int),
                new SqlMetaData("Task", SqlDbType.Int),
                new SqlMetaData("Timestamp", SqlDbType.DateTimeOffset),
                new SqlMetaData("Version", SqlDbType.Int),
                new SqlMetaData("FormattedMessage", SqlDbType.NVarChar, 4000),
                new SqlMetaData("Payload", SqlDbType.Xml),
                // new SqlMetaData("ActivityId", SqlDbType.UniqueIdentifier),
                // new SqlMetaData("RelatedActivityId", SqlDbType.UniqueIdentifier),
                // new SqlMetaData("ProcessId", SqlDbType.Int),
                // new SqlMetaData("ThreadId", SqlDbType.Int),
                new SqlMetaData("IpAddress", SqlDbType.NVarChar, 45),
                new SqlMetaData("Source", SqlDbType.NVarChar, 100),
                new SqlMetaData("UserName", SqlDbType.NVarChar, -1),
                new SqlMetaData("SessionId", SqlDbType.NVarChar, 40),
                new SqlMetaData("OccurredAt", SqlDbType.DateTimeOffset),
                new SqlMetaData("ActionName", SqlDbType.NVarChar, 200),
                new SqlMetaData("CorrelationId", SqlDbType.UniqueIdentifier),
                new SqlMetaData("Duration", SqlDbType.Float)
            };

            Fields = SqlMetaData.Select(x => x.Name).ToArray();
        }

        internal static SqlDataRecord ToSqlDataRecord(this EventEntry record, string instanceName)
        {
            var sqlDataRecord = new SqlDataRecord(SqlMetaData);

            sqlDataRecord.SetValue(0, instanceName ?? string.Empty);
            sqlDataRecord.SetValue(1, record.ProviderId);
            sqlDataRecord.SetValue(2, record.Schema.ProviderName ?? string.Empty);
            sqlDataRecord.SetValue(3, record.EventId);
            sqlDataRecord.SetValue(4, (long)record.Schema.Keywords);
            sqlDataRecord.SetValue(5, (int)record.Schema.Level);
            sqlDataRecord.SetValue(6, (int)record.Schema.Opcode);
            sqlDataRecord.SetValue(7, (int)record.Schema.Task);
            sqlDataRecord.SetValue(8, record.Timestamp);
            sqlDataRecord.SetValue(9, record.Schema.Version);
            sqlDataRecord.SetValue(10, (object)record.FormattedMessage ?? DBNull.Value);
            sqlDataRecord.SetValue(11, (object)EventEntryUtil.XmlSerializePayload(record) ?? DBNull.Value);
            // sqlDataRecord.SetValue(12, record.ActivityId);
            // sqlDataRecord.SetValue(13, record.RelatedActivityId);
            // sqlDataRecord.SetValue(14, record.ProcessId);
            // sqlDataRecord.SetValue(15, record.ThreadId);
            sqlDataRecord.SetValue(12, EventEntryUtil.GetPayloadValue(record, "IpAddress"));
            sqlDataRecord.SetValue(13, EventEntryUtil.GetPayloadValue(record, "Source"));
            sqlDataRecord.SetValue(14, EventEntryUtil.GetPayloadValue(record, "UserName"));
            sqlDataRecord.SetValue(15, EventEntryUtil.GetPayloadValue(record, "SessionId"));
            sqlDataRecord.SetValue(16, I18NHelper.DateTimeOffsetParseInvariant(EventEntryUtil.GetPayloadValue(record, "OccurredAt")));
            var actionName = EventEntryUtil.GetPayloadValue(record, "ActionName");
            sqlDataRecord.SetValue(17, actionName);
            // if (!string.IsNullOrWhiteSpace(actionName))
            // {
            Guid correlationId;
            if (Guid.TryParse(EventEntryUtil.GetPayloadValue(record, "CorrelationId"), out correlationId))
            {
                sqlDataRecord.SetValue(18, correlationId);
            }
            double duration = 0;
            if (double.TryParse(EventEntryUtil.GetPayloadValue(record, "Duration"), out duration))
            {
                sqlDataRecord.SetValue(19, duration);
            }
            // }

            return sqlDataRecord;
        }
    }
}
