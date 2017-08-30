using System;
using System.Linq;
using System.Threading;
using BlueprintSys.RC.Services.Logging;
using BluePrintSys.Messaging.CrossCutting.Logging;
using log4net.Appender;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the logging in the Action Handler Service
    /// </summary>
    [TestClass]
    public class LoggingTests
    {
        [TestMethod]
        public void Logging_LogEachMessageTypeSuccessfully()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            const string debugTestMessage = "Debug Test Message";
            const string infoTestMessage = "Info Test Message";
            const string warnTestMessage = "Warn Test Message";
            const string errorTestMessage = "Error Test Message";
            const string fatalTestMessage = "Fatal Test Message";
            Log.Debug(debugTestMessage);
            Log.Info(infoTestMessage);
            Log.Warn(warnTestMessage);
            Log.Error(errorTestMessage);
            Log.Fatal(fatalTestMessage);

            // Let it finish...
            Thread.Sleep(1000);

            // Get all log Entries from the Memory Appender
            var logEntries = appender.GetEvents();
            var appenders = log4net.LogManager.GetRepository().GetAppenders();

            // Remove Listeners
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();

            // Check the number of logged messages
            Assert.AreEqual(5, logEntries.Length);
            // Debug Message type should be first by priority
            Assert.AreEqual(debugTestMessage, logEntries.First().RenderedMessage);
            // Error Message must be the last one
            Assert.AreEqual(errorTestMessage, logEntries.Last().RenderedMessage);
            // Must be at least One Appender
            Assert.IsTrue(appenders.Length >= 1);
        }

        [TestMethod]
        public void Logging_LogExceptionsSuccessfully()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);
            
            const string exceptionTestMessage1 = "Exception Test Message 1";
            const string exceptionTestMessage2 = "Exception Test Message 2";
            const string fatalTestMessage = "Fatal Test Message";
            Log.Fatal(new Exception(exceptionTestMessage1));
            Log.Fatal(fatalTestMessage, new Exception(exceptionTestMessage2));

            // Let it finish...
            Thread.Sleep(1000);

            // Get all log Entries from the Memory Appender
            var logEntries = appender.GetEvents();

            // Remove Listeners
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();

            // Check for the messages
            Assert.IsTrue(logEntries[0].RenderedMessage.Contains(exceptionTestMessage1));
            Assert.IsTrue(logEntries[1].RenderedMessage.Contains(exceptionTestMessage2));
            Assert.IsTrue(logEntries[1].RenderedMessage.Contains(fatalTestMessage));
        }
    }
}
