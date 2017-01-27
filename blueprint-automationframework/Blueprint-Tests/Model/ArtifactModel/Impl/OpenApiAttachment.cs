using System.Collections.Generic;
using System.Net;
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
            System.Uri uri = new System.Uri(Link, System.UriKind.Absolute);
            var restApi = new RestApiFacade(uri, tokenValue);
            //in this case we have resourcePath as a part of URI
            restApi.SendRequestAndGetResponse(string.Empty, RestRequestMethod.DELETE,
                expectedStatusCodes: expectedStatusCodes);
        }

        public bool Equals(AttachedFile attachedFile)
        {
            if (attachedFile == null)
            { return false; }
            else
            {
                return (((attachedFile.AttachmentId) == Id) && 
                    (attachedFile.FileName == FileName));
            }
        }
    }
}
