using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Web.Http;
using System.Web.Http.Description;
using AdminStore.Repositories;
using ServiceLibrary.Attributes;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;

namespace AdminStore.Controllers
{
    [ApiControllerJsonConfig]
    [RoutePrefix("licenses")]
    public class LicensesController : ApiController
    {
        internal readonly IHttpClientProvider _httpClientProvider;
        internal readonly ISqlUserRepository _userRepository;
        internal readonly IServiceLogRepository _log;

        public LicensesController(): this(new HttpClientProvider(), new SqlUserRepository(), new ServiceLogRepository())
        {
        }

        internal LicensesController(IHttpClientProvider httpClientProvider, ISqlUserRepository userRepository, IServiceLogRepository log)
        {
            _httpClientProvider = httpClientProvider;
            _userRepository = userRepository;
            _log = log;
        }

        /// <summary>
        /// GetLicenseTransactions
        /// </summary>
        /// <remarks>
        /// Returns license transactions for the past <paramref name="days" /> days.
        /// </remarks>
        /// <param name="days">The number of past days for which to return transactions.</param>
        /// <response code="200">OK.</response>
        /// <response code="401">Unauthorized. The session token is invalid, missing or malformed.</response>
        /// <response code="500">Internal Server Error. An error occurred.</response>
        [HttpGet, NoCache]
        [Route("transactions"), SessionRequired]
        [ResponseType(typeof(IEnumerable<LicenseTransaction>))]
        public async Task<IHttpActionResult> GetLicenseTransactions(int days)
        {
            try
            {
                var uri = new Uri(WebApiConfig.AccessControl);
                var http = _httpClientProvider.Create(uri);
                if (!Request.Headers.Contains("Session-Token"))
                {
                    throw new ArgumentNullException();
                }
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(uri, "licenses/transactions?days=" + days + "&consumerType=1"), // LicenseConsumerType.Client
                    Method = HttpMethod.Get
                };
                request.Headers.Add("Session-Token", Request.Headers.GetValues("Session-Token").First());
                var result = await http.SendAsync(request);
                var transactions = (await result.Content.ReadAsAsync<IEnumerable<LicenseTransaction>>()).ToArray();
                var users = (await _userRepository.GetLicenseTransactionUserInfoAsync(transactions.Select(t => t.UserId).Distinct())).ToDictionary(u => u.Id);
                foreach (var transaction in transactions.Where(t => users.ContainsKey(t.UserId)))
                {
                    var user = users[transaction.UserId];
                    transaction.Username = user.Login;
                    transaction.Department = user.Department;
                }
                var response = Request.CreateResponse(HttpStatusCode.OK, transactions);
                return ResponseMessage(response);
            }
            catch (ArgumentNullException)
            {
                return Unauthorized();
            }
            catch (Exception ex)
            {
                await _log.LogError(WebApiConfig.LogSourceLicenses, ex);
                return InternalServerError();
            }
        }
    }
}
