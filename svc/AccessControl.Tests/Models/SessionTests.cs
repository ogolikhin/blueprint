using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AccessControl.Models
{
    [TestClass]
    public class SessionTests
    {
        [TestMethod]
        public void Convert_Guid_ReturnsCorrectlyFormattedString()
        {
            // Arrange
            var guid = new Guid("12345678901234567890123456789012");

            // Act
            string result = Session.Convert(guid);

            // Assert
            var expected = "12345678901234567890123456789012";
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void Convert_CorrectlyFormattedString_ReturnsGuid()
        {
            // Arrange
            string val = "12345678901234567890123456789012";

            // Act
            Guid result = Session.Convert(val);

            // Assert
            var expected = new Guid("12345678901234567890123456789012");
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Convert_IncorrectlyFormattedString_ThrowsException()
        {
            // Arrange
            string val = "12345678-9012-3456-7890-123456789012";

            // Act
            Session.Convert(val);

            // Assert
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Convert_NullString_ThrowsException()
        {
            // Arrange

            // Act
            Session.Convert(null);

            // Assert
        }
    }
}
