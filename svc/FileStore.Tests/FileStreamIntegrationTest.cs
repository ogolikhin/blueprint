using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Net.Mime;

namespace FileStore
{
    [TestClass]
    public class FileStreamIntegrationTest
    {
        // Note: database connection strings for integration testing are in app.config
        // The connection strings will be loaded into WebApiConfig static variables when
        // the service initializes

        private struct TestSetup
        {
            public string FileStoreSvcUri;
            public string FileGuid;
            public string FileName;
            public int FileSize;
            public string ContentType;
        }


        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestGetFileInfoWhenFileExists()
        {
            TestSetup thisTest = SetupTest();

            //Call Head Method
            var response = GetResponse(thisTest.FileStoreSvcUri, thisTest.FileGuid, "HEAD");

            AssertResponseHeadersMatch(thisTest, response);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestGetFileContentWhenFileExists()
        {
            // Request file content from FileStream database
            // Repeat this test for each row in the datasource csv file

            // Setup
            TestSetup thisTest = SetupTest();

            // Act
            var response = GetResponse(thisTest.FileStoreSvcUri, thisTest.FileGuid, "GET");

            // Assert
            AssertResponseHeadersMatch(thisTest, response);

            AssertFileContentNotEmpty(thisTest, response);

        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestFileStreamIntegration.csv", "TestFileStreamIntegration#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestGetFileContentWhenFileNotFound()
        {
            // Request file content from FileStream database
            // using a file id that is not in the database

            // Repeat this test for each row in the datasource csv file

            // Setup
            TestSetup thisTest = SetupTest();
            thisTest.FileGuid = "9E9A2E81-6363-463A-8467-8E2DF5635B60";

            // Act
            var response = GetResponse(thisTest.FileStoreSvcUri, thisTest.FileGuid, "GET");

            // Assert
            AssertResponseStatusCodeNotFound(response);

            AssertFileContentIsEmpty(response);

        }

        #region [ Private Methods]

        private TestSetup SetupTest()
        {
            var testSetup = new TestSetup
            {
                ContentType = Convert.ToString(TestContext.DataRow["ContentType"]),
                FileGuid = Convert.ToString(TestContext.DataRow["FileGuid"]),
                FileName = Convert.ToString(TestContext.DataRow["FileName"]),
                FileSize = Convert.ToInt32(TestContext.DataRow["FileSize"]),
                FileStoreSvcUri = Convert.ToString(TestContext.DataRow["FilesUriCall"])
            };

            return testSetup;

        }

        private HttpWebResponse GetResponse(string filesUriCall, string fileGuid, string method)
        {
            string uri = string.Format("{0}{1}", filesUriCall, fileGuid);
            var fetchRequest = (HttpWebRequest)WebRequest.Create(uri);
            fetchRequest.Method = method;
            fetchRequest.Accept = "application/json";
            fetchRequest.KeepAlive = true;
            fetchRequest.Credentials = CredentialCache.DefaultCredentials;

            HttpWebResponse objResponse;
            try
            {
                objResponse = fetchRequest.GetResponse() as HttpWebResponse;// Source of xml
            }
            catch (WebException e)
            {
                if (e.Response == null)
                {
                    throw;
                }
                objResponse = (HttpWebResponse)e.Response;
            }

            return objResponse;
        }

        private void AssertResponseHeadersMatch(TestSetup thisTest, HttpWebResponse response)
        {
            var contentDispositionHeader = response.Headers["Content-Disposition"];
            var contentDispositionHeaderValue = new ContentDisposition(contentDispositionHeader);
            //var contentDispositionHeaderValue = new ContentDispositionHeaderValue(contentDispositionHeader);

            string receivedFileName = contentDispositionHeaderValue.FileName.Replace("\"", " ").Trim();

            Assert.IsTrue(String.Equals(thisTest.FileName, receivedFileName,
                StringComparison.InvariantCultureIgnoreCase),
                String.Format("Content disposition header file name is different. " +
                "Expecting file name to be {0} but got {1}.",
                thisTest.FileName, contentDispositionHeaderValue.FileName));

            Assert.IsTrue(String.Equals(thisTest.ContentType, response.ContentType,
                StringComparison.InvariantCultureIgnoreCase),
                String.Format("ContentType does not match. Expected {0} but got {1}", thisTest.ContentType,
                response.ContentType));

            var contentLength = response.Headers["File-Size"];
            int actualFileSize;
            int.TryParse(contentLength, out actualFileSize);
            Assert.AreEqual(thisTest.FileSize, actualFileSize, "ContentLength does not match. Expected {0} but got {1}", thisTest.FileSize,
               actualFileSize);
        }

        private void AssertFileContentNotEmpty(TestSetup thisTest, HttpWebResponse response)
        {

            using (var stream = response.GetResponseStream())
            {
                var md5 = GetMD5ForFileStream(stream);
                Assert.IsNotNull(md5, String.Format("File content is null. Expected {0} bytes got 0 bytes", thisTest.FileSize));
            }

            Assert.AreEqual(response.ContentLength, thisTest.FileSize,
                String.Format("File download failed. Expected {0} bytes got {1} bytes",
                thisTest.FileSize, response.ContentLength));

        }

        private void AssertResponseStatusCodeNotFound(HttpWebResponse response)
        {

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode,
                String.Format("Expecting status code 404 (Not Found) but got {0} instead.", response.StatusCode));

        }


        private void AssertFileContentIsEmpty(HttpWebResponse response)
        {
            Assert.AreEqual(response.ContentLength, 0,
                String.Format("Request for file info returned content as well. Expected 0 bytes got {0} bytes",
                response.ContentLength));

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
