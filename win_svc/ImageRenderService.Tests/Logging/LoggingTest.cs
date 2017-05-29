using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ImageRenderService.Logging;
using System.Threading.Tasks;
using System.Threading;

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
           
            Log.Info("ImageGen Service Logging Info-Test.");
            
            Log.Debug("ImageGen Service Logging Debug-Test.");

            Log.Warn("ImageGen Service Logging Warn-Test.");

            Log.Error("ImageGen Service Logging Error-Test.");

            Log.Fatal("ImageGen Service Logging Fatal-Test.");
                        
            Thread.Sleep(5000);
            Log4NetStandardLogListener.Clear();
            LogManager.Manager.ClearListeners();

            Assert.IsTrue(true);
        }
        
    }
}
