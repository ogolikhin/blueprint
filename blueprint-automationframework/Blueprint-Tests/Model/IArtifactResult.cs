namespace Model
{
    public interface IArtifactResult<T>
    { 
        T Artifact { get; set; }
        string Message { get; set; }
        string ResultCode { get; set; }
    }
}
