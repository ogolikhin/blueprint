using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using Common;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class ConfigControl : IConfigControl
    {
        private const string SVC_PATH = "/svc/configcontrol";
        private const string SESSION_TOKEN_COOKIE_NAME = "BLUEPRINT_SESSION_TOKEN";

        private readonly string _address;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URL of the ConfigControl service.</param>
        public ConfigControl(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            _address = address;
        }

        #region Inherited from IConfigControl

        /// <see cref="IConfigControl.GetLog(IUser, List{HttpStatusCode}, bool)"/>
        public IFile GetLog(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            string tokenValue = user?.Token?.AccessControlToken ?? string.Empty;    // Set to token or empty string, not null.

            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SESSION_TOKEN_COOKIE_NAME, tokenValue);
                tokenValue = string.Empty;
            }

            var restApi = new RestApiFacade(_address, user?.Username, user?.Password, tokenValue);
            var path = I18NHelper.FormatInvariant("{0}/log/getlog", SVC_PATH);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            File file = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes.ToArray(),
//                    LastModifiedDate =
//                        DateTime.ParseExact(response.Headers.First(h => h.Key == "Stored-Date").Value.ToString(), "o",
//                            null),
                    FileType = response.ContentType,
                    FileName =
                        new ContentDisposition(
                            response.Headers.First(h => h.Key == "Content-Disposition").Value.ToString()).FileName
                };
            }

            return file;
        }

        #endregion Inherited from IConfigControl
    }
}
