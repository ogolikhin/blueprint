using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging.Utility;

namespace ServiceLibrary.EventSources
{
    /// <summary>
    /// Summary description for EventSourcesTests
    /// </summary>
    [TestClass]
    public class EventSourcesTests
    {

        [TestMethod]
        public void BlueprintEventSource_Verify()
        {
            var analyzer = new EventSourceAnalyzer
            {
                ExcludeWriteEventTypeOrder = true
            };
            analyzer.Inspect(BlueprintEventSource.Log);
        }

        [TestMethod]
        public void CLogEventSource_Verify()
        {
            var analyzer = new EventSourceAnalyzer
            {
                ExcludeWriteEventTypeOrder = true
            };
            analyzer.Inspect(CLogEventSource.Log);
        }

        [TestMethod]
        public void PerformanceLogEventSource_Verify()
        {
            var analyzer = new EventSourceAnalyzer
            {
                ExcludeWriteEventTypeOrder = true
            };
            analyzer.Inspect(PerformanceLogEventSource.Log);
        }

        [TestMethod]
        public void SQLTraceLogEventSource_Verify()
        {
            var analyzer = new EventSourceAnalyzer
            {
                ExcludeWriteEventTypeOrder = true
            };
            analyzer.Inspect(SQLTraceLogEventSource.Log);
        }

        [TestMethod]
        public void StandardLogEventSource_Verify()
        {
            var analyzer = new EventSourceAnalyzer
            {
                ExcludeWriteEventTypeOrder = true
            };
            analyzer.Inspect(StandardLogEventSource.Log);
        }
    }
}
