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

    /// <summary>
    /// Interface containing properties that are shared between types such as NovaTrace and Relationship.
    /// </summary>
    public interface INovaTrace
    {
        int ArtifactId { get; set; }
        string ArtifactTypePrefix { get; set; }
        string ArtifactName { get; set; }
        int ItemId { get; set; }
        string ItemTypePrefix { get; set; }
        string ItemName { get; set; }
        string ItemLabel { get; set; }
        int ProjectId { get; set; }
        string ProjectName { get; set; }
        TraceDirection Direction { get; set; }
        bool IsSuspect { get; set; }
        bool HasAccess { get; set; }
        int PrimitiveItemTypePredefined { get; set; }
    }
}
