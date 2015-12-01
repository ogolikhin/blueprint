using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Helpers
{
    [TestClass]
    public class SystemEncryptionsTests
    {
        #region Encrypt

        [TestMethod]
        public void Encrypt_Null_ReturnsNull()
        {
            // Arrange

            // Act
            string result = SystemEncryptions.Encrypt(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Encrypt_NonEmpty_ReturnsResult()
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
        public void Decrypt_Null_ReturnsNull()
        {
            // Arrange

            // Act
            string result = SystemEncryptions.Decrypt(null);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void Decrypt_NonEmpty_ReturnsResult()
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
