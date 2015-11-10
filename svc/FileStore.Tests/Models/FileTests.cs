using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileStore.Models
{
    [TestClass]
    public class FileTests
    {
        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void ConvertToStoreId_ImproperGuid_FormatException()
        {
            // Arrange

            try
            {
                // Act 
                File.ConvertToStoreId("333333333!@#@!@!@!33333333333333333333333");
            }
            catch (FormatException)
            {
                // assert
                return;
            }
            Assert.Fail("No exception was thrown.");
        }

        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void ConvertToStoreId_ProperGuid_NFormat()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");

            // Act 
            var actualGuid = File.ConvertToStoreId(guid);

            Assert.IsTrue(actualGuid != Guid.Empty);
        }

        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void ConvertToStoreId_ProperGuid_DFormat()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("D");

            // Act 
            var actualGuid = File.ConvertToStoreId(guid);

            Assert.IsTrue(actualGuid != Guid.Empty);
        }
    }
}
