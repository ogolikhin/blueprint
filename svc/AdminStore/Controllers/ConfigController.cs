using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Net;
using System.Linq;
using System.Net.Http.Headers;
using AdminStore.Repositories;
using System;
using AdminStore.Models;
using System.Text;
using System.Collections.Generic;

namespace AdminStore.Controllers
{
    [RoutePrefix("config")]
    public class ConfigController : ApiController
    {
        private readonly IConfigRepository _configRepo;

        public ConfigController() : this(new SqlConfigRepository())
      {
        }

        internal ConfigController(IConfigRepository configRepo)
        {
            _configRepo = configRepo;
        }

        [HttpGet]
        [Route("settings")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetConfigSettings()
        {
            try
            {
                using (var http = new HttpClient())
                {
                    http.BaseAddress = new Uri(WebApiConfig.ConfigControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").FirstOrDefault());
                    var result = await http.GetAsync("settings/false");
                    result.EnsureSuccessStatusCode();
                    var response = Request.CreateResponse(HttpStatusCode.OK, result.Content);
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
                using (var http = new HttpClient())
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

        string SerializeSettings(IEnumerable<ConfigSetting> settings)
        {
            var str = new StringBuilder();
            foreach (var s in settings)
                str.AppendFormat("'{0}':'{'{1}', '{2}'}',", s.Key, s.Value, s.Group);
            return str.ToString();
        }

        string SerializeLabels(IEnumerable<ApplicationLabel> labels)
        {
            var str = new StringBuilder();
            foreach (var l in labels)
                str.AppendFormat("'{0}':'{1}',", l.Key, l.Text);
            return str.ToString();
        }
    }
}
