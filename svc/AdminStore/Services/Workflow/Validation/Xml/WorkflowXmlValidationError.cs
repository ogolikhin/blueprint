namespace AdminStore.Services.Workflow.Validation.Xml
{
    public class WorkflowXmlValidationError
    {
        public object Element { get; set; }
        public WorkflowXmlValidationErrorCodes ErrorCode { get; set; }
    }
}