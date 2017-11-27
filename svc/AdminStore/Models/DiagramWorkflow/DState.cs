namespace AdminStore.Models.DiagramWorkflow
{
    public class DState
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public bool? IsInitial { get; set; }
        public float OrderIndex { get; set; }
        public string Location { get; set; }
    }
}