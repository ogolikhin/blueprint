using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;

namespace Model.ArtifactModel.Impl
{
    public enum LockResult
    {
        Success,
        AlreadyLocked,
        DoesNotExist,
        AccessDenied,
        Failure
    }

    public enum DiscardResult
    {
        Success,
        Failure
    }

    public class OpenApiAddArtifactResult
    {
        [JsonConverter(typeof(SerializationUtilities.ConcreteConverter<OpenApiArtifact>))]
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

    public class SaveArtifactResult
    {
        public string Message { get; set; }
        public int ErrorCode { get; set; }
    }

    public class OpenApiPublishArtifactResult
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
        public List<NovaDiscardArtifactResult> DiscardResults { get; set; }
    }

    public class OpenApiDeleteArtifactResult
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

    /// <summary>
    /// This is the structure returned by the REST call to display error messages.
    /// </summary>
    public class MessageResult
    {
        public string Message { get; set; }
    }
}
