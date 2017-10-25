using System;
using System.Linq;
using MailBee.Mime;
using MailBee.Security;
using MailBee.SmtpMail;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Email;

namespace BlueprintSys.RC.Services.MessageHandlers.Notifications
{
    public class SmtpClient
    {
        #region Constructors
        public SmtpClient(SMTPClientConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            Configuration = configuration;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Synchronous e-mail sending
        /// </summary>
        /// <param name="message"></param>
        public void SendEmail(Message message)
        {
            SendMailBeeMessage(message);
        }

        #endregion

        #region Private Methods, Vatiables and Properies

        private SslStartupMode SslStartupMode
        {
            get
            {
                if (Configuration.EnableSsl && Configuration.Port == 465/*Implicit SSL Port*/)
                    return SslStartupMode.OnConnect;

                if (!Configuration.EnableSsl)
                    return SslStartupMode.Manual;

                return SslStartupMode.UseStartTls;
            }
        }

        private SmtpServer SmtpServer
        {
            get
            {
                SmtpServer smtpServer = new SmtpServer
                {
                    Name = Configuration.HostName,
                    Port = Configuration.Port,
                    Timeout = 100000
                };
                //default 100 secs
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

        private void SendMailBeeMessage(Message message)
        {
            var smtpServer = SmtpServer;

            var mailMessage = new MailMessage();
            if (!string.IsNullOrWhiteSpace(message.FromDisplayName))
            {
                mailMessage.From.DisplayName = $"\"{message.FromDisplayName}\" <{message.From}>";
            }
            mailMessage.From.Email = message.From;
            message.To.ForEach(email => mailMessage.To.Add(email));
            mailMessage.Subject = message.Subject;
            if (message.IsBodyHtml)
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
                        DiscussionEmail.LogoImageAttachmentContentId, DiscussionEmail.LogoImageAttachmentContentId,
                        null, null, NewAttachmentOptions.Inline | NewAttachmentOptions.ReplaceIfExists, MailTransferEncoding.Base64);
                }
                if (message.DiscussionEmail.ArtifactImageAttachmentArray != null)
                {
                    mailMessage.Attachments.Add(message.DiscussionEmail.ArtifactImageAttachmentArray.ToArray(),
                        DiscussionEmail.ArtifactImageAttachmentContentId, DiscussionEmail.ArtifactImageAttachmentContentId,
                        null, null, NewAttachmentOptions.Inline | NewAttachmentOptions.ReplaceIfExists, MailTransferEncoding.Base64);
                }
            }

            var smtp = new Smtp();
            smtp.SmtpServers.Add(smtpServer);
            smtp.Message = mailMessage;
            smtp.Send();
        }

        #endregion

        #region Properties
        public SMTPClientConfiguration Configuration { get; set; }

        #endregion

    }
}
