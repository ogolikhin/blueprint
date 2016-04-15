using CustomAttributes;
using Logging.Database.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using NUnit.Framework;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Xml.Linq;

namespace LoggingDatabaseTests
{
    [TestFixture]
    [Category(Categories.LoggingDatabase)]
    public static class BlueprintSqlDatabaseConfigurationTests
    {
        [TestCase()]
        [Description("Create sink with minimal configuration should create sink")]
        public static void CreateWithMinimalConfiguration_ShouldCreateSink()
        {
            string validConnectionFormat = @"Data Source=(localdb)\v11.0;Initial Catalog=SemanticLoggingTests;Integrated Security=True";

            var element = new XElement(XName.Get("blueprintSqlDatabaseSink", @"http://schemas.blueprintsys.com/sinks/blueprintSqlDatabaseSink"),
                new XAttribute("instanceName", "instanceName"),
                new XAttribute("connectionString", validConnectionFormat));

            var sinkElement = new BlueprintSqlDatabaseSinkElement();

            Assert.IsTrue(sinkElement.CanCreateSink(element), "Unable to validate sink");

            Assert.DoesNotThrow(() =>
            {
                var observer = sinkElement.CreateSink(element);
                Assert.IsNotNull(observer, "Observer should not be null");
            }, "Unable to create sink");
        }

        [TestCase()]
        [Description("Create sink with configuration should create sink")]
        public static void CreateWithConfiguration_ShouldCreateSink()
        {
            string validConnectionFormat = @"Data Source=(localdb)\v11.0;Initial Catalog=SemanticLoggingTests;Integrated Security=True";

            var element = new XElement(XName.Get("blueprintSqlDatabaseSink", @"http://schemas.blueprintsys.com/sinks/blueprintSqlDatabaseSink"),
                new XAttribute("instanceName", "instanceName"),
                new XAttribute("connectionString", validConnectionFormat),
                new XAttribute("tableName", "tableName"),
                new XAttribute("storedProcedureName", "storedProcedureName"),
                new XAttribute("bufferingIntervalInSeconds", 15),
                new XAttribute("bufferingCount", Buffering.DefaultBufferingCount),
                new XAttribute("bufferingFlushAllTimeoutInSeconds", 5),
                new XAttribute("maxBufferSize", Buffering.DefaultMaxBufferSize)
                );

            var sinkElement = new BlueprintSqlDatabaseSinkElement();

            Assert.IsTrue(sinkElement.CanCreateSink(element), "Unable to validate sink");

            Assert.DoesNotThrow(() =>
            {
                var observer = sinkElement.CreateSink(element);
                Assert.IsNotNull(observer, "Observer should not be null");
            }, "Unable to create sink");
        }

        [TestCase()]
        [Description("Create sink element with null parameter should throw ArgumentNullException")]
        public static void WhenCreatingSinkElementWithNullElement_ShouldThrow()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                new BlueprintSqlDatabaseSinkElement().CanCreateSink(null);
            });
        }

    }
}
