namespace ActionHandlerService.Models
{
    public class SqlWorkflowEvent
    {
        public int HolderId { get; set; }
        public int VersionItemId { get; set; }
        public int CurrentStateId { get; set; }
        public int WorkflowId { get; set; }
        public int? RequiredPreviousStateId { get; set; }
        public int? RequiredNewStateId { get; set; }
        public string Triggers { get; set; }
        public int EventType { get; set; }
        public int? EventPropertyTypeId { get; set; }
    }
}
