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
using ServiceLibrary.Helpers;

namespace AdminStore.Controllers
{
    [RoutePrefix("config")]
    public class ConfigController : ApiController
    {
        internal readonly IConfigRepository _configRepo;
        internal readonly IHttpClientProvider _httpClientProvider;

        public ConfigController() : this(new SqlConfigRepository(), new HttpClientProvider())
        {

        }

        internal ConfigController(IConfigRepository configRepo, IHttpClientProvider httpClientProvider)
        {
            _configRepo = configRepo;
            _httpClientProvider = httpClientProvider;
        }

        [HttpGet]
        [Route("settings")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetConfigSettings()
        {
            try
            {
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(WebApiConfig.ConfigControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                    var result = await http.GetAsync("settings/false");
                    result.EnsureSuccessStatusCode();
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = result.Content;
                    response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                    response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                    return ResponseMessage(response);
                }
            }
            catch
            {
                return InternalServerError();
            }
        }

        [HttpGet]
        [Route("config.js")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetConfig()
        {
            try
            {
                IEnumerable<ConfigSetting> settings;
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(WebApiConfig.ConfigControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                    var result = await http.GetAsync("settings/false");
                    result.EnsureSuccessStatusCode();
                    settings = await result.Content.ReadAsAsync<ConfigSetting[]>();
                }
                var locale = (Request.Headers.AcceptLanguage.FirstOrDefault() ?? new StringWithQualityHeaderValue("en-US")).Value;
                var labels = await _configRepo.GetLabels(locale);
                var config = "window.config = { settings: {" + SerializeSettings(settings) + "}, labels: {" + SerializeLabels(labels) + "} };";
                var log = "console.log('Configuration for locale " + locale + " loaded successfully.');";
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(config + log, Encoding.UTF8, "text/plain");
                response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate"); // HTTP 1.1.
                response.Headers.Add("Pragma", "no-cache"); // HTTP 1.0.
                return ResponseMessage(response);
            }
            catch
            {
                return InternalServerError();
            }
        }

        private string SerializeSettings(IEnumerable<ConfigSetting> settings)
        {
            var str = new StringBuilder();
            foreach (var s in settings)
            {
                str.AppendFormat("'{0}':{{'{1}', '{2}'}},", s.Key, s.Value, s.Group);
            }
            return str.ToString(0, str.Length - 1);
        }

        private string SerializeLabels(IEnumerable<ApplicationLabel> labels)
        {
            var str = new StringBuilder();
            foreach (var l in labels)
            {
                str.AppendFormat("'{0}':'{1}',", l.Key, l.Text);
            }
            return str.ToString(0, str.Length - 1);
        }
    }
}
