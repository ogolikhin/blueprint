namespace AdminStore.Services.Email
{
    public interface IIncomingEmailService
    {
        void TryConnect(EmailClientConfig config);
    }
}
