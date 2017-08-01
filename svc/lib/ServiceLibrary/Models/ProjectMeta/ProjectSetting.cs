namespace ServiceLibrary.Models.ProjectMeta
{
    public class ProjectSetting
    {
        public int ProjectSettingId { get; set; }
        public int ProjectId { get; set; }
        public bool ReadOnly { get; set; }
        public PropertyTypePredefined Predefined { get; set; }
        public string Setting { get; set; }
        public int OrderIndex { get; set; }
        public bool Deleted { get; set; }
    }
}
