using System.Collections.Generic;
using ServiceLibrary.Models.Enums;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DPropertyChangeAction : DBaseAction
    {
        public override ActionTypes ActionType => ActionTypes.PropertyChange;
        public string PropertyName { get; set; }
        public int? PropertyId { get; set; }
        public string PropertyValue { get; set; }
        public IEnumerable<DValidValue> ValidValues { get; set; }
        public DUsersGroups UsersGroups { get; set; }
    }
}