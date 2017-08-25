﻿using System.Collections.Generic;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Models.Workflow.Actions
{
    public class PropertyChangeUserGroupsAction : PropertyChangeAction
    {
        // Used for User properties and indicates that PropertyValue contains the group name.
        public List<UserGroup> UserGroups { get; } = new List<UserGroup>();

        protected override void PopulatePropertyLite(DPropertyType propertyType)
        {
            base.PopulatePropertyLite(propertyType);
            PropertyLiteValue.UsersAndGroups.AddRange(UserGroups);
        }
    }

}