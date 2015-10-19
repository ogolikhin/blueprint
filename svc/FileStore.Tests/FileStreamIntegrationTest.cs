using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FileStore.Repositories;
using FileStore.Controllers;
using System.Net.Http;
using System.Web.Http;
using System.Configuration;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStreamIntegrationTest
    {
        // Note: database connection strings for integration testing are in app.config 
        // The connection strings will be loaded into WebApiConfig static variables when 
        // the service initializes  

        private struct TestSetup
        {
            public string fileStoreSvcUri;
            public string fileGuid;
            public string fileName;
            public int fileSize;
            public string contentType;
            public FilesController controller;
        }
        

        public TestContext TestContext { get; set; }
 

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [TestCategory("FileStream-Integration")]
        public void TestGetFileInfoWhenFileExists()
        {
            // Request HEAD info from FileStream database 
            // Repeat this test for each row in the datasource csv file 

            // Setup 
            TestSetup thisTest = SetupTest(HttpMethod.Head);

            // Act
            var actionResult = thisTest.controller.GetFile(thisTest.fileGuid).Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            AssertResponseHeadersMatch(thisTest, response);

            AssertFileContentIsEmpty(response);

        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [TestCategory("FileStream-Integration")]
        public void TestGetFileContentWhenFileExists()
        {
            // Request file content from FileStream database 
            // Repeat this test for each row in the datasource csv file 
 
            // Setup 
            TestSetup thisTest = SetupTest(HttpMethod.Get);

            // Act
            var actionResult = thisTest.controller.GetFile(thisTest.fileGuid).Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            AssertResponseHeadersMatch(thisTest, response);

            AssertFileContentNotEmpty(thisTest, response);

        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [TestCategory("FileStream-Integration")]
        public void TestGetFileContentWhenFileNotFound()
        {
            // Request file content from FileStream database 
            // using a file id that is not in the database

            // Repeat this test for each row in the datasource csv file 

            // Setup 
            TestSetup thisTest = SetupTest(HttpMethod.Get);

            // Act
            string fileIdNotFound = "9E9A2E81-6363-463A-8467-8E2DF5635B60";
            var actionResult = thisTest.controller.GetFile(fileIdNotFound).Result;

            System.Threading.CancellationToken cancellationToken = new System.Threading.CancellationToken();
            HttpResponseMessage response = actionResult.ExecuteAsync(cancellationToken).Result;

            // Assert
            AssertResponseStatusCodeNotFound(response);

            AssertFileContentIsEmpty(response);

        }
        #region [ Private Methods]

        private TestSetup SetupTest(HttpMethod requestType)
        {
            var testSetup = new TestSetup()
            {
                controller = new FilesController(),
                contentType = Convert.ToString(TestContext.DataRow["ContentType"]),             
                fileGuid = Convert.ToString(TestContext.DataRow["FileGuid"]),
                fileName = Convert.ToString(TestContext.DataRow["FileName"]),
                fileSize = Convert.ToInt32(TestContext.DataRow["FileSize"]),
                fileStoreSvcUri = ConfigurationManager.AppSettings["FileStoreSvcUri"]
            };

            testSetup.controller.Request = new HttpRequestMessage
            {
                RequestUri = new Uri(String.Format(testSetup.fileStoreSvcUri, testSetup.fileGuid)),
                Method = requestType
            };

            testSetup.controller.Configuration = new HttpConfiguration();
            testSetup.controller.Configuration.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "files/{id}",
                defaults: new { id = RouteParameter.Optional });

            return testSetup;

        }

        private void AssertResponseHeadersMatch(TestSetup thisTest, HttpResponseMessage response)
        {

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
                String.Format("Service Head request for file id {0} failed.", thisTest.fileGuid));

            var contentDispositionHeader = response.Content.Headers.ContentDisposition;
            string receivedFileName = contentDispositionHeader.FileName.ToString().Replace("\"", " ").Trim();

            Assert.IsTrue(String.Equals(thisTest.fileName, receivedFileName,
                StringComparison.InvariantCultureIgnoreCase),
                String.Format("Content disposition header file name is different. " +
                "Expecting file name to be {0} but got {1}.",
                thisTest.fileName, contentDispositionHeader.FileName));


            Assert.IsTrue(String.Equals(thisTest.contentType, response.Content.Headers.ContentType.ToString(),
                StringComparison.InvariantCultureIgnoreCase),
                String.Format("ContentType does not match. Expected {0} but got {1}", thisTest.contentType,
                response.Content.Headers.ContentType.ToString()));


            Assert.AreEqual(thisTest.fileSize, response.Content.Headers.ContentLength,
               "ContentLength does not match. Expected {0} but got {1}", thisTest.fileSize,
               response.Content.Headers.ContentLength);
        }

        private void AssertResponseStatusCodeNotFound(HttpResponseMessage response)
        {

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                String.Format("Expecting status code 404 (Not Found) but got {0} instead.", response.StatusCode));
 
        }

        private void AssertFileContentNotEmpty(TestSetup thisTest, HttpResponseMessage response)
        {

            byte[] content = response.Content.ReadAsByteArrayAsync().Result;

            Assert.IsNotNull(content,
                String.Format("File content is null. Expected {0} bytes got 0 bytes", thisTest.fileSize));

            Assert.AreEqual(content.Length, thisTest.fileSize,
                String.Format("File download failed. Expected {0} bytes got {1} bytes", 
                thisTest.fileSize, content.Length));

        }
        private void AssertFileContentIsEmpty(HttpResponseMessage response)
        {

            byte[] content = response.Content != null ? response.Content.ReadAsByteArrayAsync().Result : new byte[0];
 
            Assert.AreEqual(content.Length, 0,
                String.Format("Request for file info returned content as well. Expected 0 bytes got {0} bytes",
                content.Length));

        }

        private string GetMD5ForFileStream(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
            }
        }

        #endregion
    }
}
