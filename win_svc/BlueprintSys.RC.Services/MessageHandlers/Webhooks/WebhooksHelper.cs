using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
using ServiceLibrary.Exceptions;
using BluePrintSys.Messaging.CrossCutting.Configuration;
using BluePrintSys.Messaging.Models.Actions;
using BlueprintSys.RC.Services.Helpers;

namespace BlueprintSys.RC.Services.MessageHandlers.Webhooks
{
    public class WebhooksHelper : MessageActionHandler
    {
        public enum SignatureAlgorithm
        {
            None = 0,
            HMACSHA1 = 1,
            HMACSHA256 = 2
        }

        protected IConfigHelper ConfigHelper = new ConfigHelper();

        protected override async Task<bool> HandleActionInternal(TenantInformation tenant, ActionMessage actionMessage, IBaseRepository baseRepository)
        {
            var message = (WebhookMessage)actionMessage;
            var result = await SendWebhook(tenant, message, (IWebhookRepository)baseRepository);
            Logger.Log($"Finished processing webhook with result: {result}", message, tenant);
            return await Task.FromResult(result == true);
        }

        private async Task<bool> SendWebhook(TenantInformation tenant, WebhookMessage message, IWebhookRepository repository)
        {
            var httpClientProvider = new HttpClientProvider();
            var http = httpClientProvider.Create(GetBaseAddress(message.Url));
            // Set Webhook Connection Timeout as specified within app.config
            http.Timeout = new TimeSpan(hours: 0, minutes: 0, seconds: ConfigHelper.WebhookConnectionTimeout);
            var request = new HttpRequestMessage
            {
                RequestUri = new Uri(message.Url),
                Method = HttpMethod.Post,
                Content = new StringContent(message.WebhookJsonPayload, Encoding.UTF8, "application/json")
            };

            // Track the number of times the request has been retried
            // request.Headers.Add("X-BLUEPRINT-RETRY-NUMBER", message.NSBRetryCount);

            VerifySSLCertificate(request, message);

            AddHttpHeaders(request, message);

            AddBasicAuthentication(request, message);

            AddAuthenticationSignature(request, message);

            var result = await http.SendAsync(request);

            if (result.IsSuccessStatusCode)
            {
                return true;
            }

            if (result.StatusCode == HttpStatusCode.Gone)
            {
                Logger.Log($"Failed to send webhook. Will not try to send again.", message, tenant);
                throw new WebhookExceptionDoNotRetry($"Failed to send webhook.");
            }
            else
            {
                Logger.Log($"Failed to send webhook. Will try again in {ConfigHelper.WebhookRetryInterval} seconds.", message, tenant);
                throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook");
            }
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

        private void AddHttpHeaders(HttpRequestMessage request, WebhookMessage message)
        {
            if (message.HttpHeaders.IsEmpty())
            {
                return;
            }

            foreach(var httpHeader in message.HttpHeaders)
            {
                var headers = SystemEncryptions.Decrypt(httpHeader);
                var keyValuePair = headers.Split(':');
                request.Headers.Add(keyValuePair[0], keyValuePair[1]);
            }
        }

        private void AddBasicAuthentication(HttpRequestMessage request, WebhookMessage message)
        {
            if (message.BasicAuthUsername.IsEmpty() || message.BasicAuthPassword.IsEmpty())
            {
                return;
            }

            string basicAuthentication = SystemEncryptions.Decrypt(message.BasicAuthUsername) + ':' + SystemEncryptions.Decrypt(message.BasicAuthPassword);
            string authenticationInfo = SystemEncryptions.EncodeTo64UTF8(basicAuthentication);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationInfo);
        }

        private void AddAuthenticationSignature(HttpRequestMessage request, WebhookMessage message)
        {
            if (message.SignatureSecretToken.IsEmpty() || message.SignatureAlgorithm.IsEmpty())
            {
                return;
            }

            SignatureAlgorithm webhookAlgorithm;
            var algorithm = Enum.TryParse(message.SignatureAlgorithm, out webhookAlgorithm) ? webhookAlgorithm : SignatureAlgorithm.HMACSHA256;
            var messageHashCheckSum = CreateEncodedSignature(message.WebhookJsonPayload, message.SignatureSecretToken, algorithm);
            request.Headers.Add("X-BLUEPRINT-SIGNATURE", messageHashCheckSum);
        }

        private string CreateEncodedSignature(string message, string secretToken, SignatureAlgorithm algorithm = SignatureAlgorithm.HMACSHA256)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretToken);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmac = CreateHmacSignatureInstance(algorithm, keyByte))
            {
                byte[] hashmessage = hmac.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }

        private HMAC CreateHmacSignatureInstance(SignatureAlgorithm algorithm, byte[] keyByte)
        {
            if (algorithm == SignatureAlgorithm.HMACSHA1)
            {
                return new HMACSHA1(keyByte);
            }

            return new HMACSHA256(keyByte);
        }

        private void VerifySSLCertificate(HttpRequestMessage request, WebhookMessage message)
        {
            if (message.IgnoreInvalidSSLCertificate)
            {
                // To Do
            }

            // To Do
        }
    }
}
