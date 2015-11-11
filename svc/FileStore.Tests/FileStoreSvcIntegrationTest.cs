using System;
using System.Collections.Specialized;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;
using FileStore.Repositories;

namespace FileStore
{
    [TestClass]
    public class FileStoreSvcIntegrationTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [TestCategory("Integration")]
        public async Task TestUploadAndDeleteFilesUsingMultipart()
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
            await new SqlFilesRepository().DeleteFile(Models.File.ConvertToStoreId(fileGuid));

            //Try to call methods again again to ensure that NotFound is returned
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName, true);
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName, true);
            DeleteFile(filesUriCall, fileGuid, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [TestCategory("Integration")]
        public async Task TestUploadAndDeleteFilesUsingNonMultipart()
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
            await new SqlFilesRepository().DeleteFile(Models.File.ConvertToStoreId(fileGuid));

            //Try to call methods again again to ensure that NotFound is returned
            CheckGetHead(filesUriCall, fileGuid, attachmentFileName, true);
            DownloadUploadedFile(filesUriCall, fileGuid, attachmentFileName, true);
            DeleteFile(filesUriCall, fileGuid, true);
        }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [TestCategory("Integration")]
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

        #region Private Service Call Methods

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
            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            fetchRequest.ContentType = "multipart/form-data; boundary=" + boundary;

            using (Stream rs = fetchRequest.GetRequestStream())
            {
                rs.Write(boundarybytes, 0, boundarybytes.Length);
                string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
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

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
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

        private string PostFileNonMultipart(string postCallUri, string attachmentFileName, bool addContentHeaders)
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
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
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

        private void CheckGetHead(string filesUriCall, string fileGuid, string originalFileName, bool expectedToFail = false)
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
            Assert.AreEqual("attachment; filename=BitmapAttachment.bmp", contentDispositionHeader, 
                "Service content displosition header value is different");

            var contentType = objResponse.Headers["Content-Type"];
            Assert.AreEqual("image/bmp", contentType, "Service content type value does not match");

            var fileInfo = new FileInfo(originalFileName);
            var fileSize = fileInfo.Length;

            var contentLength = objResponse.Headers["File-Size"];
            int actualFileSize;
            int.TryParse(contentLength, out actualFileSize);
            Assert.AreEqual(fileSize, actualFileSize, "Service file size value does not exist");
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
            Assert.AreEqual("attachment; filename=BitmapAttachment.bmp", contentDispositionHeader,
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
                Assert.AreEqual(HttpStatusCode.MethodNotAllowed, objResponse.StatusCode, "Non-existent file was deleted by server");
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
