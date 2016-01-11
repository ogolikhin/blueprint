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
    public class LocalLogTests
    {

        [TestMethod]
        public void LogError()
        {
            var logMock = new Mock<ILocalLog>().Object;
            logMock.LogError("Error");
        }

        [TestMethod]
        public void LogInformation()
        {
            var logMock = new Mock<ILocalLog>().Object;
            logMock.LogInformation("Information");
        }

        [TestMethod]
        public void LogWarning()
        {
            var logMock = new Mock<ILocalLog>().Object;
            logMock.LogWarning("Warning");
        }

    }
}
