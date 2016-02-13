using System;
using System.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;

namespace ServiceLibrary.Helpers
{
    public class ConfigControlHttpClientLocator : ServiceLocator<IHttpClientProvider>
    {
        public static void InitDefaultInstance()
        {
            // This is only for UnitTest where we can call WebApiConfig.register multiple times
            if (Current == null)
            {
                var sharedHttp = new SharedHttpClient(new HttpClientProvider(), InitFromConfig);

                Init(sharedHttp);
            }
        }

        public static HttpClient InitFromConfig(HttpClient http)
        {
            if (http == null)
                throw new ArgumentNullException(nameof(http));

            var uri = ConfigurationManager.AppSettings["ConfigControl"];

            if (string.IsNullOrWhiteSpace(uri))
                throw new ApplicationException("Application setting not set: ConfigControl");

            http.BaseAddress = new Uri(uri);

            http.DefaultRequestHeaders.Accept.Clear();
            http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return http;
        }
    }
}
