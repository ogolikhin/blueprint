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

        /// <see cref="IConfigControl.GetLog(List{HttpStatusCode})"/>
        public IFile GetLog(List<HttpStatusCode> expectedStatusCodes = null)
        {
            var restApi = new RestApiFacade(_address, token: string.Empty);
            var path = I18NHelper.FormatInvariant("{0}/log/getlog", SVC_PATH);

            var response = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            File file = null;

            if (response.StatusCode == HttpStatusCode.OK)
            {
                file = new File
                {
                    Content = response.RawBytes.ToArray(),
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
