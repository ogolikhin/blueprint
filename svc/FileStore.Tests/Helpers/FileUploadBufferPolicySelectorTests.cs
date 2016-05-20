using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileStore.Helpers
{
    [TestClass]
    public class FileUploadBufferPolicySelectorTests
    {
        [TestMethod]
        public void UseBufferedInputStream_ReturnsFalse()
        {
            //Arange
            var fileUploadBufferPolicySelector = new FileUploadBufferPolicySelector();

            //Act
            var result = fileUploadBufferPolicySelector.UseBufferedInputStream(null);

            //Assert
            Assert.IsFalse(result, "FileUploadBufferPolicySelector should return false for UseBufferedInputStream");
        }

        [TestMethod]
        public void UseBufferedOutputStream_ReturnsFalse()
        {
            //Arange
            var fileUploadBufferPolicySelector = new FileUploadBufferPolicySelector();

            //Act
            var result = fileUploadBufferPolicySelector.UseBufferedOutputStream(null);

            //Assert
            Assert.IsFalse(result, "UseBufferedOutputStream should return false for UseBufferedInputStream");
        }
    }
}
