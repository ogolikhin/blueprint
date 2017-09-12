using System;
using System.Linq;
using System.Threading;
using BlueprintSys.RC.Services.Helpers;
using BlueprintSys.RC.Services.Logging;
using BlueprintSys.RC.Services.Models;
using BluePrintSys.Messaging.CrossCutting.Logging;
using BluePrintSys.Messaging.Models.Actions;
using log4net.Appender;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Level = log4net.Core.Level;

namespace BlueprintSys.RC.Services.Tests
{
    /// <summary>
    /// Tests for the logging in the Action Handler Service
    /// </summary>
    [TestClass]
    public class LoggingTests
    {
        private static MemoryAppender _appender;
        private const string Args = "args";

        [TestInitialize]
        public void TestInitialize()
        {
            LogManager.Manager.AddListener(Log4NetStandardLogListener.Instance);
            _appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(_appender);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            // Remove Listeners
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();
        }

        [TestMethod]
        public void CrossCuttingLogging_LogsDebugMessagesSuccessfully()
        {
            const string debugTestMessage1 = "Debug Test Message 1";
            Log.Debug(debugTestMessage1);

            const string debugTestMessage2 = "Debug Test Message 2 {0}";
            Log.DebugFormat(debugTestMessage2, Args);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(2, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Debug && s.RenderedMessage.Contains(debugTestMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Debug && s.RenderedMessage.Contains(debugTestMessage2.Replace("{0}", Args))));
        }

        [TestMethod]
        public void CrossCuttingLogging_LogsInfoMessagesSuccessfully()
        {
            const string infoTestMessage1 = "Info Test Message 1";
            Log.Info(infoTestMessage1);

            const string infoTestMessage2 = "Info Test Message 2 {0}";
            Log.InfoFormat(infoTestMessage2, Args);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(2, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Info && s.RenderedMessage.Contains(infoTestMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Info && s.RenderedMessage.Contains(infoTestMessage2.Replace("{0}", Args))));
        }

        [TestMethod]
        public void CrossCuttingLogging_LogsWarnMessagesSuccessfully()
        {
            const string warnTestMessage1 = "Warn Test Message 1";
            Log.Warn(warnTestMessage1);

            const string warnTestMessage2 = "Warn Test Message 2 {0}";
            Log.WarnFormat(warnTestMessage2, Args);

            const string warnTestMessage3 = "Warn Test Message 3";
            const string warnExceptionMessage1 = "Warn Exception Message 1";
            Log.Warn(warnTestMessage3, new Exception(warnExceptionMessage1));

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(3, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Warn && s.RenderedMessage.Contains(warnTestMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Warn && s.RenderedMessage.Contains(warnTestMessage2.Replace("{0}", Args))));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Warn && s.RenderedMessage.Contains(warnTestMessage3) && s.RenderedMessage.Contains(warnExceptionMessage1)));
        }

        [TestMethod]
        public void CrossCuttingLogging_LogsErrorMessagesSuccessfully()
        {
            const string errorTestMessage1 = "Error Test Message 1";
            Log.Error(errorTestMessage1);

            const string errorTestMessage2 = "Error Test Message 2 {0}";
            Log.ErrorFormat(errorTestMessage2, Args);

            const string errorTestMessage3 = "Error Test Message 3";
            const string errorExceptionMessage1 = "Error Exception Message 1";
            Log.Error(errorTestMessage3, new Exception(errorExceptionMessage1));

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(3, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Error && s.RenderedMessage.Contains(errorTestMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Error && s.RenderedMessage.Contains(errorTestMessage2.Replace("{0}", Args))));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Error && s.RenderedMessage.Contains(errorTestMessage3) && s.RenderedMessage.Contains(errorExceptionMessage1)));
        }

        [TestMethod]
        public void CrossCuttingLogging_LogsFatalMessagesSuccessfully()
        {
            const string fatalTestMessage1 = "Fatal Test Message 1";
            Log.Fatal(fatalTestMessage1);

            const string fatalTestMessage2 = "Fatal Test Message 2";
            Log.FatalFormat(fatalTestMessage2, Args);

            const string fatalExceptionMessage1 = "Fatal Exception Message 1";
            Log.Fatal(new Exception(fatalExceptionMessage1));

            const string fatalTestMessage3 = "Fatal Test Message 3";
            const string fatalExceptionMessage2 = "Fatal Exception Message 2";
            Log.Fatal(fatalTestMessage3, new Exception(fatalExceptionMessage2));

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(4, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Fatal && s.RenderedMessage.Contains(fatalTestMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Fatal && s.RenderedMessage.Contains(fatalTestMessage2.Replace("{0}", Args))));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Fatal && s.RenderedMessage.Contains(fatalExceptionMessage1)));
            Assert.IsNotNull(logEntries.Single(s => s.Level == Level.Fatal && s.RenderedMessage.Contains(fatalTestMessage3) && s.RenderedMessage.Contains(fatalExceptionMessage2)));
        }

        [TestMethod]
        public void Logger_LogsDebugMessageSuccessfully()
        {
            const string debugTestMessage = "My Debug Message";
            Logger.Log(debugTestMessage, new NotificationMessage(), new TenantInformation(), LogLevel.Debug);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(1, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(e => e.RenderedMessage.Contains(debugTestMessage)));
        }

        [TestMethod]
        public void Logger_LogsInfoMessageSuccessfully()
        {
            const string infoTestMessage = "My Info Message";
            Logger.Log(infoTestMessage, new NotificationMessage(), new TenantInformation(), LogLevel.Info);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(1, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(e => e.RenderedMessage.Contains(infoTestMessage)));
        }

        [TestMethod]
        public void Logger_LogsWarnMessageSuccessfully()
        {
            const string warnTestMessage = "My Warn Message";
            Logger.Log(warnTestMessage, new NotificationMessage(), new TenantInformation(), LogLevel.Warn);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(1, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(e => e.RenderedMessage.Contains(warnTestMessage)));
        }

        [TestMethod]
        public void Logger_LogsErrorMessageSuccessfully()
        {
            const string errorTestMessage = "My Error Message";
            Logger.Log(errorTestMessage, new NotificationMessage(), new TenantInformation(), LogLevel.Error);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(1, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(e => e.RenderedMessage.Contains(errorTestMessage)));
        }

        [TestMethod]
        public void Logger_LogsFatalMessageSuccessfully()
        {
            const string fatalTestMessage = "My Fatal Message";
            Logger.Log(fatalTestMessage, new NotificationMessage(), new TenantInformation(), LogLevel.Fatal);

            // Let it finish...
            Thread.Sleep(100);

            // Get all log Entries from the Memory Appender
            var logEntries = _appender.PopAllEvents();

            // Check for the logged messages
            Assert.AreEqual(1, logEntries.Length);
            Assert.IsNotNull(logEntries.Single(e => e.RenderedMessage.Contains(fatalTestMessage)));
        }
    }
}
