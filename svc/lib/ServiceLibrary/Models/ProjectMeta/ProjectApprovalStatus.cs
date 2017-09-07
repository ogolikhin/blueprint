namespace ServiceLibrary.Models.ProjectMeta
{
    public class ProjectApprovalStatus
    {
        public string StatusText { get; set; }
        public ApprovalType ApprovalType { get; set; }
        public bool IsPreset { get; set; }
    }
}
