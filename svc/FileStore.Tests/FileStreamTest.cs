using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Text;
using System.Linq;
using Moq;
using FileStore.Repositories;
using FileStore.Controllers;
using System.Threading.Tasks;
using System.Globalization;
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
                File file = fsapi.GetFile(File.ConvertToFileStoreId("333333333!@#@!@!@!33333333333333333333333"));
            }
            catch (FormatException)
            {
                // assert
                return;
            }
            Assert.Fail("No exception was thrown.");

        }
    }
}
