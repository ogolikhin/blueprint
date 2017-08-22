namespace AdminStore.Services.Email
{
    public interface IEmailClientFactory
    {
        IEmailClient Make(EmailClientType type);
    }
}
