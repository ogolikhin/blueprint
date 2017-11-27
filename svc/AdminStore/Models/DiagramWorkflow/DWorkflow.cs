using System.Collections.Generic;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DWorkflow
    {
        public int? Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public IEnumerable<DState> States { get; set; }
        public IEnumerable<DTransitionEvent> TransitionEvents { get; set; }
        public IEnumerable<DPropertyChangeEvent> PropertyChangeEvents { get; set; }
        public IEnumerable<DNewArtifactEvent> NewArtifactEvents { get; set; }
        public IEnumerable<DProject> Projects { get; set; }
    }
}