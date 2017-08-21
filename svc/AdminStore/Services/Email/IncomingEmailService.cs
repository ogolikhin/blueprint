using System;
using ServiceLibrary.Exceptions;

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
            MailBee.Global.AutodetectPortAndSslMode = false;
            MailBee.Global.LicenseKey = "MN800-02CA3564CA2ACAAECAB17D4ADEC9-145F";

            try
            {
                using (var emailClient = _clientFactory.Make(config.ClientType))
                {
                    emailClient.UseSsl = config.EnableSsl;

                    emailClient.Connect(config.ServerAddress, config.Port);

                    emailClient.Login(config.AccountUsername, config.AccountPassword);

                    emailClient.Disconnect();
                }
            }
            catch (EmailException ex)
            {
                throw new BadRequestException(ex.Message, ex.ErrorCode);
            }

        }
    }
}
