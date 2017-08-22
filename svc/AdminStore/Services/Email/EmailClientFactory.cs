using System;

namespace AdminStore.Services.Email
{
    public class EmailClientFactory : IEmailClientFactory
    {
        public IEmailClient Make(EmailClientType type)
        {
            switch (type)
            {
                case EmailClientType.Imap:
                    return new ImapEmailClient();
                case EmailClientType.Pop3:
                    return new Pop3EmailClient();
                default:
                    throw new ArgumentException("Email Client not supported");
            }
        }
    }
}
