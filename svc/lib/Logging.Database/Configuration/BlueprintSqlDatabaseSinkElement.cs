using System;
using System.Xml.Linq;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Logging.Database.Utility;

namespace Logging.Database.Configuration
{
    public class BlueprintSqlDatabaseSinkElement : ISinkElement
    {
        private readonly XName _sinkName = XName.Get("blueprintSqlDatabaseSink", "http://schemas.blueprintsys.com/sinks/blueprintSqlDatabaseSink");

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with Guard class")]
        public bool CanCreateSink(XElement element)
        {
            Guard.ArgumentNotNull(element, "element");

            return element.Name == _sinkName;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Validated with Guard class")]
        public IObserver<EventEntry> CreateSink(XElement element)
        {
            Guard.ArgumentNotNull(element, "element");

            var subject = new EventEntrySubject();
            subject.LogToSqlDatabase(
                (string)element.Attribute("instanceName"),
                (string)element.Attribute("connectionString"),
                (string)element.Attribute("tableName") ?? BlueprintSqlDatabaseLog.DefaultTableName,
                (string)element.Attribute("storedProcedureName") ?? BlueprintSqlDatabaseLog.DefaultStoredProcedureName,
                element.Attribute("bufferingIntervalInSeconds").ToTimeSpan(),
                (int?)element.Attribute("bufferingCount") ?? Buffering.DefaultBufferingCount,
                element.Attribute("bufferingFlushAllTimeoutInSeconds").ToTimeSpan() ?? Constants.DefaultBufferingFlushAllTimeout,
                (int?)element.Attribute("maxBufferSize") ?? Buffering.DefaultMaxBufferSize);

            return subject;
        }
    }
}
