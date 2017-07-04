namespace ArtifactStore.Models.Reuse
{
    public class SqlReuseSettingsInfo
    {
        public int Id { get; set; }

        public int InstanceTypeId { get; set; }

        public bool? AllowReadOnlyOverride { get; set; }

        public int? ReadOnlySettings { get; set; }

        public int? SensitivitySettings { get; set; }

        public int PropertyTypeId {get;set;}

        public int PropertyTypePredefined { get; set; }

        public int Settings { get; set; }
    }
}
