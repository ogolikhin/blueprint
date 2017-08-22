using System;
using MailBee;
using MailBee.ImapMail;
using MailBee.Security;

namespace AdminStore.Services.Email
{
    public class ImapEmailClient : IEmailClient
    {
        private readonly Imap _imap;

        private bool _useSsl;

        public bool UseSsl
        {
            get { return _useSsl; }
            set
            {
                _useSsl = value;
                _imap.SslMode = value ? SslStartupMode.OnConnect : SslStartupMode.Manual;
            }
        }

        public ImapEmailClient()
        {
            _imap = new Imap();
        }

        public void Connect(string serverAddress, int port)
        {
            try
            {
                _imap.Connect(serverAddress, port);
            }
            catch (MailBeeException ex)
            {
                EmailClientExceptionHandler.HandleConnectException(ex);
            }
        }

        public void Login(string userName, string password)
        {
            try {
                if (UseSsl)
                {
                    _imap.Login(userName, password, AuthenticationMethods.Auto);
                }
                else
                {
                    _imap.Login(userName, password);
                }
            }
            catch (MailBeeException ex)
            {
                EmailClientExceptionHandler.HandleLoginException(ex);
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
