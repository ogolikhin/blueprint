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
        /// The User who Created the Artifact
        /// </summary>
        IUser CreatedBy { get; set; }

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
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Save(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Adds the artifact to Blueprint
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional)A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact added to blueprint</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        IOpenApiArtifact AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Discard the added artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactList">The artifact(s) to be discarded.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional)A list of expected status codes.  By default, only '200' is expected.</param>
        /// <returns>The artifact added to blueprint</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IPublishArtifactResult> DiscardArtifacts(List<IOpenApiArtifact> artifactList, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Publish(IUser user, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Publish added artifact(s) to Blueprint
        /// </summary>
        /// <param name="artifactList">The artifact(s) to be published.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  By default, only '200' is expected.</param>
        /// <returns>The artifact publish to blueprint</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IPublishArtifactResult> PublishArtifacts(List<IOpenApiArtifact> artifactList, IUser user, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null);

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
        /// Returns true for published artifact and false for unpublished. Method checks Version property.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>True for published artifact, false for unpublished artifact.</returns>
        bool IsArtifactPublished(IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
