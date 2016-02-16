using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [RoutePrefix("config")]
    public class ConfigController : ApiController
    {
        internal readonly IConfigRepository _configRepo;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly sl.IServiceLogRepository _log;

        public ConfigController() : this(new SqlConfigRepository(), new HttpClientProvider(), new sl.ServiceLogRepository())
        {

        }

        internal ConfigController(IConfigRepository configRepo, IHttpClientProvider httpClientProvider, sl.IServiceLogRepository log)
        {
            _configRepo = configRepo;
            _httpClientProvider = httpClientProvider;
            _log = log;
        }

        /// <summary>
        /// GetConfigSettings
        /// </summary>
        /// <remarks>
        /// Returns all application settings. The result is a set of key-value pairs, where
        /// the keys are group names and the values are settings. Settings are key-value pairs,
        /// where the key is the setting name and the value is a string value.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("settings"), SessionRequired]
        [ResponseType(typeof(Dictionary<string, Dictionary<string, string>>))]
        public async Task<IHttpActionResult> GetConfigSettings()
        {
            try
            {
                var uri = new Uri(WebApiConfig.ConfigControl);
                var http = _httpClientProvider.Create(uri);
                var request = new HttpRequestMessage { RequestUri = new Uri(uri, "settings/false"), Method = HttpMethod.Get };
                request.Headers.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                var result = await http.SendAsync(request);
                result.EnsureSuccessStatusCode();
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = result.Content;
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }

        /// <summary>
        /// GetConfig
        /// </summary>
        /// <remarks>
        /// Returns a valid JavaScript file that defines a config object on the window object.
        /// The config object contains a settings property, containing all application settings,
        /// and a labels property, containing all application labels.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="400">Bad Request. The session token is missing or malformed.</response>
        /// <response code="401">Unauthorized. The session token is invalid.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("config.js"), SessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetConfig()
        {
            try
            {
                Dictionary<string, Dictionary<string, string>> settings;
                var uri = new Uri(WebApiConfig.ConfigControl);
                var http = _httpClientProvider.Create(uri);
                var request = new HttpRequestMessage { RequestUri = new Uri(uri, "settings/false"), Method = HttpMethod.Get };
                request.Headers.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                var result = await http.SendAsync(request);
                result.EnsureSuccessStatusCode();
                settings = await result.Content.ReadAsAsync<Dictionary<string, Dictionary<string, string>>>();
                var locale = (Request.Headers.AcceptLanguage.FirstOrDefault() ?? new StringWithQualityHeaderValue("en-US")).Value;
                var labels = await _configRepo.GetLabels(locale);
                var config = "window.config = { settings: {" + SerializeSettings(settings) + "}, labels: {" + SerializeLabels(labels) + "} };";
                var log = "console.log('Configuration for locale " + locale + " loaded successfully.');";
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(config + log, Encoding.UTF8, "text/plain");
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }

        private string SerializeSettings(Dictionary<string, Dictionary<string, string>> settings)
        {
            var str = new StringBuilder();
            foreach (var group in settings)
            {
                foreach (var setting in group.Value)
                {
                    str.AppendFormat("'{0}':{{'{1}', '{2}'}},", setting.Key, setting.Value, group.Key);
                }
            }
            return str.Length == 0 ? string.Empty : str.ToString(0, str.Length - 1);
        }

        private string SerializeLabels(IEnumerable<ApplicationLabel> labels)
        {
            var str = new StringBuilder();
            foreach (var l in labels)
            {
                str.AppendFormat("'{0}':'{1}',", l.Key, l.Text);
            }
            return str.Length == 0 ? string.Empty : str.ToString(0, str.Length - 1);
        }
    }
}
