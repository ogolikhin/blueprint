using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FileStore.Tests
{
    [TestClass]
    class FileStoreSveIntegrationTest
    {

        public TestContext TestContext { get; set; }


        [TestMethod]
        [DataSource("Microsoft.VisualStudio.TestTools.DataSource.CSV", "TestUploadAndDeleteFiles.csv", "TestUploadAndDeleteFiles#csv", DataAccessMethod.Sequential)]
        [TestCategory("Api-Integration")]
        public void TestUploadAndDeleteFiles()
        {
            var BaseURI = "";
            if (TestContext.DataRow.Table.Columns.Contains("BaseUriForApi"))
            {
                BaseURI = Convert.ToString(TestContext.DataRow["BaseURI"]);
            }
            BaseURI = "";
            //    //Call REST API
            //    var fetchRequest = (HttpWebRequest)WebRequest.Create(string.Concat("http://localhost:52618", " / ", APICall));
            //    fetchRequest.Method = httpMethod;
            //    if (loadAuthenticationHeader)
            //    {
            //        fetchRequest.Headers.Add("Authorization", string.Format("{0} {1}", AuthenticationConstants.BlueprintTokenScheme, AuthenticationToken));
            //    }
            //    if (headersToAdd != null && headersToAdd.Count > 0)
            //    {
            //        foreach (var hdr in headersToAdd.AllKeys)
            //        {
            //            fetchRequest.Headers.Add(hdr, headersToAdd[hdr]);
            //        }
            //    }
            //    fetchRequest.Accept = "application/xml";
            //    fetchRequest.Method = "POST";
            //    fetchRequest.KeepAlive = true;
            //    fetchRequest.Credentials = System.Net.CredentialCache.DefaultCredentials;
            //    string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
            //    byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            //    fetchRequest.ContentType = "multipart/form-data; boundary=" + boundary;
            //    using (Stream rs = fetchRequest.GetRequestStream())
            //    {
            //        if (nvc != null)
            //        {
            //            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            //            foreach (string key in nvc.Keys)
            //            {
            //                rs.Write(boundarybytes, 0, boundarybytes.Length);
            //                string formitem = string.Format(formdataTemplate, key, nvc[key]);
            //                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
            //                rs.Write(formitembytes, 0, formitembytes.Length);
            //            }
            //        }
            //        rs.Write(boundarybytes, 0, boundarybytes.Length);
            //        string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
            //        string header = string.Format(headerTemplate, paramName, file, contentType);
            //        byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            //        rs.Write(headerbytes, 0, headerbytes.Length);

            //        using (var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
            //        {
            //            var buffer = new byte[4096];
            //            int bytesRead = 0;
            //            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            //            {
            //                rs.Write(buffer, 0, bytesRead);
            //            }
            //        }

            //        byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
            //        rs.Write(trailer, 0, trailer.Length);
            //    }

            //    HttpWebResponse objResponse = null;
            //    try
            //    {
            //        objResponse = fetchRequest.GetResponse() as HttpWebResponse;// Source of xml
            //    }
            //    catch (WebException e)
            //    {
            //        if (e.Response == null)
            //        {
            //            throw;
            //        }
            //        objResponse = (HttpWebResponse)e.Response;
            //    }
            //    return objResponse;
            //}
        }
    }
}
