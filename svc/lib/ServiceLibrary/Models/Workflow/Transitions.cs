namespace ServiceLibrary.Models.Workflow
{
    public class Transitions
    {
        public int WorkflowId { get; set; }
        public int TransitionId { get; set; }
        public string TransitionLabel { get; set; }
        public int FromStateId { get; set; }
        public int ToStateId { get; set; }
    }
}