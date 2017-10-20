using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.Workflow;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public class TriggerConverter : ITriggerConverter
    {
        #region Interface Implementation

        public XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, WorkflowDataMaps dataMaps)
        {
            if (ieTriggers == null)
            {
                return null;
            }

            var xmlTriggers = new XmlWorkflowEventTriggers();
            ieTriggers.ForEach(t => xmlTriggers.Triggers.Add(ToXmlModel(t, dataMaps)));
            return xmlTriggers;
        }

        public IEnumerable<IeTrigger> FromXmlModel(XmlWorkflowEventTriggers xmlTriggers, WorkflowDataNameMaps dataMaps,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect)
        {
            if (xmlTriggers == null)
            {
                return null;
            }

            var triggers = new List<IeTrigger>();

                xmlTriggers.Triggers?.ForEach(t =>
                {
                    var trigger = FromXmlModel(t, dataMaps, userIdsToCollect, groupIdsToCollect);
                    if (trigger != null)
                    {
                        triggers.Add(trigger);
                    }
                });

            return triggers;
        }

        #endregion

        #region Private Methods

        private static XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, WorkflowDataMaps dataMaps)
        {
            if (ieTrigger == null)
            {
                return null;
            }

            var xmlTrigger = new XmlWorkflowEventTrigger
            {
                Name = ieTrigger.Name
            };

            if (ieTrigger.Condition != null)
            {
                xmlTrigger.Condition = ToXmlModel(ieTrigger.Condition, dataMaps.StateMap);
            }

            // Triggers must have an action.
            xmlTrigger.Action = ToXmlModel(ieTrigger.Action, dataMaps);

            return xmlTrigger;
        }

        private static IeTrigger FromXmlModel(XmlWorkflowEventTrigger xmlTrigger, WorkflowDataNameMaps dataMaps,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect)
        {
            if (xmlTrigger == null)
            {
                return null;
            }

            var action = FromXmlModel(xmlTrigger.Action, dataMaps, userIdsToCollect, groupIdsToCollect);
            var ieTrigger = action != null
                ? new IeTrigger
                {
                    Name = xmlTrigger.Name,
                    Action = action,
                    Condition = FromXmlModel(xmlTrigger.Condition, dataMaps)
                }
                : null;

            return ieTrigger;
        }

        private static IeBaseAction FromXmlModel(XmlAction xmlAction, WorkflowDataNameMaps dataMaps,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect)
        {
            if (xmlAction == null)
            {
                return null;
            }

            string name = null;
            IeBaseAction action = null;

            switch (xmlAction.ActionType)
            {
                case ActionTypes.EmailNotification:
                    var xeAction = xmlAction as XmlEmailNotificationAction;
                    action = !xeAction.PropertyTypeId.HasValue
                        || dataMaps.PropertyTypeMap.TryGetValue(xeAction.PropertyTypeId.Value, out name)
                        ? new IeEmailNotificationAction
                        {
                            Name = xeAction.Name,
                            Emails = xeAction.Emails,
                            PropertyId = xeAction.PropertyTypeId,
                            PropertyName = name,
                            Message = xeAction.Message
                        }
                        : null;
                    break;
                case ActionTypes.PropertyChange:
                    var xpAction = xmlAction as XmlPropertyChangeAction;
                    var isPropertyFound = dataMaps.PropertyTypeMap.TryGetValue(xpAction.PropertyTypeId, out name)
                        || WorkflowHelper.TryGetNameOrDescriptionPropertyTypeName(xpAction.PropertyTypeId, out name);

                    action = isPropertyFound
                        ? new IePropertyChangeAction
                        {
                            Name = xpAction.Name,
                            PropertyId = xpAction.PropertyTypeId,
                            PropertyName = name,
                            PropertyValue = xpAction.PropertyValue,
                            ValidValues = FromXmlModel(xpAction.ValidValues, dataMaps),
                            UsersGroups = FromXmlModel(xpAction.UsersGroups, userIdsToCollect, groupIdsToCollect)
                        }
                        : null;
                    break;
                case ActionTypes.Generate:
                    var xgAction = xmlAction as XmlGenerateAction;
                    action = xgAction.GenerateActionType != GenerateActionTypes.Children
                        || (xgAction.ArtifactTypeId.HasValue
                        && dataMaps.ArtifactTypeMap.TryGetValue(xgAction.ArtifactTypeId.Value, out name))
                        ? new IeGenerateAction
                        {
                            Name = xgAction.Name,
                            GenerateActionType = xgAction.GenerateActionType,
                            ChildCount = xgAction.ChildCount,
                            ArtifactTypeId = xgAction.ArtifactTypeId,
                            ArtifactType = name
                        }
                        : null;
                    break;
                default:
                    break;
            }

            return action;
        }

        private static IeUsersGroups FromXmlModel(XmlUsersGroups xmlUsersGroups,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect)
        {
            if (xmlUsersGroups == null)
            {
                return null;
            }

            var ieUsersGroups = new IeUsersGroups
            {
                IncludeCurrentUser = xmlUsersGroups.IncludeCurrentUser,
                UsersGroups = FromXmlModel(xmlUsersGroups.UsersGroups, userIdsToCollect, groupIdsToCollect)
            };

            return ieUsersGroups;
        }

        private static List<IeValidValue> FromXmlModel(List<int> valueIds, WorkflowDataNameMaps dataMaps)
        {
            var values = new List<IeValidValue>();
            valueIds?.ForEach(id =>
            {
                string value;
                if (dataMaps.ValidValueMap.TryGetValue(id, out value))
                {
                    values.Add(new IeValidValue
                    {
                        Id = id,
                        Value = value
                    });
                }
            });

            return values.Any() ? values : null;
        }

        private static List<IeUserGroup> FromXmlModel(List<XmlUserGroup> xmlUserGroups,
            ISet<int> userIdsToCollect, ISet<int> groupIdsToCollect)
        {
            if (xmlUserGroups.IsEmpty())
            {
                return null;
            }

            var userGroups = new List<IeUserGroup>();
            foreach (var g in xmlUserGroups)
            {
                if (!g.IsGroup.HasValue)
                {
                    continue;
                }

                if (g.IsGroup.GetValueOrDefault())
                {
                    // Name and GroupProjectId properties will be assigned later after converting the entire workflow
                    // since we need to know all group Ids to retrieve group information form the database.
                    var group = new IeUserGroup
                    {
                        Id = g.Id,
                        IsGroup = g.IsGroup
                    };
                    userGroups.Add(group);
                    groupIdsToCollect.Add(g.Id);
                }
                else
                {
                    // Name and property will be assigned later after converting the entire workflow
                    // since we need to know all user Ids to retrieve user information form the database.
                    var user = new IeUserGroup
                    {
                        Id = g.Id,
                        IsGroup = false
                    };
                    userGroups.Add(user);
                    userIdsToCollect.Add(g.Id);
                }
            }

            return userGroups;
        }

        private static IeCondition FromXmlModel(XmlCondition xmlCondition, WorkflowDataNameMaps dataMaps)
        {
            if (xmlCondition == null)
            {
                return null;
            }

            switch (xmlCondition.ConditionType)
            {
                case ConditionTypes.State:
                    string name;
                    var stateId = (xmlCondition as XmlStateCondition).StateId;
                    var ieCondition = dataMaps.StateMap.TryGetValue(stateId, out name)
                        ? new IeStateCondition
                        {
                            StateId = stateId,
                            State = name
                        }
                        : null;
                    return ieCondition;
                default:
                    throw new ArgumentOutOfRangeException(nameof(xmlCondition.ConditionType));
            }
        }

        private static XmlAction ToXmlModel(IeBaseAction ieAction, WorkflowDataMaps dataMaps)
        {
            if (ieAction == null)
            {
                return null;
            }

            switch (ieAction.ActionType)
            {
                case ActionTypes.EmailNotification:
                    return ToXmlModel(ieAction as IeEmailNotificationAction, dataMaps.PropertyTypeMap);
                case ActionTypes.PropertyChange:
                    return ToXmlModel(ieAction as IePropertyChangeAction, dataMaps);
                case ActionTypes.Generate:
                    return ToXmlModel(ieAction as IeGenerateAction, dataMaps.ArtifactTypeMap);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ieAction.ActionType));
            }
        }

        private static XmlEmailNotificationAction ToXmlModel(IeEmailNotificationAction ieAction, IDictionary<string, int> propertyTypeMap)
        {
            if (ieAction == null)
            {
                return null;
            }

            var xmlAction = new XmlEmailNotificationAction
            {
                Name = ieAction.Name,
                Emails = ieAction.Emails,
                Message = ieAction.Message
            };

            if (ieAction.PropertyName != null)
            {
                int propertyTypeId;
                if (!propertyTypeMap.TryGetValue(ieAction.PropertyName, out propertyTypeId))
                {
                    throw new ExceptionWithErrorCode(
                        I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.",
                            ieAction.PropertyName),
                        ErrorCodes.UnexpectedError);
                }
                xmlAction.PropertyTypeId = propertyTypeId;
            }
            return xmlAction;
        }

        private static XmlPropertyChangeAction ToXmlModel(IePropertyChangeAction ieAction, WorkflowDataMaps dataMaps)
        {
            if (ieAction == null)
            {
                return null;
            }

            var xmlAction = new XmlPropertyChangeAction
            {
                Name = ieAction.Name,
                PropertyValue = ieAction.PropertyValue,
                UsersGroups = ToXmlModel(ieAction.UsersGroups, dataMaps)
            };

            int propertyTypeId;
            if (!dataMaps.PropertyTypeMap.TryGetValue(ieAction.PropertyName, out propertyTypeId)
                 && !WorkflowHelper.TryGetNameOrDescriptionPropertyTypeId(ieAction.PropertyName, out propertyTypeId))
            {
                throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.", ieAction.PropertyName),
                    ErrorCodes.UnexpectedError);
            }
            xmlAction.PropertyTypeId = propertyTypeId;

            IDictionary<string, int> vvMap = null;
            ieAction.ValidValues?.ForEach(vv =>
            {
                if (vvMap == null)
                {
                    if (!dataMaps.ValidValueMap.TryGetValue(propertyTypeId, out vvMap))
                    {
                        throw new ExceptionWithErrorCode(
                            I18NHelper.FormatInvariant(
                                "Valid Values of Choice Standard Property Type '{0}' are not found.",
                                ieAction.PropertyName),
                            ErrorCodes.UnexpectedError);
                    }
                }

                int vvId;
                if (!vvMap.TryGetValue(vv.Value, out vvId))
                {
                    throw new ExceptionWithErrorCode(
                        I18NHelper.FormatInvariant(
                            "Valid Value '{0}' of Choice Standard Property Type '{1}' is not found.",
                            vv.Value, ieAction.PropertyName),
                        ErrorCodes.UnexpectedError);
                }

                (xmlAction.ValidValues ?? (xmlAction.ValidValues = new List<int>())).Add(vvId);
            });

            return xmlAction;
        }

        private static XmlUsersGroups ToXmlModel(IeUsersGroups ieUsersGroups, WorkflowDataMaps dataMaps)
        {
            if (ieUsersGroups == null)
            {
                return null;
            }

            var xmlUsersGroups = new XmlUsersGroups
            {
                IncludeCurrentUser = ieUsersGroups.IncludeCurrentUser,
                UsersGroups = !ieUsersGroups.UsersGroups.IsEmpty()
                    ? new List<XmlUserGroup>()
                    : null
            };

            ieUsersGroups.UsersGroups?.ForEach(ug =>
            {
                var isGroup = ug.IsGroup.GetValueOrDefault();
                int ugId;
                if (isGroup)
                {
                    if (!dataMaps.GroupMap.TryGetValue(Tuple.Create(ug.Name, ug.GroupProjectId), out ugId))
                    {
                        throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Group '{1}' is not found.", ug.Name),
                            ErrorCodes.UnexpectedError);
                    }
                }
                else
                {
                    if (!dataMaps.UserMap.TryGetValue(ug.Name, out ugId))
                    {
                        throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of User '{0}' is not found.", ug.Name),
                            ErrorCodes.UnexpectedError);
                    }
                }

                xmlUsersGroups.UsersGroups.Add(new XmlUserGroup
                {
                    IsGroup = isGroup,
                    Id = ugId
                });
            });

            return xmlUsersGroups;
        }

        private static XmlGenerateAction ToXmlModel(IeGenerateAction ieAction, IDictionary<string, int> artifactTypeMap)
        {
            if (ieAction == null)
            {
                return null;
            }

            var xmlAction = new XmlGenerateAction
            {
                Name = ieAction.Name,
                GenerateActionType = ieAction.GenerateActionType
            };
            switch (ieAction.GenerateActionType)
            {
                case GenerateActionTypes.Children:
                    xmlAction.ChildCount = ieAction.ChildCount;
                    int artifactTypeId;
                    if (!artifactTypeMap.TryGetValue(ieAction.ArtifactType, out artifactTypeId))
                    {
                        throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Standard Artifact Type '{0}' is not found.", ieAction.ArtifactType),
                            ErrorCodes.UnexpectedError);
                    }
                    xmlAction.ArtifactTypeId = artifactTypeId;
                    break;
                case GenerateActionTypes.UserStories:
                case GenerateActionTypes.TestCases:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return xmlAction;
        }

        private static XmlStateCondition ToXmlModel(IeCondition ieCondition, IDictionary<string, int> stateMap)
        {
            if (ieCondition == null)
            {
                return null;
            }

            switch (ieCondition.ConditionType)
            {
                case ConditionTypes.State:
                    return ToXmlModel(ieCondition as IeStateCondition, stateMap);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ieCondition.ConditionType));
            }
        }

        private static XmlStateCondition ToXmlModel(IeStateCondition ieCondition, IDictionary<string, int> stateMap)
        {
            if (ieCondition == null)
            {
                return null;
            }

            int stateId;
            if (!stateMap.TryGetValue(ieCondition.State, out stateId))
            {
                throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of State '{0}' is not found.", ieCondition.State),
                    ErrorCodes.UnexpectedError);
            }

            var xmlCondition = new XmlStateCondition { StateId = stateId };
            return xmlCondition;
        }

        #endregion
    }
}