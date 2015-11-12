using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Helpers
{
    [TestClass]
    public class SystemEncryptionsTests
    {
        #region Encrypt

        [TestMethod]
        public void Encrypt_Input_CorrectResult()
        {
            // Arrange
            string input = "Encrypt this!";

            // Act
            string result = SystemEncryptions.Encrypt(input);

            // Assert
            Assert.AreEqual("0vhy8lC3RZxzjPEyoAm1Dw==", result);
        }

        #endregion Encrypt

        #region Decrypt

        [TestMethod]
        public void Decrypt_Input_CorrectResult()
        {
            // Arrange
            string input = "fb0caYSrKYtfjm/+Hy4D2w==";

            // Act
            string result = SystemEncryptions.Decrypt(input);

            // Assert
            Assert.AreEqual("This is a test.", result);
        }

        #endregion Decrypt
    }
}
