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
        List<IOpenApiProperty> Properties { get; }
        List<IOpenApiComment> Comments { get; }
        List<IOpenApiTrace> Traces { get; }
        List<IOpenApiAttachment> Attachments { get; }

        /// <summary>
        /// Property Setter
        /// </summary>
        /// <param name="properties"> list of property</param>
        void SetProperties(List<IOpenApiProperty> properties);

        /// <summary>
        /// Adds the artifact to Blueprint
        /// </summary>
        /// <param name="artifact">The artifact to add.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.  By default, only '201' is expected.</param>
        /// <returns>The artifact added to blueprint</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        IOpenApiArtifact AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

        /// <summary>
        /// Delete the artifact to Blueprint.
        /// </summary>
        /// <param name="artifact">The artifact to delete.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">A list of expected status codes.</param>
        /// <returns>The artifactResult after delete artifact call</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        IArtifactResult<IOpenApiArtifact> DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
    }
}
