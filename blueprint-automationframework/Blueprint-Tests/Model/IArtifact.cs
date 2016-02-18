using System;
using System.Collections.Generic;
using System.Net;

namespace Model
{

    public interface IArtifact : IArtifactBase
    {
        // TODO Discuss with team how to update class referring IArtifact
        int ProjectId { get; set; }
        int Version { get; set; }
        int ParentId { get; set; }
        Uri BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
    }

    public interface IOpenApiArtifact : IArtifactBase
    {
        int ProjectId { get; set; }
        int Version { get; set; }
        int ParentId { get; set; }
        Uri BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
        string ArtifactTypeName { get; set; }
        string BaseArtifactType { get; set; }
        bool AreTracesReadOnly { get; set; }
        bool AreAttachmentsReadOnly { get; set; }
        bool AreDocumentReferencesReadOnly { get; set; }
        List<IOpenApiProperty> Properties { get; }
        List<IOpenApiComment> Comments { get; }
        List<IOpenApiTrace> Traces { get; }
        List<IOpenApiAttachment> Attachments { get; }
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
        IOpenApiArtifactResult DeleteArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);

    }
}
