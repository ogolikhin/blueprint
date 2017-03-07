﻿using AdminStore.Models;
using MailBee.Mime;
using MailBee.SmtpMail;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace AdminStore.Helpers
{
    [TestClass]
    public class EmailHelperTests
    {

        [TestMethod]
        public void EmailHelperTests_SendEmail_CorrectResult()
        {
            //Arange
            var toEmail = "email@test.com";
            var fromEmail = "from@test.com";

            //Act
            MailMessage message = EmailHelper.PreparePasswordResetMessage(toEmail, fromEmail);

            //Assert
            Assert.AreEqual(toEmail, message.To[0].Email);
            Assert.AreEqual(fromEmail, message.From.Email);
        }
    }
}
