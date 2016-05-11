using System.Collections.Generic;
using System.Net;
using Model.ArtifactModel.Impl;

namespace Model.ArtifactModel
{
    public interface IOpenApiArtifact : IArtifactBase
    {
        /// <summary>
        /// List of Open Api Artifact Properties
        /// </summary>
        List<OpenApiProperty> Properties { get; }

        /// <summary>
        /// List of Open Api Artifact Comments
        /// </summary>
        List<OpenApiComment> Comments { get; }

        /// <summary>
        /// List of Open Api Artifact Traces
        /// </summary>
        List<OpenApiTrace> Traces { get; }

        /// <summary>
        ///  List of Open Api Artifact Attachments
        /// </summary>
        List<OpenApiAttachment> Attachments { get; }

        /// <summary>
        /// Save the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void Save(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">(optional) The user to authenticate to Blueprint. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        void Publish(IUser user = null, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Discard changes to an artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to delete using the credentials</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The DiscardedArtifactResult list after discard artifact call</returns>
        List<DiscardArtifactResult> Discard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);

        /// <summary>
        /// Delete the artifact on Blueprint server.
        /// To delete artifact permanently, Publish must be called after the Delete, otherwise the deletion can be discarded.
        /// </summary>
        /// <param name="user">(optional) The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        List<DeleteArtifactResult> Delete(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false, bool deleteChildren = false);

        /// <summary>
        /// Gets the Version property of an Artifact via OpenAPI call
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The historical version of the artifact.</returns>
        int GetVersion(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false);
    }
}