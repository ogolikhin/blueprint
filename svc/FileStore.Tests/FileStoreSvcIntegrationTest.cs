using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Security.Cryptography;

namespace FileStore
{
    [TestClass]
    public class FileStoreSvcIntegrationTest
    {
        public TestContext TestContext { get; set; }
        private const int _chunkSize = 1*1024*1024; // Default chunk size is 1MB in Filestore service
        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestUploadAndDeleteFilesUsingMultipart()
        {
            var filesUriCall = "";
            if (TestContext.DataRow.Table.Columns.Contains("FilesUriCall"))
            {
                filesUriCall = Convert.ToString(TestContext.DataRow["FilesUriCall"]);
            }

            var attachmentFileName = "";
            if (TestContext.DataRow.Table.Columns.Contains("AttachmentFileName"))
            {
                attachmentFileName = Convert.ToString(TestContext.DataRow["AttachmentFileName"]);
            }

            var statusCallUri = "";
            if (TestContext.DataRow.Table.Columns.Contains("StatusUriCall"))
            {
                statusCallUri = Convert.ToString(TestContext.DataRow["StatusUriCall"]);
            }

            //Get status of Web service
            var status = GetStatus(statusCallUri);

            Assert.IsTrue(status, "Service health check failed");

            //post file and get guid
            var fileGuid = PostFile(filesUriCall, attachmentFileName);

            Assert.IsNotNull(fileGuid, "file could not be uploaded");

            //Correct guid for further usage
            fileGuid = fileGuid.Replace("\"", string.Empty);

            //Call Head Method
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName);

            //Download file
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName);

            //Delete File
            DeleteFile(filesUriCall, fileGuid);// await new SqlFilesRepository().DeleteFile(Models.File.ConvertToStoreId(fileGuid));

