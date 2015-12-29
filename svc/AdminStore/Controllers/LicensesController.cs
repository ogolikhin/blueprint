using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace AdminStore.Controllers
{
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly ISqlUserRepository _userRepository;

        public LicensesController(): this(new HttpClientProvider(), new SqlUserRepository())
        {
        }

        internal LicensesController(IHttpClientProvider httpClientProvider, ISqlUserRepository userRepository)
        {
            _httpClientProvider = httpClientProvider;
            _userRepository = userRepository;
        }

        [HttpGet]
        [Route("transactions")]
        [ResponseType(typeof(HttpResponseMessage))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days)
        {
            try
            {
                using (var http = _httpClientProvider.Create())
                {
                    http.BaseAddress = new Uri(WebApiConfig.AccessControl);
                    http.DefaultRequestHeaders.Accept.Clear();
                    http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    if (!Request.Headers.Contains("Session-Token"))
                    {
                        throw new ArgumentNullException();
                    }
                    http.DefaultRequestHeaders.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                    var result = await http.GetAsync("licenses/transactions?days=" + days + "&consumerType=1"); // LicenseConsumerType.Client
                    var transactions = (await result.Content.ReadAsAsync<IEnumerable<LicenseTransaction>>()).ToArray();
                    var users = (await _userRepository.GetLicenseTransactionUserInfoAsync(transactions.Select(t => t.UserId).Distinct())).ToDictionary(u => u.Id);
                    foreach (var transaction in transactions)
                    {
                        var user = users[transaction.UserId];
                        transaction.Username = user.Login;
                        transaction.Department = user.Department;
                    }
                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new ObjectContent<IEnumerable<LicenseTransaction>>(transactions, new JsonMediaTypeFormatter());
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
    }
}
