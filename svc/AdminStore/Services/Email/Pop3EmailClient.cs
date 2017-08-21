using System;
using MailBee;
using MailBee.Pop3Mail;
using MailBee.Security;

namespace AdminStore.Services.Email
{
    public class Pop3EmailClient : IEmailClient
    {
        private readonly Pop3 _pop3;

        public Pop3EmailClient()
        {
            _pop3 = new Pop3();
        }

        private bool _useSsl;

        public bool UseSsl
        {
            get { return _useSsl; }
            set
            {
                _useSsl = value;
                _pop3.SslMode = value ? SslStartupMode.OnConnect : SslStartupMode.Manual;
            }
        }

        public void Connect(string serverAddress, int port)
        {
            _pop3.Connect(serverAddress, port);
        }

        public void Login(string userName, string password)
        {
            if (UseSsl)
            {
                _pop3.Login(userName, password, AuthenticationMethods.Auto);
            }
            else
            {
                _pop3.Login(userName, password);
            }
        }

        public void Disconnect()
        {
            _pop3.Disconnect();
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
                _pop3.Dispose();
            }
        }
    }
}
