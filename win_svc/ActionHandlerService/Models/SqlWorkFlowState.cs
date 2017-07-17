namespace ActionHandlerService.Models
{
    public class SqlWorkFlowState
    {
        public int? Result { get; set; }
        public int WorkflowId { get; set; }
        public string WorkflowStateName { get; set; }
        public int WorkflowStateId { get; set; }
    }
}
