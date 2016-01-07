using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccessControl.Helpers
{
    [TestClass]
    public class LicenceHelperTests
    {
        #region GetLicenseHoldTime

        [TestMethod]
        public void GetLicenseHoldTime_NullString_ReturnsDefault()
        {
            // Arrange
            int licenseHoldTimeDefault = 1234;

            // Act
            var result = LicenceHelper.GetLicenseHoldTime(null, licenseHoldTimeDefault);

            // Assert
            Assert.AreEqual(licenseHoldTimeDefault, result);
        }

        [TestMethod]
        public void GetLicenseHoldTime_EmptyString_ReturnsDefault()
        {
            // Arrange
            string encryptedLicenceHoldTimerString = string.Empty;
            int licenseHoldTimeDefault = 1234;

            // Act
            var result = LicenceHelper.GetLicenseHoldTime(encryptedLicenceHoldTimerString, licenseHoldTimeDefault);

            // Assert
            Assert.AreEqual(licenseHoldTimeDefault, result);
        }

        [TestMethod]
        public void GetLicenseHoldTime_NonBase64String_ReturnsDefault()
        {
            // Arrange
            string encryptedLicenceHoldTimerString = "\0";
            int licenseHoldTimeDefault = 1234;

            // Act
            var result = LicenceHelper.GetLicenseHoldTime(encryptedLicenceHoldTimerString, licenseHoldTimeDefault);

            // Assert
            Assert.AreEqual(licenseHoldTimeDefault, result);
        }

        [TestMethod]
        public void GetLicenseHoldTime_EncryptedString_ReturnsDecryptedValue()
        {
            // Arrange
            string encryptedLicenceHoldTimerString = "z1CEEZ3i9Yj4RcIXSmbWfA==";
            int licenseHoldTimeDefault = 1234;

            // Act
            var result = LicenceHelper.GetLicenseHoldTime(encryptedLicenceHoldTimerString, licenseHoldTimeDefault);

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion GetLicenseHoldTime
    }
}
