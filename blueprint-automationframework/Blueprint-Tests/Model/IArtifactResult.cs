namespace Model
{
    public interface IArtifactResultBase
    {
        string Message { get; set; }
        string ResultCode { get; set; }
    }

    public interface IArtifactResult : IArtifactResultBase
    {
        IArtifact Artifact { get; set; }
    }

    public interface IOpenApiArtifactResult : IArtifactResultBase
    {
        IOpenApiArtifact Artifact { get; set; }
    }
}
