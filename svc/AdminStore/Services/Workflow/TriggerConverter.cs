using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;
using System.Globalization;

namespace AdminStore.Services.Workflow
{
    public class TriggerConverter : ITriggerConverter
    {
        #region Interface Implementation

        public XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, WorkflowDataMaps dataMaps, int currentUserId)
        {
            if (ieTriggers == null)
            {
                return null;
            }

            var xmlTriggers = new XmlWorkflowEventTriggers();
            ieTriggers?.ForEach(t => xmlTriggers.Triggers.Add(ToXmlModel(t, dataMaps, currentUserId)));
            return xmlTriggers;
        }

        public XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, WorkflowDataMaps dataMaps, int currentUserId)
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
            xmlTrigger.Action = ToXmlModel(ieTrigger.Action, dataMaps, currentUserId);

            return xmlTrigger;
        }

        public IEnumerable<IeTrigger> FromXmlModel(XmlWorkflowEventTriggers xmlTriggers,
            IDictionary<string, int> artifactTypeMap, 
            IDictionary<string, int> propertyTypeMap, 
            IDictionary<string, int> groupMap, 
            IDictionary<string, int> stateMap)
        {
            if(xmlTriggers == null)
            {
                throw new ArgumentNullException(nameof(xmlTriggers));
            }

            var triggers = new List<IeTrigger>();

            if (xmlTriggers != null)
            {
                foreach (var t in xmlTriggers.Triggers)
                {
                    var trigger = FromXmlModel(t, artifactTypeMap, propertyTypeMap, groupMap, stateMap);
                    if (trigger != null)
                    {
                        triggers.Add(trigger);
                    }
                }
            }

            return triggers;
        }

        public IeTrigger FromXmlModel(XmlWorkflowEventTrigger xmlTrigger, 
            IDictionary<string, int> artifactTypeMap, 
            IDictionary<string, int> propertyTypeMap, 
            IDictionary<string, int> groupMap, 
            IDictionary<string, int> stateMap)
        {
            if(xmlTrigger == null)
            {
                throw new ArgumentNullException(nameof(xmlTrigger));
            }

            IeTrigger ieTrigger = new IeTrigger
            {
                Name = xmlTrigger.Name,
                Action = FromXmlModel(xmlTrigger.Action, artifactTypeMap, propertyTypeMap, groupMap),
                Condition = FromXmlModel(xmlTrigger.Condition, stateMap)
            };

            return ieTrigger;
        }

        #endregion

        #region Private Methods

        private IeBaseAction FromXmlModel(XmlAction xmlAction, 
            IDictionary<string, int> artifactTypeMap, 
            IDictionary<string, int> propertyTypeMap, 
            IDictionary<string, int> groupMap)
        {
            if(xmlAction == null)
            {
                throw new ArgumentNullException(nameof(xmlAction));
            }

            IeBaseAction action = null;

            switch (xmlAction.ActionType)
            {
                case ActionTypes.EmailNotification:
                    var xeAction = xmlAction as XmlEmailNotificationAction;
                    action = new IeEmailNotificationAction
                    {
                        Name = xeAction.Name,
                        Emails = xeAction.Emails,
                        PropertyId = xeAction.PropertyTypeId,
                        PropertyName = propertyTypeMap.FirstOrDefault(x => x.Value == xeAction.PropertyTypeId).Key,
                        Message = xeAction.Message
                    };
                    break;
                case ActionTypes.PropertyChange:
                    var xpAction = xmlAction as XmlPropertyChangeAction;
                    action = new IePropertyChangeAction
                    {
                        Name = xpAction.Name,
                        PropertyId = xpAction.PropertyTypeId,
                        PropertyName = propertyTypeMap.FirstOrDefault(x => x.Value == xpAction.PropertyTypeId).Key,
                        PropertyValue = xpAction.PropertyValue,
                        ValidValues = xpAction.ValidValues.ConvertAll(x => new IeValidValue { Value = x.ToString(CultureInfo.InvariantCulture) }),
                        UsersGroups = FromXmlModel(xpAction.UsersGroups, groupMap),
                        IncludeCurrentUser = xpAction.CurrentUserId != null
                    };
                    break;
                case ActionTypes.Generate:
                    var xgAction = xmlAction as XmlGenerateAction;
                    action = new IeGenerateAction
                    {
                        Name = xgAction.Name,
                        ChildCount = xgAction.ChildCount,
                        ArtifactTypeId = xgAction.ArtifactTypeId,
                        ArtifactType = artifactTypeMap.FirstOrDefault(x => x.Value == xgAction.ArtifactTypeId).Key
                    };
                    break;
                default:
                    break;
            }

            if (action == null)
            {
                throw new ArgumentOutOfRangeException(nameof(xmlAction.ActionType));
            }

            return action;
        }

        private List<IeUserGroup> FromXmlModel(List<XmlUserGroup> xmlUserGroups, IDictionary<string, int> groupMap)
        {
            if(xmlUserGroups == null)
            {
                throw new ArgumentNullException(nameof(xmlUserGroups));
            }
            var userGroups = new List<IeUserGroup>();

            if (xmlUserGroups != null)
            {
                foreach (var g in xmlUserGroups)
                {
                    var group = new IeUserGroup
                    {
                        Id = g.Id,
                        Name = groupMap.FirstOrDefault(x => x.Value == g.Id).Key,
                        IsGroup = g.IsGroup
                    };
                }
            }

            return userGroups;
        }

        private IeCondition FromXmlModel(XmlCondition xmlCondition, IDictionary<string, int> stateMap)
        {
            if(xmlCondition == null)
            {
                throw new ArgumentNullException(nameof(xmlCondition));
            }

            switch (xmlCondition.ConditionType)
            {
                case ConditionTypes.State:
                    var ieCondition = new IeStateCondition
                    {
                        State = stateMap.FirstOrDefault(x => x.Value == (xmlCondition as XmlStateCondition).StateId).Key
                    };
                    return ieCondition;
                default:
                    throw new ArgumentOutOfRangeException(nameof(xmlCondition.ConditionType));
            }
        }

        private XmlAction ToXmlModel(IeBaseAction ieAction, WorkflowDataMaps dataMaps, int currentUserId)
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
                    return ToXmlModel(ieAction as IePropertyChangeAction, dataMaps, currentUserId);
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

        private static XmlPropertyChangeAction ToXmlModel(IePropertyChangeAction ieAction, WorkflowDataMaps dataMaps, int currentUserId)
        {
            if (ieAction == null)
            {
                return null;
            }

            var xmlAction = new XmlPropertyChangeAction
            {
                Name = ieAction.Name,
                PropertyValue = ieAction.PropertyValue,
                CurrentUserId = ieAction.IncludeCurrentUser.GetValueOrDefault() ? currentUserId : (int?) null,
                UsersGroups = !ieAction.UsersGroups.IsEmpty() ? new List<XmlUserGroup>() : null
            };

            int propertyTypeId;
            if (!dataMaps.PropertyTypeMap.TryGetValue(ieAction.PropertyName, out propertyTypeId))
            {
                throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.", ieAction.PropertyName),
                    ErrorCodes.UnexpectedError);
            }
            xmlAction.PropertyTypeId = propertyTypeId;

            ieAction.UsersGroups?.ForEach(ug =>
            {
                var isGroup = ug.IsGroup.GetValueOrDefault();
                var ugMap = isGroup ? dataMaps.GroupMap : dataMaps.UserMap;
                int ugId;
                if (!ugMap.TryGetValue(ug.Name, out ugId))
                {
                    throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of {0} '{1}' is not found.",
                        isGroup ? "group" : "user", ug.Name), ErrorCodes.UnexpectedError);
                }

                xmlAction.UsersGroups.Add(new XmlUserGroup
                {
                    IsGroup = isGroup,
                    Id = ugId
                });
            });

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
            switch(ieAction.GenerateActionType)
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

        private XmlStateCondition ToXmlModel(IeCondition ieCondition, IDictionary<string, int> stateMap)
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