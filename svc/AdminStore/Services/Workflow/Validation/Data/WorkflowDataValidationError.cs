namespace AdminStore.Services.Workflow.Validation.Data
{
    public class WorkflowDataValidationError
    {
        public object Element { get; set; }
        public WorkflowDataValidationErrorCodes ErrorCode { get; set; }
    }
}