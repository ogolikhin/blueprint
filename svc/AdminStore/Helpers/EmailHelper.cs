﻿using System;
using System.Globalization;
using AdminStore.Models;
using MailBee.Mime;
using MailBee.Security;
using MailBee.SmtpMail;

namespace AdminStore.Helpers
{
    public interface IEmailHelper
    {
        void Initialize(IEmailConfigInstanceSettings configuration);
        void SendEmail(AuthenticationUser user);
        
    }

    public class EmailHelper : IEmailHelper
    {
        private static readonly string MessageBody = @"
<html>
    <div>Hello {0}.</div>
    <br>
    <div>We have received a request to reset your password.</div>
    <br>
    <div>To confirm this password reset, visit the following address:</div>
    <a href='javascript:void()'>&lt;link&gt;</a>
    <br><br>
    <div>If you did not make this request, you can ignore this email, and no changes will be made.</div>
    <br>
    <div>If you have any questions, please contact your administrator. </div>
</html>
";
        private IEmailConfigInstanceSettings _configuration { get; set; }
        public EmailHelper()
        {
        }

        public void Initialize(IEmailConfigInstanceSettings configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _configuration = configuration;
            MailBee.Global.AutodetectPortAndSslMode = false;
            MailBee.Global.LicenseKey = "MN800-02CA3564CA2ACAAECAB17D4ADEC9-145F";
        }

        public void SendEmail(AuthenticationUser user)
        {
            var smtpServer = SmtpServer;
            var smtp = new Smtp();
            smtp.SmtpServers.Add(smtpServer);
            smtp.Message = PreparePasswordResetMessage(user.DisplayName, user.Email, _configuration.UserName);
            smtp.Send();
        }

        internal static MailMessage PreparePasswordResetMessage(string displayName, string toEmail, string fromEmail)
        {
            var mailMessage = new MailMessage();
            mailMessage.To.Add(toEmail);
            mailMessage.From.Email = fromEmail;
            mailMessage.Subject = "password reset";
            mailMessage.BodyHtmlText = string.Format(CultureInfo.InvariantCulture, MessageBody, displayName);
            return mailMessage;
        }

        private SmtpServer SmtpServer
        {
            get
            {
                SmtpServer smtpServer = new SmtpServer();
                smtpServer.Name = _configuration.HostName;
                smtpServer.Port = _configuration.Port;
                smtpServer.Timeout = 100000; //default 100 secs
                if (_configuration.Authenticated)
                {
                    smtpServer.AccountName = _configuration.UserName;
                    smtpServer.Password = SystemEncryptions.DecryptFromSilverlight(_configuration.Password);
                    //MailBee.AuthenticationMethods.None by default
                    smtpServer.AuthMethods = MailBee.AuthenticationMethods.Auto;
                }
                smtpServer.SslMode = SslStartupMode;
                return smtpServer;
            }
        }

        private SslStartupMode SslStartupMode
        {
            get
            {
                if (_configuration.EnableSSL && _configuration.Port == 465/*Implicit SSL Port*/)
                    return SslStartupMode.OnConnect;

                if (!_configuration.EnableSSL)
                    return SslStartupMode.Manual;

                return SslStartupMode.UseStartTls;
            }
        }
    }
}