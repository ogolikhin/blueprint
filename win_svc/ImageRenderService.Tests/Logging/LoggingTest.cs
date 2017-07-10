using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageRenderService.Logging;
using System.Threading;
using BluePrintSys.Messaging.CrossCutting.Logging;
using log4net.Appender;

namespace ImageRenderService.Tests.Logging
{
    [TestClass]
    public class LoggingTest
    {
        private ILogListener _instance = null;
        public LoggingTest()
        {
            _instance = Log4NetStandardLogListener.Instance;
            LogManager.Manager.AddListener(_instance);
        }

        [TestMethod]
        public void LoggingBasicTest()
        {
            const string DEBUG_MSG = "ImageGen Service Logging Debug-Test.";
            const string INFO_MSG = "ImageGen Service Logging Info-Test.";
            const string WARN_MSG = "ImageGen Service Logging Warn-Test.";
            const string ERROR_MSG = "ImageGen Service Logging Error-Test.";
            const string FATAL_MSG = "ImageGen Service Logging Fatal-Test.";

            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            Log.Debug(DEBUG_MSG);

            Log.Info(INFO_MSG);

            Log.Warn(WARN_MSG);

            Log.Error(ERROR_MSG);

            Log.Fatal(FATAL_MSG);
            
              
            // Let it finish...          
            Thread.Sleep(1000);

            // Get all Five log Entries from Memory Appender
            var logEntries = appender.GetEvents();

            var appenders = log4net.LogManager.GetRepository().GetAppenders();

            // Remove Listeners
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();

            // Check the number of logged messages
            Assert.IsTrue(logEntries.Length == 5);

            // Debug Message type should be first by priority
            Assert.IsTrue(logEntries[0].RenderedMessage.Equals(DEBUG_MSG));

            // Error Message must be the last one
            Assert.IsTrue(logEntries[4].RenderedMessage.Equals(ERROR_MSG));

            // Must be at least One Appender
            Assert.IsTrue(appenders.Length >= 1);
            
        }

        [TestMethod]
        public void LoggingExceptionTest()
        {
            const string TITLE_MSG = "Exception Message";
            const string EXCEPTION_MSG = "ImageGen Service Logging Exception-test";

            var appender = new MemoryAppender();
            log4net.Config.BasicConfigurator.Configure(appender);

            var exc = new Exception(EXCEPTION_MSG);

            Log.Fatal(exc);
            Log.Fatal(TITLE_MSG, exc);

            Thread.Sleep(1000);

            var logEntries = appender.GetEvents();

            // Remove Listeners
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();

            // Check for Exception message
            Assert.IsTrue(logEntries[0].RenderedMessage.Contains(EXCEPTION_MSG));

            // Check for text message
            Assert.IsTrue(logEntries[1].RenderedMessage.Contains(TITLE_MSG));
        }

    }
}
