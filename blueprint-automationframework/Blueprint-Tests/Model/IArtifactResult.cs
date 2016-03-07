namespace Model
{
    public interface IArtifactResult<T>
    { 
        T Artifact { get; set; }
        string Message { get; set; }
        string ResultCode { get; set; }
    }

    public interface IPublishArtifactResult
    {
        string ArtifactId { get; set; }
        string ProjectId { get; set; }
        string Message { get; set; }
        string ResultCode { get; set; }
    }

    public interface IDeleteArtifactResult
    {
        string ArtifactId { get; set; }
        string Message { get; set; }
        string ResultCode { get; set; }
    }
}
