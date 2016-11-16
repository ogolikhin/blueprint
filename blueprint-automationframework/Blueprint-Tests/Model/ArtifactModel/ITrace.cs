using Model.ArtifactModel.Impl;

namespace Model.ArtifactModel
{
    public interface ITrace
    {
        int ProjectId { get; set; }

        int ArtifactId { get; set; }

        TraceDirection Direction { get; set; }

        bool IsSuspect { get; set; }
    }
}
