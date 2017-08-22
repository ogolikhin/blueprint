using System;
using System.Collections.Generic;
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

        public IEnumerable<IeTrigger> FromXmlModel(XmlWorkflowEventTriggers xmlTriggers, WorkflowDataNameMaps dataMaps)
        {
            if (xmlTriggers == null)
            {
                return null;
            }

            var triggers = new List<IeTrigger>();

            if (xmlTriggers != null)
            {
                foreach (var t in xmlTriggers.Triggers)
                {
                    var trigger = FromXmlModel(t, dataMaps);
                    if (trigger != null)
                    {
                        triggers.Add(trigger);
                    }
                }
            }

            return triggers;
        }

        public IeTrigger FromXmlModel(XmlWorkflowEventTrigger xmlTrigger, WorkflowDataNameMaps dataMaps)
        {
            if (xmlTrigger == null)
            {
                return null;
            }

            IeTrigger ieTrigger = new IeTrigger
            {
                Name = xmlTrigger.Name,
                Action = FromXmlModel(xmlTrigger.Action, dataMaps),
                Condition = FromXmlModel(xmlTrigger.Condition, dataMaps)
            };

            return ieTrigger;
        }

        #endregion

        #region Private Methods

        private IeBaseAction FromXmlModel(XmlAction xmlAction, WorkflowDataNameMaps dataMaps)
        {
            if (xmlAction == null)
            {
                return null;
            }

            string name;
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
                        PropertyName = xeAction.PropertyTypeId == null ? null : 
                            dataMaps.PropertyTypeMap.TryGetValue((int)xeAction.PropertyTypeId, out name) ? name : null,
                        Message = xeAction.Message
                    };
                    break;
                case ActionTypes.PropertyChange:
                    var xpAction = xmlAction as XmlPropertyChangeAction;
                    action = new IePropertyChangeAction
                    {
                        Name = xpAction.Name,
                        PropertyId = xpAction.PropertyTypeId,
                        PropertyName = dataMaps.PropertyTypeMap.TryGetValue(xpAction.PropertyTypeId, out name) ? name : null,
                        PropertyValue = xpAction.PropertyValue,
                        ValidValues = GetValidValues(xpAction.ValidValues, dataMaps),
                        UsersGroups = FromXmlModel(xpAction.UsersGroups, dataMaps),
                        IncludeCurrentUser = xpAction.CurrentUserId != null
                    };
                    break;
                case ActionTypes.Generate:
                    var xgAction = xmlAction as XmlGenerateAction;
                    action = new IeGenerateAction
                    {
                        Name = xgAction.Name,
                        GenerateActionType = xgAction.GenerateActionType,
                        ChildCount = xgAction.ChildCount,
                        ArtifactTypeId = xgAction.ArtifactTypeId,
                        ArtifactType = GetArtifactType(xgAction.ArtifactTypeId, dataMaps)
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

        private List<IeValidValue> GetValidValues(List<int> valueIds, WorkflowDataNameMaps dataMaps)
        {
            var values = valueIds.ConvertAll(x => new IeValidValue { Id = x });
            foreach(var v in values)
            {
                string vv = null;
                v.Value = dataMaps.ValidValueMap.TryGetValue((int)v.Id, out vv) ? vv : null;
            }
            return values.Count == 0 ? null : values;
        }
        private string GetArtifactType(int? xArtifactTypeId, WorkflowDataNameMaps dataMaps)
        {
            
            string artifactType = null;
            if (xArtifactTypeId != null)
            {
                string name = null;
                artifactType = dataMaps.ArtifactTypeMap.TryGetValue((int)xArtifactTypeId, out name) ? name : null;
            }

            return artifactType;
        }
        private List<IeUserGroup> FromXmlModel(List<XmlUserGroup> xmlUserGroups, WorkflowDataNameMaps dataMaps)
        {
            if (xmlUserGroups == null || xmlUserGroups.Count == 0)
            {
                return null;
            }
            string name;
            var userGroups = new List<IeUserGroup>();
            foreach (var g in xmlUserGroups)
            {
                var map = g.IsGroup.HasValue && (bool)g.IsGroup ? dataMaps.GroupMap : dataMaps.UserMap;
               
                var group = new IeUserGroup
                {
                    Id = g.Id,
                    Name = map.TryGetValue(g.Id, out name) ? name : null,
                    IsGroup = g.IsGroup
                };
                userGroups.Add(group);
            }

            return userGroups; 
        }

        private IeCondition FromXmlModel(XmlCondition xmlCondition, WorkflowDataNameMaps dataMaps)
        {
            if (xmlCondition == null)
            {
                return null;
            }

            switch (xmlCondition.ConditionType)
            {
                case ConditionTypes.State:
                    string name;
                    int stateId = (xmlCondition as XmlStateCondition).StateId;
                    var ieCondition = new IeStateCondition
                    {
                        StateId = stateId,
                        State = dataMaps.StateMap.TryGetValue(stateId, out name) ? name : null
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