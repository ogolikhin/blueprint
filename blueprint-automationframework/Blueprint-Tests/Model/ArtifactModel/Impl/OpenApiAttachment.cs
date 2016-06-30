using System.Net;
using System.Collections.Generic;
using Utilities;
using Utilities.Facades;

namespace Model.ArtifactModel.Impl
{
    public class OpenApiAttachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string Link { get; set; }

        public void Delete(IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.OpenApiToken;
            System.Uri uri;
            System.Uri.TryCreate(Link, System.UriKind.Absolute, out uri);
            var restApi = new RestApiFacade("http://" + uri.Host, tokenValue);
            restApi.SendRequestAndGetResponse(uri.AbsolutePath, RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);
        }
    }
}
