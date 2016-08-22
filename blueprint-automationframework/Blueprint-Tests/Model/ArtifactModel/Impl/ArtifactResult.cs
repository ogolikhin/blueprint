using System;
using System.Net;
using System.Collections.Generic;
using Newtonsoft.Json;
using Utilities;
using System.Diagnostics.CodeAnalysis;
using Model.Impl;

namespace Model.ArtifactModel.Impl
{
    public enum LockResult
    {
        Success,
        AlreadyLocked,
        Failure
    }

    public enum DiscardResult
    {
        Success,
        Failure
    }

    public class UpdateArtifactResult
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<string> Messages { get; set; }
        public ArtifactDetails Result { get; set; }
    }

    public class OpenApiAddArtifactResult
    {
        [JsonConverter(typeof(Deserialization.ConcreteConverter<OpenApiArtifact>))]
        public IOpenApiArtifact Artifact { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    public class OpenApiUpdateArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    public class PublishArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
        public int ProjectId { get; set; }
    }

    public class DiscardArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
        public int ProjectId { get; set; }
    }

    public class NovaDiscardArtifactResult
    {
        public enum ResultCode
        {
            Success = 0,
            ArtifactHasNothingToDiscard = 1,
            Failure = 2
        }

        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public ResultCode Result { get; set; }
        public int ProjectId { get; set; }
    }

    public class NovaPublishArtifactResult
    {
        public enum Result
        {
            None = 0, //CA1008
            Success = 200,
            ArtifactAlreadyPublished = 113,
            Failure = 1 // TODO: check code for fail
        }

        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public Result StatusCode { get; set; }
        public int ProjectId { get; set; }
    }

    public class NovaDiscardArtifactResults
    {
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<NovaDiscardArtifactResult> DiscardResults { get; set; }
    }

    public class DeleteArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    // Todo Refactor to get rid of this if possible
    // This is required because the OpenApi call only returns an ArtifactId rather than
    // an Artifact.
    public class FailedArtifactResult
    {
        public int ArtifactId { get; set; }
        public string Message { get; set; }
        public HttpStatusCode ResultCode { get; set; }
    }

    public class LockResultInfo
    {
        public LockResult Result { get; set; }

        public VersionInfo Info { get; set; }
    }

    public class VersionInfo
    {
        public int? ArtifactId { get; set; }
        /// <summary>
        /// The historical version id of the artifact
        /// </summary>
        public int ServerArtifactVersionId { get; set; }
        public DateTime? UtcLockedDateTime { get; set; }
        public string LockOwnerLogin { get; set; }
        public int? ProjectId { get; set; }
    }

    public class DiscardResultInfo
    {
        public DiscardResult Result { get; set; }
        public string Message { get; set; }
    }
}
