using System.Collections.Generic;
using System.Net;
using NUnit.Framework;
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

        /// <summary>
        /// Asserts that the OpenApiAttachment properties match the AttachedFile properties.
        /// </summary>
        /// <param name="expectedOpenApiAttachment">The OpenApiAttachment whose properties are expected.</param>
        /// <param name="attachedFile">The AttachedFile from Nova.</param>
        public static void AssertAreEqual(OpenApiAttachment expectedOpenApiAttachment, AttachedFile attachedFile)
        {
            Assert.AreEqual(expectedOpenApiAttachment?.Id, attachedFile?.AttachmentId, "The Id and AttachmentId properties don't match!");
            Assert.AreEqual(expectedOpenApiAttachment?.FileName, attachedFile?.FileName, "The FileName properties don't match!");
        }
    }
}
