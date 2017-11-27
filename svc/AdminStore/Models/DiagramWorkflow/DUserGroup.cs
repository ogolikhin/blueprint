namespace AdminStore.Models.DiagramWorkflow
{
    public class DUserGroup
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool? IsGroup { get; set; }
        public string GroupProjectPath { get; set; }
        public int? GroupProjectId { get; set; }
    }
}