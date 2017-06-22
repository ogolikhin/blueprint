using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers.Security;

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
            Assert.AreEqual("AyziEDAP1KDcBevH2skTdQ==", result);
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
            string input = "syXE1v5RHe6bNf8wzYDw6w==";

            // Act
            string result = SystemEncryptions.Decrypt(input);

            // Assert
            Assert.AreEqual("This is a test.", result);
        }

        #endregion Decrypt
    }
}
