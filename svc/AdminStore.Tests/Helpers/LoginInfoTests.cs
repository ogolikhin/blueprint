using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Helpers
{
    [TestClass]
    public class LoginInfoTests
    {
        #region Parse

        [TestMethod]
        public void Parse_NullString_ReturnsEmpty()
        {
            // Arrange

            // Act
            LoginInfo result = LoginInfo.Parse(null);

            // Assert
            Assert.AreEqual(new LoginInfo(), result);
        }

        [TestMethod]
        public void Parse_WithDomain_ReturnsInfo()
        {
            // Arrange
            string login = "domain\\user";

            // Act
            LoginInfo result = LoginInfo.Parse(login);

            // Assert
            Assert.AreEqual(new LoginInfo { Domain = "domain", UserName = "user", Login = login }, result);
        }

        [TestMethod]
        public void Parse_WithoutDomain_ReturnsInfo()
        {
            // Arrange
            string login = "login";

            // Act
            LoginInfo result = LoginInfo.Parse(login);

            // Assert
            Assert.AreEqual(new LoginInfo { Domain = null, UserName = login, Login = login }, result);
        }

        #endregion Parse
    }
}
