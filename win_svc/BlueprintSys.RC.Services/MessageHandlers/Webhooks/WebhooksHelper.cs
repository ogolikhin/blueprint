using System;
using System.Threading.Tasks;
using BlueprintSys.RC.Services.Helpers;
using BluePrintSys.Messaging.Models.Actions;
using ServiceLibrary.Repositories.Webhooks;
using ServiceLibrary.Helpers;
using System.Net.Http;

namespace BlueprintSys.RC.Services.MessageHandlers.Webhooks
{
    public class WebhooksHelper : MessageActionHandler
    {
        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (WebhookMessage)actionMessage;
            var result = await SendWebhook(tenant, message, (IWebhookRepository)baseRepository);
            Logger.Log($"Finished processing message with result: {result}", message, tenant);
            return await Task.FromResult(result == true);
        }

        private async Task<bool> SendWebhook(TenantInformation tenant, WebhookMessage message, IWebhookRepository repository)
        {
            var httpClientProvider = new HttpClientProvider();
            var http = httpClientProvider.Create(GetBaseAddress(message.Url));
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(message.Url),
                Method = HttpMethod.Post
            };

            var result = await http.SendAsync(request);

            return (result.StatusCode == System.Net.HttpStatusCode.OK);
        }

        private Uri GetBaseAddress(string urlString)
        {
            var url = new Uri(urlString);
            var builder = new UriBuilder();
            builder.Scheme = url.Scheme;
            builder.Host = url.Host;
            builder.Port = url.Port;
            return builder.Uri;
        }
    }
}
