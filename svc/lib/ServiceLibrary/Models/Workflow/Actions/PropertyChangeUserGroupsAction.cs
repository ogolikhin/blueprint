using System;
using System.Collections.Generic;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.Workflow.Actions
{
    public class PropertyChangeUserGroupsAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public List<UserGroup> UserGroups { get; } = new List<UserGroup>();

        protected override PropertySetResult PopulatePropertyLite(WorkflowPropertyType propertyType)
        {
            if (!propertyType.PrimitiveType.HasValue ||
                propertyType.PrimitiveType.Value != PropertyPrimitiveType.User ||
                !String.IsNullOrEmpty(PropertyValue))
            {
                return new PropertySetResult(InstancePropertyTypeId, ErrorCodes.InvalidArtifactProperty,
                    "Property type is not a user property anymore. Property change action is currently invalid");
            }

            PropertyLiteValue = new PropertyLite()
            {
                PropertyTypeId = InstancePropertyTypeId
            };
            PropertyLiteValue.UsersAndGroups.AddRange(UserGroups);
            return null;
        }
    }

}