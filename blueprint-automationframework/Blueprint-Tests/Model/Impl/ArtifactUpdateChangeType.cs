
namespace Model.Impl
{
    /// <summary>
    ///  To use in artifact/subartifact details to update artifact via PATCH svc/bpartifactstore/artifacts/{artifactId}
    /// </summary>
    public enum ArtifactUpdateChangeType
    {
        /// <summary>add new entity</summary>
        Add = 0,
        /// <summary>update entity</summary>
        Update = 1,
        /// <summary>delete entity</summary>
        Delete = 2
    }
}
