namespace ServiceLibrary.Models.VersionControl
{
    public interface IBaseArtifactVersionControlInfo
    {
        int Id { get; set; }

        string Name { get; set; }

        int ProjectId { get; set; }

        int ItemTypeId { get; set; }

        ItemTypePredefined PredefinedType { get; set; }
    }
}
