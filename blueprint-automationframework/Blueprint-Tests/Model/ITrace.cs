using System;

namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface ITrace
    {
        // TODO Future development
    }

    public interface IOpenApiTrace
    {
        string Type { get; set; }
        string Direction { get; set; }
        int ProjectId { get; set; }
        int ArtifactId { get; set; }
        string ArtifactPropertyName { get; set; }
        string Label { get; set; }
        Uri BlueprintUrl { get; set; }
        string Link { get; set; }
        bool IsSuspect { get; set; }
    }
}
