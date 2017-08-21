using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MailBee;
using MailBee.Pop3Mail;
using MailBee.Security;

namespace AdminStore.Services.Email
{
    public class IncomingEmailService : IIncomingEmailService
    {
        private readonly IEmailClientFactory _clientFactory;

        public IncomingEmailService() : this(new EmailClientFactory())
        {
        }

        public IncomingEmailService(IEmailClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public void TryConnect(EmailClientConfig config)
        {
            using (var emailClient = _clientFactory.Make(config.ClientType))
            {
                emailClient.UseSsl = config.EnableSsl;

                emailClient.Connect(config.ServerAddress, config.Port);

                emailClient.Login(config.AccountUsername, config.AccountPassword);

                emailClient.Disconnect();
            }
        }
    }
}
