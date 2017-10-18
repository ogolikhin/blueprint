using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Helpers.Workflow
{
    public static class WorkflowHelper
    {
        public static bool CollectionEquals<T>(IEnumerable<T> col1, IEnumerable<T> col2)
        {
            if (ReferenceEquals(col1, col2))
            {
                return true;
            }

            if (col1.IsEmpty() && col2.IsEmpty())
            {
                return true;
            }

            if (col1?.GetType() != col2?.GetType())
            {
                return false;
            }
            var list1 = col1.ToList();
            var list2 = col2.ToList();
            if (list1.Count != list2.Count)
            {
                return false;
            }

            return !list1.Where((t, i) => !t.Equals(list2[i])).Any();
        }

        public static T CloneViaXmlSerialization<T>(T serObject) where T : class
        {
            if (serObject == null)
            {
                return null;
            }

            var xml = SerializationHelper.ToXml(serObject);
            var clone = SerializationHelper.FromXml<T>(xml);
            return clone;
        }


        #region Name and Description Properties

        private static readonly IDictionary<int, string> NameAndDescriptionMap = new Dictionary<int, string>
        {
            {WorkflowConstants.PropertyTypeFakeIdName, WorkflowConstants.PropertyNameName},
            {WorkflowConstants.PropertyTypeFakeIdDescription, WorkflowConstants.PropertyNameDescription} 
        };

        public static bool IsNameOrDescriptionProperty(string propertyTypeName)
        {
            return NameAndDescriptionMap.Values.Contains(propertyTypeName);
        }

        public static bool IsNameOrDescriptionProperty(int propertyId)
        {
            return NameAndDescriptionMap.Keys.Contains(propertyId);
        }

        public static bool TryGetNameOrDescriptionPropertyTypeName(int propertyTypeId, out string propertyTypeName)
        {
            return NameAndDescriptionMap.TryGetValue(propertyTypeId, out propertyTypeName);
        }

        public static bool TryGetNameOrDescriptionPropertyTypeId(string propertyTypeName, out int propertyTypeId)
        {
            propertyTypeId = NameAndDescriptionMap.FirstOrDefault(kv => kv.Value == propertyTypeName).Key;
            return IsNameOrDescriptionProperty(propertyTypeId);
        }

        public static bool TryGetNameOrDescriptionPropertyType(int propertyTypeId, out PropertyType propertyType)
        {
            string propertyTypeName;
            if (!NameAndDescriptionMap.TryGetValue(propertyTypeId, out propertyTypeName))
            {
                propertyType = null;
                return false;
            }

            propertyType = new PropertyType
            {
                Id = propertyTypeId,
                Name = propertyTypeName,
                PrimitiveType = PropertyPrimitiveType.Text,
                IsRequired = propertyTypeId == WorkflowConstants.PropertyTypeFakeIdName,
                IsRichText = propertyTypeId != WorkflowConstants.PropertyTypeFakeIdName
            };

            return true;
        }

        public static bool TryGetNameOrDescriptionPropertyType(string propertyTypeName, out PropertyType propertyType)
        {
            int propertyTypeId;
            if (!TryGetNameOrDescriptionPropertyTypeId(propertyTypeName, out propertyTypeId))
            {
                propertyType = null;
                return false;
            }
            return TryGetNameOrDescriptionPropertyType(propertyTypeId, out propertyType);
        }

        #endregion


            #region NormalizeWorkflow

            // Remove empty collections and nullable boolean properties with the default value.
        public static IeWorkflow NormalizeWorkflow(IeWorkflow workflow)
        {
            if (workflow == null)
            {
                return null;
            }

            workflow.States = NormalizeList(workflow.States);
            workflow.TransitionEvents = NormalizeList(workflow.TransitionEvents);
            workflow.PropertyChangeEvents = NormalizeList(workflow.PropertyChangeEvents);
            workflow.NewArtifactEvents = NormalizeList(workflow.NewArtifactEvents);
            workflow.Projects = NormalizeList(workflow.Projects);

            workflow.States?.ForEach(s => s.IsInitial = NormalizeNullableBool(s.IsInitial));
            workflow.TransitionEvents?.ForEach(NormalizeTransitionEvent);
            workflow.PropertyChangeEvents?.ForEach(NormalizeEvent);
            workflow.NewArtifactEvents?.ForEach(NormalizeEvent);
            workflow.Projects?.ForEach(p => p.ArtifactTypes = NormalizeList(p.ArtifactTypes));

            return workflow;
        }

        private static bool? NormalizeNullableBool(bool? flag)
        {
            return flag.GetValueOrDefault() ? true : (bool?)null;
        }

        private static List<T> NormalizeList<T>(List<T> list)
        {
            return list.IsEmpty() ? null : list;
        }

        private static void NormalizeEvent(IeEvent wEvent)
        {
            if (wEvent == null)
            {
                return;
            }

            wEvent.Triggers = NormalizeList(wEvent.Triggers);
            wEvent.Triggers?.ForEach(t => NormalizeAction(t.Action));
        }

        private static void NormalizeTransitionEvent(IeTransitionEvent tEvent)
        {
            if (tEvent == null)
            {
                return;
            }

            tEvent.PermissionGroups = NormalizeList(tEvent.PermissionGroups);
            tEvent.SkipPermissionGroups = NormalizeNullableBool(tEvent.SkipPermissionGroups);
            NormalizeEvent(tEvent);
        }

        private static void NormalizeAction(IeBaseAction action)
        {
            if (action == null)
            {
                return;
            }

            switch (action.ActionType)
            {
                case ActionTypes.EmailNotification:
                    NormalizeEmailNotificationAction((IeEmailNotificationAction)action);
                    break;
                case ActionTypes.PropertyChange:
                    NormalizePropertyChangeAction((IePropertyChangeAction)action);
                    break;
                case ActionTypes.Generate:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(action.ActionType));
            }
        }

        private static void NormalizeEmailNotificationAction(IeEmailNotificationAction action)
        {
            if (action == null)
            {
                return;
            }

            action.Emails = NormalizeList(action.Emails);
        }

        private static void NormalizePropertyChangeAction(IePropertyChangeAction action)
        {
            if (action == null)
            {
                return;
            }

            action.ValidValues = NormalizeList(action.ValidValues);
            action.UsersGroups = NormalizeUsersGroups(action.UsersGroups);
        }

        private static IeUsersGroups NormalizeUsersGroups(IeUsersGroups usersGroups)
        {
            if (usersGroups == null)
            {
                return null;
            }

            usersGroups.UsersGroups = NormalizeList(usersGroups.UsersGroups);
            usersGroups.UsersGroups?.ForEach(NormalizeUserGroup);
            usersGroups.IncludeCurrentUser = NormalizeNullableBool(usersGroups.IncludeCurrentUser);

            return usersGroups;
        }

        private static void NormalizeUserGroup(IeUserGroup userGroup)
        {
            userGroup.IsGroup = NormalizeNullableBool(userGroup.IsGroup);
        }

        #endregion

    }
}