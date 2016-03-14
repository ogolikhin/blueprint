using Model.Impl;
using System.Collections.Generic;
using System.Net;

namespace Model
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
        List<OpenApiProperty> Properties { get; }
        List<OpenApiComment> Comments { get; }
        List<OpenApiTrace> Traces { get; }
        List<OpenApiAttachment> Attachments { get; }

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
        /// </summary>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <param name="deleteChildren">(optional) Specifies whether or not to also delete all child artifacts of the specified artifact</param>
        /// <returns>The DeletedArtifactResult list after delete artifact call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        List<IDeleteArtifactResult> DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null, bool deleteChildren = false);

        /// <summary>
        /// Publish the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which define the whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Publish(IUser user, bool shouldKeepLock = false, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Save the artifact on Blueprint server.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Save(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete the artifact on Blueprint server.
        /// To delete artifact permanently Publish must be called after Delete, otherwise deletion can be discarded.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        void Delete(IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Returns true for published artifact and false for unpublished. Method checks Version property.
        /// </summary>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>True for published artifact, false for unpublished artifact.</returns>
        bool IsArtifactPublished(IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
