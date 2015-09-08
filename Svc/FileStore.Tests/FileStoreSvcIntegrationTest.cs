using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;
using System.IO;

namespace FileStore.Tests
{
    [TestClass]
    public class FileStoreSvcIntegrationTest
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [TestCategory("Api-Integration")]
        public void TestUploadAndDeleteFiles()
        {
            var postCallUri = "";
            if (TestContext.DataRow.Table.Columns.Contains("FilesUriCall"))
            {
                postCallUri = Convert.ToString(TestContext.DataRow["FilesUriCall"]);
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

            var status = GetStatus(statusCallUri);

            Assert.IsTrue(status);

            var fileGuid = PostFile(postCallUri, attachmentFileName);

            Assert.IsNotNull(fileGuid);

            
        }

        private bool GetStatus(string statusCallUri)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(statusCallUri);
            fetchRequest.Method = "Get";
            fetchRequest.Accept = "application/json";
            fetchRequest.KeepAlive = true;
            fetchRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;

            HttpWebResponse objResponse = null;
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

            Assert.IsTrue(objResponse.StatusCode == HttpStatusCode.OK);
            bool isRunning = false;
            using (var stream = objResponse.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    var value = reader.ReadToEnd();
                    bool.TryParse(value, out isRunning);
                }
            }

            return isRunning;
        }

        private string PostFile(string postCallUri, string attachmentFileName)
        {
            var fetchRequest = (HttpWebRequest)WebRequest.Create(postCallUri);
            fetchRequest.Accept = "application/json";
            fetchRequest.Method = "POST";
            fetchRequest.KeepAlive = true;
            fetchRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
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
                    int bytesRead = 0;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        rs.Write(buffer, 0, bytesRead);
                    }
                }

                byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
                rs.Write(trailer, 0, trailer.Length);
            }

            HttpWebResponse objResponse = null;
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

            Assert.IsTrue(objResponse.StatusCode == HttpStatusCode.OK);
            string fileGuid = null;
            using (var stream = objResponse.GetResponseStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    fileGuid = reader.ReadToEnd();
                }
            }

            return fileGuid;
        }
    }
}
