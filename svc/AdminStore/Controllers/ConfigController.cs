using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using sl = ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("config")]
    public class ConfigController : ApiController
    {
        internal readonly IApplicationSettingsRepository _appSettingsRepo;
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly sl.IServiceLogRepository _log;

        public ConfigController() : this(new ApplicationSettingsRepository(),  new HttpClientProvider(), new sl.ServiceLogRepository())
        {
        }

        internal ConfigController(IApplicationSettingsRepository settingsRepo, IHttpClientProvider httpClientProvider, sl.IServiceLogRepository log)
        {
            _appSettingsRepo = settingsRepo;
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
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
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
        /// The config object contains a settings property, containing all application settings.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("config.js"), NoSessionRequired]
        [ResponseType(typeof(string))]
        public async Task<IHttpActionResult> GetConfig()
        {
            try
            {
                var settings = (await _appSettingsRepo.GetSettings()).ToDictionary(it => it.Key, it => it.Value);

                var script = "(function (window) {\n" +
                    "    if (!window.config) {\n" +
                    "        window.config = {};\n" +
                    "    }\n" +
                    $"    window.config.settings = {settings.ToJSON()}\n" +
                    "}(window));";

                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(script, Encoding.UTF8, "application/javascript");
                return ResponseMessage(response);
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceConfig, ex);
                return InternalServerError();
            }
        }
    }
}
