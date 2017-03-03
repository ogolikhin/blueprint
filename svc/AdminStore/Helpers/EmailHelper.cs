using System;
using MailBee.Mime;
using MailBee.Security;
using MailBee.SmtpMail;

namespace AdminStore.Helpers
{
    public class EmailHelper
    {
        public SmtpClientConfiguration Configuration { get; set; }
        public EmailHelper(SmtpClientConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            Configuration = configuration;
        }

        /// <summary>
        /// Synchronous e-mail sending
        /// </summary>
        public void SendEmail()
        {
            SendMailBeeMessage();
        }

        private void SendMailBeeMessage()
        {
            var smtpServer = SmtpServer;

            var mailMessage = new MailMessage();
            /*if (!string.IsNullOrWhiteSpace(message.FromDisplayName))
            {
                mailMessage.From.DisplayName = String.Format("\"{0}\" <{1}>", message.FromDisplayName, message.From);
            }
            mailMessage.From.Email = message.From;
            message.To.ForEach(email => mailMessage.To.Add(email));
            mailMessage.Subject = message.Subject;
            if (message.IsBodyHTML)
            {
                mailMessage.BodyHtmlText = message.Body;
            }
            else
            {
                mailMessage.BodyPlainText = message.Body;
            }

            if (message.DiscussionEmail != null)
            {
                if (message.DiscussionEmail.LogoImageAttachmentArray != null)
                {
                    mailMessage.Attachments.Add(message.DiscussionEmail.LogoImageAttachmentArray,
                        DDiscussionEmail.LogoImageAttachmentContentId, DDiscussionEmail.LogoImageAttachmentContentId,
                        null, null, NewAttachmentOptions.Inline | NewAttachmentOptions.ReplaceIfExists, MailTransferEncoding.Base64);
                }
                if (message.DiscussionEmail.ArtifactImageAttachmentArray != null)
                {
                    mailMessage.Attachments.Add(message.DiscussionEmail.ArtifactImageAttachmentArray,
                        DDiscussionEmail.ArtifactImageAttachmentContentId, DDiscussionEmail.ArtifactImageAttachmentContentId,
                        null, null, NewAttachmentOptions.Inline | NewAttachmentOptions.ReplaceIfExists, MailTransferEncoding.Base64);
                }
            }*/

            var smtp = new Smtp();
            smtp.SmtpServers.Add(smtpServer);
            smtp.Message = mailMessage;
            smtp.Send();
        }

        private SmtpServer SmtpServer
        {
            get
            {
                SmtpServer smtpServer = new SmtpServer();
                smtpServer.Name = Configuration.HostName;
                smtpServer.Port = Configuration.Port;
                smtpServer.Timeout = 100000; //default 100 secs
                if (Configuration.Authenticated)
                {
                    smtpServer.AccountName = Configuration.UserName;
                    smtpServer.Password = Configuration.Password;
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
                if (Configuration.EnableSSL && Configuration.Port == 465/*Implicit SSL Port*/)
                    return SslStartupMode.OnConnect;

                if (!Configuration.EnableSSL)
                    return SslStartupMode.Manual;

                return SslStartupMode.UseStartTls;
            }
        }
    }
}