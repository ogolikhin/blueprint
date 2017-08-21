using System;
using MailBee;
using MailBee.ImapMail;

namespace AdminStore.Services.Email
{
    public class ImapEmailClient : IEmailClient
    {
        private readonly Imap _imap;

        public bool UseSsl { get; set; }

        public ImapEmailClient()
        {
            _imap = new Imap();
        }

        public void Connect(string serverAddress, int port)
        {
            _imap.Connect(serverAddress, port);
        }

        public void Login(string userName, string password)
        {
            if (UseSsl)
            {
                _imap.Login(userName, password, AuthenticationMethods.Auto);
            }
            else
            {
                _imap.Login(userName, password);
            }
        }

        public void Disconnect()
        {
            _imap.Disconnect();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _imap.Dispose();
            }
        }
    }
}
