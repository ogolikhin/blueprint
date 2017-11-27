using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public abstract class DBaseAction
    {
        public abstract ActionTypes ActionType { get; }
        public string Name { get; set; }
    }
}