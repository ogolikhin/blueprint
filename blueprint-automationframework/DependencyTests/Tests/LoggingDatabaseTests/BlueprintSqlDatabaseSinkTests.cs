using Common;
using CustomAttributes;
using Logging.Database;
using Logging.Database.Sinks;
using LoggingDatabaseModel;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Sinks;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Model.Factories;
using NUnit.Framework;
using System;
using System.Diagnostics.Tracing;
using System.Linq;

namespace LoggingDatabaseTests
{
    [TestFixture]
    [Category(Categories.LoggingDatabase)]
    public static class BlueprintSqlDatabaseSinkTests
    {
        [TestCase(10, Description = "Testing small bufferingCount")]
        [TestCase(200, Description = "Testing large bufferingCount")]
        [Description("Create an EventListener and create some events. Verify events are logged to the database")]
        public static void CreateListener_ValidConnection(int bufferingCount)
        {
            EventListener dbListener = null;
            var cn = DatabaseFactory.GetConnectionString("AdminStore");

            Assert.DoesNotThrow(() =>
            {
                dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    instanceName: "Test",
                    connectionString: cn,
                    bufferingInterval: TimeSpan.FromSeconds(15),
                    bufferingCount: bufferingCount);
            }, "Creation of BlueprintSqlDatabaseLog should succeed");

            using (dbListener)
            {
                Assert.DoesNotThrow(() =>
                {
                    dbListener.EnableEvents(TestEventSource.Log, EventLevel.LogAlways, Keywords.All);
                }, "EnableEvents should succeed");

                Assert.DoesNotThrow(() =>
                {
                    for (var i = 0; i < 200; i++)
                    {
                        TestEventSource.Log.Informational("10.0.0.0", "Test", I18NHelper.FormatInvariant("message-{0}", i), DateTime.Now, "", "", 666);
                    }
                }, "Logging of events should succeed");

                Assert.DoesNotThrow(() =>
                {
                    dbListener.DisableEvents(TestEventSource.Log);
                }, "DisableEvents should succeed");
            }

        }

        [TestCase()]
        [Description("Create an EventListener with an invalid database connection string. Should throw ArgumentException")]
        public static void CreateListener_InValidConnection_ShouldThrow()
        {
            EventListener dbListener = null;
            var invalidConnection = @"Data Source=(localdb)\v11.0;Initial Database=SemanticLoggingTests;Integrated Security=True"; ;

            Assert.Throws<ArgumentException>(() =>
            {
                dbListener = BlueprintSqlDatabaseLog.CreateListener(
                    instanceName: "Test",
                    connectionString: invalidConnection);
            }, "Creation of BlueprintSqlDatabaseLog should should fail");

            dbListener?.Dispose();

        }

        [TestCase(null, "connectionString", "tableName", "storedProcedureName", Description = "Testing instanceName set to null")]
        [TestCase("instanceName", null, "tableName", "storedProcedureName", Description = "Testing connectionString set to null")]
        [TestCase("instanceName", "connectionString", null, "storedProcedureName", Description = "Testing tableName set to null")]
        [TestCase("instanceName", "connectionString", "tableName", null, Description = "Testing storedProcedureName set to null")]
        [Description("When creating BlueprintSqlDatabaseSink with null parameter then should throw ArgumentNullException")]
        public static void WhenCreatingSinkWithNullParam_ShouldThrow(string instanceName, string connectionString, string tableName, string storedProcedureName)
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                var sink = new BlueprintSqlDatabaseSink(
                    instanceName: instanceName,
                    connectionString: connectionString,
                    tableName: tableName,
                    storedProcedureName: storedProcedureName,
                    bufferingInterval: Buffering.DefaultBufferingInterval,
                    bufferingCount: Buffering.DefaultBufferingCount,
                    maxBufferSize: Buffering.DefaultMaxBufferSize,
                    onCompletedTimeout: TimeSpan.FromSeconds(20));
                sink?.Dispose();
            }, "Should throw ArgumentNullException");
        }

        [TestCase("", "connectionString", "tableName", "storedProcedureName", Description = "Testing instanceName set to empty string")]
        [TestCase("instanceName", "", "tableName", "storedProcedureName", Description = "Testing connectionString set to empty string")]
        [TestCase("instanceName", "connectionString", "", "storedProcedureName", Description = "Testing tableName set to empty string")]
        [TestCase("instanceName", "connectionString", "tableName", "", Description = "Testing storedProcedureName set to empty string")]
        [Description("When creating BlueprintSqlDatabaseSink with empty parameter then should throw ArgumentException")]
        public static void WhenCreatingSinkWithEmptyParam_ShouldThrow(string instanceName, string connectionString, string tableName, string storedProcedureName)
        {
            Assert.Throws<ArgumentException>(() =>
            {
                var sink = new BlueprintSqlDatabaseSink(
                    instanceName: instanceName,
                    connectionString: connectionString,
                    tableName: tableName,
                    storedProcedureName: storedProcedureName,
                    bufferingInterval: Buffering.DefaultBufferingInterval,
                    bufferingCount: Buffering.DefaultBufferingCount,
                    maxBufferSize: Buffering.DefaultMaxBufferSize,
                    onCompletedTimeout: TimeSpan.FromSeconds(20));
                sink?.Dispose();
            }, "Should throw ArgumentNullException");
        }

        [TestCase(-2, 10, 1000, 30000, Description = "Testing bufferingInterval minimum value out of range")]
        [TestCase(2147483648, 10, 1000, 30000, Description = "Testing bufferingInterval maximum value out of range")]
        [TestCase(10, -2, 1000, 30000, Description = "Testing onCompletedTimeout minimum value out of range")]
        [TestCase(10, 2147483648, 1000, 30000, Description = "Testing onCompletedTimeout maximum value out of range")]
        [TestCase(10, 10, -1, 30000, Description = "Testing bufferingCount minimum value out of range")]
        [TestCase(10, 10, 1000, 499, Description = "Testing maxBufferSize minimum value out of range")]
        [Description("When creating BlueprintSqlDatabaseSink with a parameter that is out of range then should throw ArgumentOutOfRangeException")]
        public static void WhenCreatingSinkWithOutOfRangeParam_ShouldThrow(double bufferingIntervalMilliseconds, double onCompletedTimeoutMilliseconds, int bufferingCount, int maxBufferSize)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                var sink = new BlueprintSqlDatabaseSink(
                    instanceName: "instanceName",
                    connectionString: "connectionString",
                    tableName: "tableName",
                    storedProcedureName: "storedProcedureName",
                    bufferingInterval: TimeSpan.FromMilliseconds(bufferingIntervalMilliseconds),
                    bufferingCount: bufferingCount,
                    maxBufferSize: maxBufferSize,
                    onCompletedTimeout: TimeSpan.FromMilliseconds(onCompletedTimeoutMilliseconds));
                sink?.Dispose();
            }, "Should throw ArgumentNullException");
        }

    }
}
