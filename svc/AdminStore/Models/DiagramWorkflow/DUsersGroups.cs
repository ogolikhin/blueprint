using System.Collections.Generic;

namespace AdminStore.Models.DiagramWorkflow
{
    public class DUsersGroups
    {
        public IEnumerable<DUserGroup> UsersGroups { get; set; }
        public bool? IncludeCurrentUser { get; set; }

    }
}