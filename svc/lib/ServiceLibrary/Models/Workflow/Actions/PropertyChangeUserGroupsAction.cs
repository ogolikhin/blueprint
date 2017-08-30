using System.Collections.Generic;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class PropertyChangeUserGroupsAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public List<UserGroup> UserGroups { get; } = new List<UserGroup>();

        protected override PropertySetResult PopulatePropertyLite(WorkflowPropertyType propertyType)
        {
            var baseResult = base.PopulatePropertyLite(propertyType);
            if (baseResult != null)
            {
                return baseResult;
            }
            PropertyLiteValue.UsersAndGroups.AddRange(UserGroups);
            return null;
        }
    }

}