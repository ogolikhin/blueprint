﻿using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
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
                Logger.Log($"Finished processing webhook with result: {result.StatusCode}", message, tenant);
                return true;
            }

            if (result.StatusCode == HttpStatusCode.Gone)
            {
                Logger.Log($"Failed to send webhook. Will not try again.", message, tenant, LogLevel.Error);
                throw new WebhookExceptionDoNotRetry($"Failed to send webhook.");
            }
            else
            {
                Logger.Log($"Failed to send webhook. Will try again in {ConfigHelper.WebhookRetryInterval} seconds.", message, tenant, LogLevel.Error);
                throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook");
            }
        }

        public async Task<HttpResponseMessage> SendWebhook(TenantInformation tenant, WebhookMessage message)
        {
            try
            {
                var webhookUri = GetBaseAddress(message.Url);
                var httpClientProvider = new HttpClientProvider();
                var httpClient = httpClientProvider.CreateWithCustomCertificateValidation(webhookUri, message.IgnoreInvalidSSLCertificate, ConfigHelper.WebhookConnectionTimeout);

                // Check if the httpClient configuration of ignoring SSL Certificate errors is inline with the webhook configuration of ignoring SSL errors
                // We must perform this check, as the creation of HttpClients is cached and we may need to update the cache if the webhook configuration has changed since
                if (httpClientProvider.HttpClientIgnoresCertificateErrors(webhookUri) != message.IgnoreInvalidSSLCertificate)
                {
                    httpClientProvider.UpdateHttpClient(webhookUri, message.IgnoreInvalidSSLCertificate);
                }

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
            catch(HttpRequestException e)
            {
                if (e.InnerException is WebException &&
                    ((WebException)e.InnerException).Status == WebExceptionStatus.TrustFailure)
                {
                    throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook due to invalid SSL Certificate. {e}.");
                }
                else
                {
                    throw new WebhookExceptionRetryPerPolicy($"Failed to send webhook due to {e.Message}.");
                }
            }
        }

        private void VerifySSLCertificate(WebhookMessage message)
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) =>
            {
                if (message.IgnoreInvalidSSLCertificate)
                {
                    return true;
                }
                return errors == SslPolicyErrors.None;
            };
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object,System.Object)")]
        private void AddHttpHeaders(HttpRequestMessage request, WebhookMessage message, TenantInformation tenant)
        {
            if (message.HttpHeaders.IsEmpty())
            {
                return;
            }

            foreach(var httpHeader in message.HttpHeaders)
            {
                var headers = SystemEncryptions.Decrypt(httpHeader);
                var keyValuePair = headers.Split(':');
                try
                {
                    request.Headers.Add(keyValuePair[0], keyValuePair[1]);
                }
                catch (ArgumentException)
                {
                    Logger.Log($"Failed to add the following Http Header to Webhook: {keyValuePair[0]}:{keyValuePair[1]}.", message, tenant, LogLevel.Error);
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
                var messageHashCheckSum = CreateEncodedSignature(message.WebhookJsonPayload, message.SignatureSecretToken, algorithm);
                request.Headers.Add("X-BLUEPRINT-SIGNATURE", messageHashCheckSum);
            }
            catch
            {
                Logger.Log("Failed to add Signature Authentication to Webhook.", message, tenant, LogLevel.Error);
            }
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
