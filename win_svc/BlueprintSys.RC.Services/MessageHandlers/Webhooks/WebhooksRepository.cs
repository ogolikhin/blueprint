namespace BlueprintSys.RC.Services.MessageHandlers.Webhooks
{
    public interface IWebhookRepository : IBaseRepository
    {

    }

    public class WebhooksRepository : BaseRepository, IWebhookRepository
    {
        public WebhooksRepository(string connectionString) : base(connectionString)
        {

        }

    }
}
