using System;
using System.Xml;
using System.Xml.Linq;
using Logging.Database.TestObjects;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Configuration;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Logging.Database
{
    public abstract class GivenPartialSqlDatabaseSinkElement : ContextBase
    {
        private ISinkElement _sut;
        private XElement _element;

        protected override void Given()
        {
            _element = new XElement(XName.Get("blueprintSqlDatabaseSink", @"http://schemas.blueprintsys.com/sinks/blueprintSqlDatabaseSink"),
                new XAttribute("instanceName", "instanceName"),
                new XAttribute("connectionString", "Data Source=(localdb)\v11.0;Initial Catalog=SemanticLoggingTests;Integrated Security=True"));

            _sut = new Logging.Database.Configuration.BlueprintSqlDatabaseSinkElement();
        }

        [TestClass]
        public class WhenQueryForCanCreateSink : GivenPartialSqlDatabaseSinkElement
        {
            [TestMethod]
            [TestCategory("Logging.Database")]
            public void then_instance_can_be_created()
            {
                Assert.IsTrue(_sut.CanCreateSink(_element));
            }
        }

        [TestClass]
        public class WhenCreateSinkWithRequiredParameters : GivenPartialSqlDatabaseSinkElement
        {
            private IObserver<EventEntry> _observer;

            protected override void When()
            {
                _observer = _sut.CreateSink(_element);
            }

            [TestMethod]
            [TestCategory("Logging.Database")]
            public void then_sink_is_created()
            {
                Assert.IsNotNull(this._observer);
            }
        }
    }

    public abstract class GivenSqlDatabaseSinkElement : ContextBase
    {
        private ISinkElement _sut;
        private XElement _element;

        protected override void Given()
        {
            _element = new XElement(XName.Get("blueprintSqlDatabaseSink", @"http://schemas.blueprintsys.com/sinks/blueprintSqlDatabaseSink"),
                new XAttribute("instanceName", "instanceName"),
                new XAttribute("connectionString", "Data Source=(localdb)\v11.0;Initial Catalog=SemanticLoggingTests;Integrated Security=True"),
                new XAttribute("tableName", "tableName"),
                new XAttribute("storedProcedureName", "storedProcedureName"),
                new XAttribute("bufferingIntervalInSeconds", 15),
                new XAttribute("bufferingCount", Buffering.DefaultBufferingCount),
                new XAttribute("bufferingFlushAllTimeoutInSeconds", 5),
                new XAttribute("maxBufferSize", Buffering.DefaultMaxBufferSize)
                );

            _sut = new Logging.Database.Configuration.BlueprintSqlDatabaseSinkElement();
        }

        [TestClass]
        public class WhenQueryForCanCreateSink : GivenSqlDatabaseSinkElement
        {
            [TestMethod]
            [TestCategory("Logging.Database")]
            public void then_instance_can_be_created()
            {
                Assert.IsTrue(this._sut.CanCreateSink(this._element));
            }
        }

        [TestClass]
        public class WhenCreateSinkWithRequiredParameters : GivenSqlDatabaseSinkElement
        {
            private IObserver<EventEntry> _observer;

            protected override void When()
            {
                _observer = _sut.CreateSink(_element);
            }

            [TestMethod]
            [TestCategory("Logging.Database")]
            public void then_sink_is_created()
            {
                Assert.IsNotNull(this._observer);
            }
        }
    }
}
