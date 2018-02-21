namespace BlueprintSys.RC.Services.MessageHandlers.Webhooks
{
    public interface IWebhooksRepository : IBaseRepository
    {

    }

    public class WebhooksRepository : BaseRepository, IWebhooksRepository
    {
        public WebhooksRepository(string connectionString) : base(connectionString)
        {
        }
    }
}
