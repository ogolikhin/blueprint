namespace AdminStore.Models.DiagramWorkflow {
    public class DTrigger
    {
        public string Name { get; set; }
        public DBaseAction Action { get; set; }
        public DStateCondition Condition { get; set; }
    }
}