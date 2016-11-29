using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ServiceLibrary.LocalLog
{
    /// <summary>
    /// Summary description for LocalLogTests
    /// </summary>
    [TestClass]
    [Ignore] // These tests create Files and have no asserts!
    public class LocalLogTests
    {
        #region LocalFileLog Tests

        [TestMethod]
        public void LocalFileLog_LogError()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogError("Error");
        }

        [TestMethod]
        public void LocalFileLog_LogInformation()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogInformation("Information");
        }

        [TestMethod]
        public void LocalFileLog_LogWarning()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogWarning("Warning");
        }

        [TestMethod]
        public void LocalFileLog_LogErrorFormat()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogErrorFormat("Error");
        }

        [TestMethod]
        public void LocalFileLog_LogInformationFormat()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogInformationFormat("Information");
        }

        [TestMethod]
        public void LocalFileLog_LogWarningFormat()
        {
            ILocalLog log = new LocalFileLog { IsTest = true };
            log.LogWarningFormat("Warning");
        }

        #endregion

        #region LocalEventLog Tests

        [TestMethod]
        public void LocalEventLog_LogError()
        {
            ILocalLog log = new LocalEventLog() { IsTest = true };
            log.LogError("Error");
        }

        [TestMethod]
        public void LocalEventLog_LogInformation()
        {
            ILocalLog log = new LocalEventLog { IsTest = true };
            log.LogInformation("Information");
        }

        [TestMethod]
        public void LocalEventLog_LogWarning()
        {
            ILocalLog log = new LocalEventLog { IsTest = true };
            log.LogWarning("Warning");
        }

        [TestMethod]
        public void LocalEventLog_LogErrorFormat()
        {
            ILocalLog log = new LocalEventLog { IsTest = true };
            log.LogErrorFormat("Error");
        }

        [TestMethod]
        public void LocalEventLog_LogInformationFormat()
        {
            ILocalLog log = new LocalEventLog { IsTest = true };
            log.LogInformationFormat("Information");
        }

        [TestMethod]
        public void LocalEventLog_LogWarningFormat()
        {
            ILocalLog log = new LocalEventLog { IsTest = true };
            log.LogWarningFormat("Warning");
        }

        #endregion

    }
}
