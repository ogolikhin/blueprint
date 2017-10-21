using MailBee.Mime;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdminStore.Helpers
{
    [TestClass]
    public class EmailHelperTests
    {

        [TestMethod]
        public void EmailHelperTests_SendEmail_CorrectResult()
        {
            // Arange
            var toEmail = "email@test.com";
            var fromEmail = "from@test.com";

            // Act
            MailMessage message = EmailHelper.PrepareMessage(toEmail, fromEmail, string.Empty, string.Empty);

            // Assert
            Assert.AreEqual(toEmail, message.To[0].Email);
            Assert.AreEqual(fromEmail, message.From.Email);
        }
    }
}
