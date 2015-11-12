using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace AdminStore.Helpers
{
    [TestClass]
    public class HashingUtilitiesTests
    {
        #region GenerateSaltedHash

        [TestMethod]
        public void GenerateSaltedHash_StringSalt_CorrectResult()
        {
            // Arrange
            string plainText = "plainText";
            string salt = "salt";

            // Act
            string result = HashingUtilities.GenerateSaltedHash(plainText, salt);

            // Assert
            Assert.AreEqual("JHlkKopHjdRv7Q3nk2deN+q5TYgdKwff9Qr+zJii+7A=", result);
        }

        [TestMethod]
        public void GenerateSaltedHash_GuidSalt_CorrectResult()
        {
            // Arrange
            string plainText = "text to hash";
            Guid salt = new Guid("66666666666666666666666666666666");

            // Act
            string result = HashingUtilities.GenerateSaltedHash(plainText, salt);

            // Assert
            Assert.AreEqual("tkoaNHI1k9elu0cqa27l2QCb9ORRK+qNQzXSEt5Tslo=", result);
        }

        #endregion GenerateSaltedHash
    }
}
