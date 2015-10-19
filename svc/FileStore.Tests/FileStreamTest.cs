using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FileStore.Repositories;
using FileStore.Models;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStreamTest
    {
        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void GetFileFromFileStream_ImproperGuid_FormatException()
        {
            // Arrange

            FileStreamRepository fsapi = new FileStreamRepository();
            try
            {
                // Act 
                File file = fsapi.GetFile(File.ConvertToStoreId("333333333!@#@!@!@!33333333333333333333333"));
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
        public void GetFileFromFileStream_ProperGuid_NFormat()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("N");

            // Act 
            var actualGuid = File.ConvertToStoreId(guid);

            Assert.IsTrue(actualGuid != Guid.Empty);
        }

        [TestCategory("FileStreamSvc-UnitTests")]
        [TestMethod]
        public void GetFileFromFileStream_ProperGuid_DFormat()
        {
            // Arrange
            var guid = Guid.NewGuid().ToString("D");

            // Act 
            var actualGuid = File.ConvertToStoreId(guid);

            Assert.IsTrue(actualGuid != Guid.Empty);
        }
    }
}
