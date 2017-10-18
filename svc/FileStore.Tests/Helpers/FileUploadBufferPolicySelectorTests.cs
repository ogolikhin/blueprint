using System;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileStore.Helpers
{
    [TestClass]
    public class FileUploadBufferPolicySelectorTests
    {
        [TestMethod]
        public void UseBufferedInputStream_ReturnsFalse()
        {
            // Arange
            var fileUploadBufferPolicySelector = new FileUploadBufferPolicySelector();

            // Act
            var result = fileUploadBufferPolicySelector.UseBufferedInputStream(null);

            // Assert
            Assert.IsFalse(result, "FileUploadBufferPolicySelector should return false for UseBufferedInputStream");
        }

        [TestMethod]
        public void UseBufferedOutputStream_ReturnsFalse()
        {
            // Arange
            var fileUploadBufferPolicySelector = new FileUploadBufferPolicySelector();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK);

            // Act
            var result = fileUploadBufferPolicySelector.UseBufferedOutputStream(response);

            // Assert
            Assert.IsFalse(result, "UseBufferedOutputStream should return false for UseBufferedInputStream");
        }

        [TestMethod]
        public void UseBufferedOutputStream_ReturnsTrue()
        {
            // Arange
            var fileUploadBufferPolicySelector = new FileUploadBufferPolicySelector();
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);

            // Act
            var result = fileUploadBufferPolicySelector.UseBufferedOutputStream(response);

            // Assert
            Assert.IsTrue(result, "UseBufferedOutputStream should return true for UseBufferedInputStream");
        }
    }
}
