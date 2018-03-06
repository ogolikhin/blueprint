using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Security;
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

            var result = await SendWebhook(tenant, message);

            if (result.IsSuccessStatusCode)
            {
                Logger.Log($"Finished processing Webhook with result: {(int)result.StatusCode} - {result.StatusCode}", message, tenant);
                return true;
            }

            if (result.StatusCode == HttpStatusCode.Gone)
            {
                var errorMessage = $"Failed to send Webhook. Response was {(int)result.StatusCode} - {result.StatusCode}. Will not try again.";
                Logger.Log(errorMessage, message, tenant, LogLevel.Error);
                throw new WebhookExceptionDoNotRetry(errorMessage);
            }
            else
            {
                var errorMessage = $"Failed to send Webhook. Response was {(int)result.StatusCode} - {result.StatusCode}. Will try again in {ConfigHelper.WebhookRetryInterval} seconds.";
                Logger.Log(errorMessage, message, tenant, LogLevel.Error);
                throw new WebhookExceptionRetryPerPolicy(errorMessage);
            }
        }

        public async Task<HttpResponseMessage> SendWebhook(TenantInformation tenant, WebhookMessage message)
        {
            try
            {
                var webhookUri = new Uri(message.Url);
                var httpClientProvider = new HttpClientProvider();
                var httpClient = httpClientProvider.CreateWithCustomCertificateValidation(webhookUri, message.IgnoreInvalidSSLCertificate, ConfigHelper.WebhookConnectionTimeout);

                var request = new HttpRequestMessage
                {
                    RequestUri = webhookUri,
                    Method = HttpMethod.Post,
                    Content = new StringContent(message.WebhookJsonPayload, Encoding.UTF8, "application/json")
                };

                AddHttpHeaders(request, message, tenant);

                AddBasicAuthentication(request, message, tenant);

                AddSignatureAuthentication(request, message, tenant);

                AddNServiceBusHeaders(request, message, tenant);

                return await httpClient.SendAsync(request);
            }
            catch (HttpRequestException e)
            {
                if (e.InnerException is WebException)
                {
                    var webException = (WebException)e.InnerException;
                    if (webException.Status == WebExceptionStatus.TrustFailure)
                    {
                        throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook due to invalid SSL Certificate. {webException.Message}.");
                    }
                    else
                    {
                        throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook due {webException.Status}. {webException.Message} {webException.Response}");
                    }
                }
                else
                {
                    throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook due to {e.InnerException}.");
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object,System.Object)")]
        private void AddHttpHeaders(HttpRequestMessage request, WebhookMessage message, TenantInformation tenant)
        {
            if (message.HttpHeaders.IsEmpty())
            {
                return;
            }

            foreach (var httpHeader in message.HttpHeaders)
            {
                var headers = SystemEncryptions.Decrypt(httpHeader);
                var keyValuePair = headers.Split(':');

                // Do not allow overriding of Restricted Http Headers
                if (WebHeaderCollection.IsRestricted(keyValuePair[0]))
                {
                    Logger.Log($"'{keyValuePair[0]}' is a restricted Http Header. '{keyValuePair[0]}:{keyValuePair[1]}' was not added to the headers of the webhook request.",
                        message, tenant, LogLevel.Error);
                    continue;
                }
                else
                {
                    request.Headers.Add(keyValuePair[0], keyValuePair[1]);
                }
            }
        }

        private void AddBasicAuthentication(HttpRequestMessage request, WebhookMessage message, TenantInformation tenant)
        {
            if (message.BasicAuthUsername.IsEmpty() || message.BasicAuthPassword.IsEmpty())
            {
                return;
            }

            try
            {
                string basicAuthentication = SystemEncryptions.Decrypt(message.BasicAuthUsername) + ':' + SystemEncryptions.Decrypt(message.BasicAuthPassword);
                string authenticationInfo = SystemEncryptions.EncodeTo64UTF8(basicAuthentication);
                request.Headers.Authorization = new AuthenticationHeaderValue("Basic", authenticationInfo);
            }
            catch
            {
                Logger.Log("Failed to add Basic Authentication Header to Webhook.", message, tenant, LogLevel.Error);
            }
        }

        private void AddSignatureAuthentication(HttpRequestMessage request, WebhookMessage message, TenantInformation tenant)
        {
            if (message.SignatureSecretToken.IsEmpty() || message.SignatureAlgorithm.IsEmpty())
            {
                return;
            }

            try
            {
                SignatureAlgorithm webhookAlgorithm;
                var algorithm = Enum.TryParse(message.SignatureAlgorithm, out webhookAlgorithm) ? webhookAlgorithm : SignatureAlgorithm.HMACSHA256;
                var secretToken = SystemEncryptions.Decrypt(message.SignatureSecretToken);
                var messageHashCheckSum = CreateEncodedSignature(message.WebhookJsonPayload, secretToken, algorithm);
                request.Headers.Add("X-BLUEPRINT-SIGNATURE", messageHashCheckSum);
            }
            catch
            {
                Logger.Log("Failed to add Signature Authentication to Webhook.", message, tenant, LogLevel.Error);
            }
        }

        private string CreateEncodedSignature(string message, string secretToken, SignatureAlgorithm algorithm = SignatureAlgorithm.HMACSHA256)
        {
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secretToken);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmac = CreateHmacSignatureInstance(algorithm, keyByte))
            {
                byte[] hashmessage = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashmessage).Replace("-", "").ToUpperInvariant();
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

        private void AddNServiceBusHeaders(HttpRequestMessage request, WebhookMessage message, TenantInformation tenant)
        {
            try
            {
                // Include the NServiceBus Message Id in the request header
                request.Headers.Add("X-BLUEPRINT-MESSAGE-ID", message.NSBMessageId);

                // Track the number of times the request has been retried by the NServiceBus
                request.Headers.Add("X-BLUEPRINT-RETRY-NUMBER", message.NSBRetryCount);
            }
            catch
            {
                Logger.Log("Failed to add NServiceBus Headers to Webhook.", message, tenant, LogLevel.Error);
            }
        }
    }
}