            //Try to call methods again again to ensure that NotFound is returned
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName, true);
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName, true);
            DeleteFile(filesUriCall, fileGuid, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestUploadAndDeleteFilesUsingNonMultipart()
        {
            var filesUriCall = "";
            if (TestContext.DataRow.Table.Columns.Contains("FilesUriCall"))
            {
                filesUriCall = Convert.ToString(TestContext.DataRow["FilesUriCall"]);
            }

            var attachmentFileName = "";
            if (TestContext.DataRow.Table.Columns.Contains("AttachmentFileName"))
            {
                attachmentFileName = Convert.ToString(TestContext.DataRow["AttachmentFileName"]);
            }

            var statusCallUri = "";
            if (TestContext.DataRow.Table.Columns.Contains("StatusUriCall"))
            {
                statusCallUri = Convert.ToString(TestContext.DataRow["StatusUriCall"]);
            }

            //Get status of Web service
            var status = GetStatus(statusCallUri);

            Assert.IsTrue(status, "Service health check failed");

            //post file and get guid
            var fileGuid = PostFileNonMultipart(filesUriCall, attachmentFileName, true);

            Assert.IsNotNull(fileGuid, "file could not be uploaded");

            //Correct guid for further usage
            fileGuid = fileGuid.Replace("\"", string.Empty);

            //Call Head Method
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName);

            //Download file
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName);

            //Delete File
            DeleteFile(filesUriCall, fileGuid);

            //Try to call methods again again to ensure that NotFound is returned
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName, true);
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName, true);
            DeleteFile(filesUriCall, fileGuid, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        public void TestUploadAndDeleteFilesUsingNonMultipartNoHeadersFailure()
        {
            var filesUriCall = "";
            if (TestContext.DataRow.Table.Columns.Contains("FilesUriCall"))
            {
                filesUriCall = Convert.ToString(TestContext.DataRow["FilesUriCall"]);
            }

            var attachmentFileName = "";
            if (TestContext.DataRow.Table.Columns.Contains("AttachmentFileName"))
            {
                attachmentFileName = Convert.ToString(TestContext.DataRow["AttachmentFileName"]);
            }

            var statusCallUri = "";
            if (TestContext.DataRow.Table.Columns.Contains("StatusUriCall"))
            {
                statusCallUri = Convert.ToString(TestContext.DataRow["StatusUriCall"]);
            }

            //Get status of Web service
            var status = GetStatus(statusCallUri);

            Assert.IsTrue(status, "Service health check failed");

            //post file and get guid
            var fileGuid = PostFileNonMultipart(filesUriCall, attachmentFileName, false);

            Assert.IsNull(fileGuid, "file should not be uploaded");
        }

        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        public void TestPutUploadAndDeleteFiles_SendDivisibleChunkSize()
        {
            PutTestExecute(_chunkSize*2);
        }

        [Ignore] // Integration test should be moved to blueprint-automationframework repository
        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        public void TestPutUploadAndDeleteFiles_SendUnDivisibleChunkSize()
        {
            PutTestExecute(_chunkSize*2+1);
        }


        #region Private Service Call Methods

        private void PutTestExecute(int sendChunkSize)
        {
            var testData = GetTestData();

            //Get status of Web service
            var status = GetStatus(testData.StatusCallUri);

            Assert.IsTrue(status, "Service health check failed");

            int chunkSize = sendChunkSize;
            //post file and get guid
            var fileGuid = PostFileNonMultipart(testData.FilesUriCall, testData.AttachmentFileName, true, chunkSize);

            fileGuid = fileGuid.Replace("\"", string.Empty);

            PutFile(testData.FilesUriCall, fileGuid, testData.AttachmentFileName, chunkSize, chunkSize, true);


            //Call Head Method
            CheckGetHead(testData.FilesUriCall, fileGuid, testData.AttachmentFileName, sentChunkSize: chunkSize);

            //Download file
            DownloadUploadedFile(testData.FilesUriCall, fileGuid, testData.AttachmentFileName);

            //Delete File
            DeleteFile(testData.FilesUriCall, fileGuid);

            //Try to call methods again again to ensure that NotFound is returned
            CheckGetHead(testData.FilesUriCall, fileGuid, testData.AttachmentFileName, true);
            DownloadUploadedFile(testData.FilesUriCall, fileGuid, testData.AttachmentFileName, true);
            DeleteFile(testData.FilesUriCall, fileGuid, true);
        }
        private bool GetStatus(string statusCallUri)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(statusCallUri);
            fetchRequest.Method = "Get";
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

            Assert.IsNotNull(objResponse, "objResponse is null");
            return objResponse.StatusCode == HttpStatusCode.OK;
        }

        private string PostFile(string postCallUri, string attachmentFileName)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(postCallUri);
            fetchRequest.Accept = "application/json";
            fetchRequest.Method = "POST";
            fetchRequest.KeepAlive = true;
            fetchRequest.Credentials = CredentialCache.DefaultCredentials;
            string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes(boundary);

            fetchRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            using (Stream rs = fetchRequest.GetRequestStream())
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string headerTemplate = "\r\nContent-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "attachment", attachmentFileName, "image/bmp");
                byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
                rs.Write(headerbytes, 0, headerbytes.Length);

                using (var fileStream = new FileStream(attachmentFileName, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[4096];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                }

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n" + boundary + "\r\n");
                rs.Write(trailer, 0, trailer.Length);
            }

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

            Assert.IsNotNull(objResponse, "objResponse should not be null");
            Assert.IsTrue(objResponse.StatusCode == HttpStatusCode.OK, "Uploading of file to FileStore service failed");
            string fileGuid;
            var stream = objResponse.GetResponseStream();
            using (var reader = new StreamReader(stream))
            {
                fileGuid = reader.ReadToEnd();
            }

            return fileGuid;
        }

        private string PostFileNonMultipart(string postCallUri, string attachmentFileName, bool addContentHeaders, int bytesToUpload = Int32.MaxValue)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(postCallUri);
            fetchRequest.Accept = "*/*";
            fetchRequest.Method = "POST";
            fetchRequest.KeepAlive = true;
            if (addContentHeaders)
            {
                string headerTemplate =
                    "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
                string header = string.Format(headerTemplate, "attachment", attachmentFileName, "image/bmp");
                fetchRequest.ContentType = "image/bmp";
                var headers = new WebHeaderCollection();
                var nameValueCollection = new NameValueCollection
                {
                    {"content-disposition", string.Format("attachment;filename=\"{0}\"", attachmentFileName)}
                };
                headers.Add(nameValueCollection);
                fetchRequest.Headers.Add(headers);
            }
            using (Stream rs = fetchRequest.GetRequestStream())
            {
                using (var fileStream = new FileStream(attachmentFileName, FileMode.Open, FileAccess.Read))
                {
                    var buffer = new byte[4096];
                    long totalBytesRead = 0;
                    int bytesToRead = buffer.Length;
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, bytesToRead)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        if (totalBytesRead == bytesToUpload)
                        {
                            break;
                        }
                        if (totalBytesRead + bytesToRead > bytesToUpload)
                        {
                            bytesToRead = (int)(bytesToUpload - totalBytesRead);
                    }
                }
            }
            }

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
            if (!addContentHeaders)
            {
                Assert.IsTrue(objResponse.StatusCode == HttpStatusCode.BadRequest, "Bad Request should be returned");
                return null;
            }
            Assert.IsTrue(objResponse.StatusCode == HttpStatusCode.OK, "Uploading of file to FileStore service failed");
            string fileGuid;
            var stream = objResponse.GetResponseStream();
            using (var reader = new StreamReader(stream))
            {
                fileGuid = reader.ReadToEnd();
            }

            return fileGuid;
        }

        private int PutRequest(string uri, string attachmentFileName, int offset, int bytesToUpload)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(uri);
            fetchRequest.Accept = "application/json";
            fetchRequest.Method = "Put";
            fetchRequest.KeepAlive = true;
            fetchRequest.Credentials = CredentialCache.DefaultCredentials;

            using (var rs = fetchRequest.GetRequestStream())
            {
                using (var fileStream = new FileStream(attachmentFileName, FileMode.Open, FileAccess.Read))
                {
                    fileStream.Position = offset;
                    var buffer = new byte[4096];
                    int readSize = buffer.Length;
                    int totalBytesRead = 0;
                    int bytesRead;
                    for (bytesRead = fileStream.Read(buffer, 0, readSize);
                        bytesRead > 0;
                        bytesRead = fileStream.Read(buffer, 0, readSize))
                    {
                        rs.Write(buffer, 0, bytesRead);
                        totalBytesRead += bytesRead;
                        if (totalBytesRead == bytesToUpload)
                        {
                            var putResponse = fetchRequest.GetResponse() as HttpWebResponse;// Source of xml
                            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode, "Service returned invalid Put for file which is expected to exist");
                            return totalBytesRead;
                        }
                        if (totalBytesRead + readSize > bytesToUpload)
                        {
                            readSize = (int)(bytesToUpload - totalBytesRead);
                        }
                    }
                    if (totalBytesRead > 0)
                    {
                        var putResponse = fetchRequest.GetResponse() as HttpWebResponse;// Source of xml
                        Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode, "Service returned invalid Put for file which is expected to exist");
                        return totalBytesRead;
                    }
                }
            }
            return 0;
        }
        private void PutFile(string putCallUri, string fileGuid, string attachmentFileName, int putSize, int offset, bool expectedToFail = false)
        {
            string uri = string.Format("{0}{1}", putCallUri, fileGuid);
            for(int bytesRead = PutRequest(uri, attachmentFileName, offset, putSize);
                bytesRead > 0; 
                bytesRead = PutRequest(uri, attachmentFileName, offset, putSize))
            {
                offset += bytesRead;
            }
        }

        private void CheckGetHead(string filesUriCall, string fileGuid, string originalFileName, bool expectedToFail = false, int sentChunkSize = _chunkSize)
        {
            string uri = string.Format("{0}{1}", filesUriCall, fileGuid);
            var fetchRequest = (HttpWebRequest)WebRequest.Create(uri);
            fetchRequest.Method = "Head";
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

            if (expectedToFail)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, objResponse.StatusCode, "Service returned valid Head for file which is expected to not exist");
                return;
            }

            Assert.AreEqual(HttpStatusCode.OK, objResponse.StatusCode, "Service returned invalid Head for file which is expected to exist");

            var contentDispositionHeader = objResponse.Headers["Content-Disposition"];
            Assert.AreEqual(string.Format("attachment; filename={0}", originalFileName), contentDispositionHeader,
                "Service content displosition header value is different");

            var contentType = objResponse.Headers["Content-Type"];
            Assert.AreEqual("image/bmp", contentType, "Service content type value does not match");

            var fileInfo = new FileInfo(originalFileName);
            var fileSize = fileInfo.Length;

            var contentLength = objResponse.Headers["File-Size"];
            int actualFileSize;
            int.TryParse(contentLength, out actualFileSize);
            Assert.AreEqual(fileSize, actualFileSize, "Service file size value does not exist");

            int actualChunkCount;
            int.TryParse(objResponse.Headers["File-Chunk-Count"], out actualChunkCount);
            int expectedChunkCount = GetChunkCountFromSentChunkSize(actualFileSize, sentChunkSize);//(int)Math.Ceiling((double)actualFileSize/(sentChunkSize));
            Assert.AreEqual(expectedChunkCount, actualChunkCount, "Service file chunk counts do not match");
        }

        private void DownloadUploadedFile(string filesUriCall, string fileGuid, string originalFileName, bool expectedToFail = false)
        {
            string uri = string.Format("{0}{1}", filesUriCall, fileGuid);
            var fetchRequest = (HttpWebRequest)WebRequest.Create(uri);
            fetchRequest.Method = "Get";
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

            if (expectedToFail)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, objResponse.StatusCode);
                return;
            }

            Assert.AreEqual(HttpStatusCode.OK, objResponse.StatusCode);

            var contentDispositionHeader = objResponse.Headers["Content-Disposition"];
            Assert.AreEqual(string.Format("attachment; filename={0}", originalFileName), contentDispositionHeader,
                "Service content displosition header value is different");

            var contentType = objResponse.Headers["Content-Type"];
            Assert.AreEqual("image/bmp", contentType, "Service content type value does not match");

            var fileInfo = new FileInfo(originalFileName);
            var fileSize = fileInfo.Length;

            var contentLength = objResponse.Headers["Content-Length"];
            int actualFileSize;
            int.TryParse(contentLength, out actualFileSize);
            Assert.AreEqual(fileSize, actualFileSize, "Service file size value does not exist");

            string expectedMD5;
            string actualMD5;

            using (var expectedFileStream = fileInfo.OpenRead())
            {
                expectedMD5 = GetMD5ForFileStream(expectedFileStream);
            }

            using (var stream = objResponse.GetResponseStream())
            {
                actualMD5 = GetMD5ForFileStream(stream);
            }

            Assert.AreEqual(expectedMD5, actualMD5, "Downloaded File content does not match uploaded file content");
        }

        private void DeleteFile(string filesUriCall, string fileGuid, bool expectedToFail = false)
        {
            string uri = string.Format("{0}{1}", filesUriCall, fileGuid);
            var fetchRequest = (HttpWebRequest)WebRequest.Create(uri);
            fetchRequest.Method = "Delete";
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

            if (expectedToFail)
            {
                Assert.AreEqual(HttpStatusCode.NotFound, objResponse.StatusCode, "Non-existent file was deleted by server");
                return;
            }

            Assert.AreEqual(HttpStatusCode.OK, objResponse.StatusCode, "Existent file was deleted by server");
            string actualDeletedFileGuid;
            var stream = objResponse.GetResponseStream();
            using (var reader = new StreamReader(stream))
            {
                actualDeletedFileGuid = reader.ReadToEnd();
            }

            Assert.AreEqual(fileGuid, actualDeletedFileGuid.Replace("\"",string.Empty), "Incorrect file was deleted by FileStoreService");
        }

        #endregion

        #region Helper Methods

        private int GetChunkCountFromSentChunkSize(int actualFileSize, int sentChunkSize)
        {
            int remainingFileSize = actualFileSize;
            int chunkCounts = 0;
            
            while (remainingFileSize > 0)
            {
                if (remainingFileSize < sentChunkSize)
                {
                    chunkCounts += (int)Math.Abs(Math.Ceiling((double)remainingFileSize / _chunkSize));
                }
                else
                {
                    chunkCounts += (int)Math.Abs(Math.Ceiling((double)sentChunkSize/_chunkSize ));
                }
                remainingFileSize -= sentChunkSize;
            }
            return chunkCounts;
        }
        private string GetMD5ForFileStream(Stream stream)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
            }
        }
        private TestData GetTestData()
        {
            TestData testData = new TestData();
            if (TestContext.DataRow.Table.Columns.Contains("FilesUriCall"))
            {
                testData.FilesUriCall = Convert.ToString(TestContext.DataRow["FilesUriCall"]);
            }

            if (TestContext.DataRow.Table.Columns.Contains("AttachmentFileName"))
            {
                testData.AttachmentFileName = Convert.ToString(TestContext.DataRow["AttachmentFileName"]);
        }

            if (TestContext.DataRow.Table.Columns.Contains("StatusUriCall"))
            {
                testData.StatusCallUri = Convert.ToString(TestContext.DataRow["StatusUriCall"]);
            }
            return testData;
        }
        #endregion
    }

    public class TestData
    {
        public string StatusCallUri { get; set; }
        public string FilesUriCall { get; set; }
        public string AttachmentFileName { get; set; }
    }
}
