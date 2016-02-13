using System;
using System.Net;
using System.Collections.Generic;

namespace Model
{

    public interface IArtifact : IArtifactBase
    {
        // TODO Find the way or wait for the API implementation which retrieves ArtifactType
        //ArtifactType ArtifactType { get; set; }
        // TODO Find the way or wait for the API implementation which retrieve descrption
        //string Description { get; set; }
        int ProjectId { get; set; }
        int Version { get; set; }
        int ParentId { get; set; }
        Uri BlueprintUrl { get; set; }
        int ArtifactTypeId { get; set; }
        IArtifact AddArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
        IArtifactResultBase DeleteArtifact(IArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
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
        void UpdateArtifactType(int projectId, IOpenApiArtifact artifact);
        IOpenApiArtifactResult AddArtifact(IOpenApiArtifact artifact, IUser user, List<HttpStatusCode> expectedStatusCodes = null);
        IOpenApiArtifact UpdateArtifact(IProject project, IOpenApiArtifact artifact, string propertyName = null, string propertyValue = null);
    }
}
