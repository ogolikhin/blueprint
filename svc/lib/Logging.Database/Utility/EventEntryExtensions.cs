﻿// Copyright (c) Microsoft Corporation. All rights reserved. 

using System;
using System.Data;
using System.Linq;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.SqlServer.Server;

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
                new SqlMetaData("ActivityId", SqlDbType.UniqueIdentifier),
                new SqlMetaData("RelatedActivityId", SqlDbType.UniqueIdentifier),
                new SqlMetaData("ProcessId", SqlDbType.Int),
                new SqlMetaData("ThreadId", SqlDbType.Int),
                new SqlMetaData("IpAddress", SqlDbType.NVarChar, 45),
                new SqlMetaData("Source", SqlDbType.NVarChar, 100),
                new SqlMetaData("MethodName", SqlDbType.NVarChar, 100),
                new SqlMetaData("FilePath", SqlDbType.NVarChar, 1000),
                new SqlMetaData("LineNumber", SqlDbType.Int),
                new SqlMetaData("StackTrace", SqlDbType.NVarChar, 4000)
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
            sqlDataRecord.SetValue(8, EventEntryUtil.GetTimestamp(record, "DateTime"));
            sqlDataRecord.SetValue(9, record.Schema.Version);
            sqlDataRecord.SetValue(10, (object)record.FormattedMessage ?? DBNull.Value);
            sqlDataRecord.SetValue(11, (object)EventEntryUtil.XmlSerializePayload(record) ?? DBNull.Value);
            sqlDataRecord.SetValue(12, record.ActivityId);
            sqlDataRecord.SetValue(13, record.RelatedActivityId);
            sqlDataRecord.SetValue(14, record.ProcessId);
            sqlDataRecord.SetValue(15, record.ThreadId);
            sqlDataRecord.SetValue(16, EventEntryUtil.GetPayloadValue(record, "IpAddress"));
            sqlDataRecord.SetValue(17, EventEntryUtil.GetPayloadValue(record, "Source"));
            sqlDataRecord.SetValue(18, EventEntryUtil.GetPayloadValue(record, "MethodName"));
            sqlDataRecord.SetValue(19, EventEntryUtil.GetPayloadValue(record, "FilePath"));
            int lineNumber = 0;
            int.TryParse(EventEntryUtil.GetPayloadValue(record, "LineNumber"), out lineNumber);
            sqlDataRecord.SetValue(20, lineNumber);
            sqlDataRecord.SetValue(21, EventEntryUtil.GetPayloadValue(record, "StackTrace"));

            return sqlDataRecord;
        }
    }
}
