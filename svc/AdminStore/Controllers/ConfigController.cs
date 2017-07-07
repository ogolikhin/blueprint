using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Models;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Controllers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ConfigControl;
using ServiceLibrary.Repositories;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("config")]
    [BaseExceptionFilter]
    public class ConfigController : LoggableApiController
    {
        internal readonly IApplicationSettingsRepository _applicationSettingsRepository;
        internal readonly ISqlSettingsRepository _settingsRepository;
        internal readonly IUserRepository _userRepository;
        internal readonly IHttpClientProvider _httpClientProvider;

        public override string LogSource => WebApiConfig.LogSourceConfig;

        public ConfigController() : this
            (
                new ApplicationSettingsRepository(), 
                new SqlSettingsRepository(), 
                new SqlUserRepository(), 
                new HttpClientProvider(), 
                new ServiceLogRepository()
            )
        {
        }

        internal ConfigController
        (
            IApplicationSettingsRepository applicationSettingsRepository, 
            ISqlSettingsRepository settingsRepository, 
            IUserRepository userRepository,
            IHttpClientProvider httpClientProvider, 
            IServiceLogRepository log
        ) : base (log)
        {
            _applicationSettingsRepository = applicationSettingsRepository;
            _settingsRepository = settingsRepository;
            _userRepository = userRepository;
            _httpClientProvider = httpClientProvider;
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

        /// <summary>
        /// GetApplicationSettings
        /// </summary>
        /// <remarks>
        /// Returns application settings.
        /// </remarks>
        /// <response code="200">OK.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route(""), NoSessionRequired]
        [ResponseType(typeof(Dictionary<string, string>))]
        public async Task<IHttpActionResult> GetApplicationSettings()
        {
            var settings = (await _applicationSettingsRepository.GetSettingsAsync(true)).ToDictionary(it => it.Key, it => it.Value);
            return Ok(settings);
        }

        /// <summary>
        /// GetUserManagementSettings
        /// </summary>
        /// <remarks>
        /// Returns settings necessary for user management.
        /// </remarks>
        [HttpGet, NoCache]
        [Route("users"), SessionRequired]
        [ResponseType(typeof(UserManagementSettings))]
        public async Task<IHttpActionResult> GetUserManagementSettings()
        {
            var user = await _userRepository.GetLoginUserByIdAsync(Session.UserId);
            if (user == null)
            {
                throw new AuthenticationException($"User does not exist with UserId: {Session.UserId}");
            }

            if (!user.InstanceAdminRoleId.HasValue)
            {
                throw new AuthorizationException("You do not have permission to access this area.", ErrorCodes.UnauthorizedAccess);
            }

            var settings = await _settingsRepository.GetUserManagementSettingsAsync();
            return Ok(settings);
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
            var settings = (await _applicationSettingsRepository.GetSettingsAsync(true)).ToDictionary(it => it.Key, it => it.Value);

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
    }
}
