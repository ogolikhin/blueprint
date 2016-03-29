using Model.OpenApiModel.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model.OpenApiModel
{

    public interface IArtifact : IArtifactBase
    {
        // TODO Find the way or wait for the API implementation which retrieves ArtifactType
        //ArtifactType ArtifactType { get; set; }
        // TODO Find the way or wait for the API implementation which retrieve descrption
        //string Description { get; set; }

        IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
        IArtifactResult<IArtifact> DeleteArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }

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
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Save(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Publish(IUser user = null, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard the added artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint. If null, attempts to delete using the credentials</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Discard(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete the artifact on Blueprint server.
        /// To delete artifact permanently Publish must be called after Delete, otherwise deletion can be discarded.
        /// </summary>
        /// <param name="user">(optional) The user deleting the artifact. If null, attempts to delete using the credentials
        /// of the user that created the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        List<IDeleteArtifactResult> Delete(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool deleteChildren = false);

        /// <summary>
        /// Gets the Version property of an Artifact via OpenAPI call
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>The historical version of the artifact.</returns>
        int GetVersion(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
