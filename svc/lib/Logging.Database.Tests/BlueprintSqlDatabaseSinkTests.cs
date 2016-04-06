using Logging.Database.Sinks;
using Logging.Database.TestObjects;
using Logging.Database.TestSupport;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics.Tracing;
using System.Globalization;
using System.IO;
using System.Threading;
using ServiceLibrary.Helpers;

namespace Logging.Database
{
    [TestClass]
    public class SqlDbSinkGivenConfiguration
    {
        [TestMethod]
        [TestCategory("Logging.Database")]
        public void when_creating_listener_for_null_instance_name_then_throws()
        {
            AssertEx.Throws<ArgumentNullException>(() => new BlueprintSqlDatabaseSink(null, "valid", "tableName", "storedProcedureName", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan));
        }

        [TestMethod]
        [TestCategory("Logging.Database")]
        public void when_creating_listener_for_null_connection_string_then_throws()
        {
            AssertEx.Throws<ArgumentNullException>(() => new BlueprintSqlDatabaseSink("test", null, "tableName", "storedProcedureName", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan));
        }

        [TestMethod]
        [TestCategory("Logging.Database")]
        public void when_creating_listener_for_null_table_name_then_throws()
        {
            AssertEx.Throws<ArgumentNullException>(() => new BlueprintSqlDatabaseSink("test", "valid", null, "storedProcedureName", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan));
        }

        [TestMethod]
        [TestCategory("Logging.Database")]
        public void when_creating_listener_with_invalid_connection_string_then_throws()
        {
            AssertEx.Throws<ArgumentException>(() => new BlueprintSqlDatabaseSink("test", "InvalidConnectionString", "tableName", "storedProcedureName", Buffering.DefaultBufferingInterval, Buffering.DefaultBufferingCount, Buffering.DefaultMaxBufferSize, Timeout.InfiniteTimeSpan));
        }
    }

    [TestClass]
    public class GivenEmptyLoggingDatabase : LocalDatabaseContext
    {

        protected override string GetLocalDatabaseFileName()
        {
            return I18NHelper.FormatInvariant("sqldbtests{0}", new Random().Next(1000, 9999));
        }

        protected override void Given()
        {
            base.Given();

            // create a temporary database

            this.LocalDbConnection.ChangeDatabase(this.DbName);

            var splitter = new string[] { "\r\nGO" };
            var commands = new List<string>();

            commands.AddRange(File.ReadAllText(@".\Logs.sql").Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            commands.AddRange(File.ReadAllText(@".\LogsType.sql").Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            commands.AddRange(File.ReadAllText(@".\WriteLogs.sql").Split(splitter, StringSplitOptions.RemoveEmptyEntries));
            commands.AddRange(File.ReadAllText(@".\DeleteLogs.sql").Split(splitter, StringSplitOptions.RemoveEmptyEntries));

            foreach (var command in commands)
            {
                using (var cmd = new SqlCommand(command, this.LocalDbConnection))
                {
                    cmd.ExecuteNonQuery();
                }
            }

        }

        [TestMethod]
        [TestCategory("Logging.Database")]
        [DeploymentItem(@"SqlScripts\Logs.sql")]
        [DeploymentItem(@"SqlScripts\LogsType.sql")]
        [DeploymentItem(@"SqlScripts\WriteLogs.sql")]
        [DeploymentItem(@"SqlScripts\DeleteLogs.sql")]
        public void CreateListenerWithLargeBufferTest()
        {
            const int bufferingCount = 200;

            var dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    "BlueprintSys-Blueprint-Blueprint",
                    this.GetSqlConnectionString(),
                    bufferingInterval: TimeSpan.FromSeconds(15),
                    bufferingCount: bufferingCount);
            dbListener.EnableEvents(TestEventSource.Log, EventLevel.LogAlways, Keywords.All);

            var logger = TestEventSource.Log;

            for (var i = 0; i < 200; i++)
            {
                logger.Informational("10.0.0.0", "Test", I18NHelper.FormatInvariant("message-{0}", i), DateTime.Now, "", "", 666);
            }

            dbListener.DisableEvents(TestEventSource.Log);
            dbListener.Dispose();

        }

        [TestMethod]
        [TestCategory("Logging.Database")]
        [DeploymentItem(@"SqlScripts\Logs.sql")]
        [DeploymentItem(@"SqlScripts\LogsType.sql")]
        [DeploymentItem(@"SqlScripts\WriteLogs.sql")]
        [DeploymentItem(@"SqlScripts\DeleteLogs.sql")]
        public void CreateListenerWithSmallBufferTest()
        {
            const int bufferingCount = 10;

            var dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    "BlueprintSys-Blueprint-Blueprint",
                    this.GetSqlConnectionString(),
                    bufferingInterval: TimeSpan.FromSeconds(15),
                    bufferingCount: bufferingCount);
            dbListener.EnableEvents(TestEventSource.Log, EventLevel.LogAlways, Keywords.All);

            var logger = TestEventSource.Log;

            for (var i = 0; i < 200; i++)
            {
                logger.Informational("10.0.0.0", "Test", I18NHelper.FormatInvariant("message-{0}", i), DateTime.Now, "", "", 666);
            }

            dbListener.DisableEvents(TestEventSource.Log);
            dbListener.Dispose();

        }

    }
}
