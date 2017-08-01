﻿using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ArtifactStore.Helpers;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.Enums;
using ServiceLibrary.Models.Workflow;

namespace AdminStore.Services.Workflow
{
    public class TriggerConverter : ITriggerConverter
    {
        #region Interface Implementation

        public XmlWorkflowEventTriggers ToXmlModel(IEnumerable<IeTrigger> ieTriggers, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap,
            IDictionary<string, int> userMap, IDictionary<string, int> groupMap, int currentUserId)
        {
            var xmlTriggers = new XmlWorkflowEventTriggers();
            ieTriggers?.ForEach(t => xmlTriggers.Triggers.Add(ToXmlModel(t, artifactTypeMap, propertyTypeMap, stateMap,
                userMap, groupMap, currentUserId)));
            return xmlTriggers;
        }

        public XmlWorkflowEventTrigger ToXmlModel(IeTrigger ieTrigger, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> stateMap,
            IDictionary<string, int> userMap, IDictionary<string, int> groupMap, int currentUserId)
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
                xmlTrigger.Condition = ToXmlModel(ieTrigger.Condition, stateMap);
            }

            // Triggers must have an action.
            xmlTrigger.Action = ToXmlModel(ieTrigger.Action, artifactTypeMap, propertyTypeMap, userMap, groupMap, currentUserId);

            return xmlTrigger;
        }

        #endregion

        #region Private Methods

        private XmlAction ToXmlModel(IeBaseAction ieAction, IDictionary<string, int> artifactTypeMap,
            IDictionary<string, int> propertyTypeMap, IDictionary<string, int> userMap, IDictionary<string, int> groupMap,
            int currentUserId)
        {
            if (ieAction == null)
            {
                throw new ArgumentNullException(nameof(ieAction));
            }

            switch (ieAction.ActionType)
            {
                case ActionTypes.EmailNotification:
                    return ToXmlModel(ieAction as IeEmailNotificationAction, propertyTypeMap);
                case ActionTypes.PropertyChange:
                    return ToXmlModel(ieAction as IePropertyChangeAction, propertyTypeMap,
                        userMap, groupMap, currentUserId);
                case ActionTypes.Generate:
                    return ToXmlModel(ieAction as IeGenerateAction, artifactTypeMap);
                default:
                    throw new ArgumentOutOfRangeException(nameof(ieAction.ActionType));
            }
        }

        private static XmlEmailNotificationAction ToXmlModel(IeEmailNotificationAction ieAction, IDictionary<string, int> propertyTypeMap)
        {
            var xmlAction = new XmlEmailNotificationAction
            {
                Name = ieAction.Name,
                Emails = ieAction.Emails,
                Message = ieAction.Message
            };

            int propertyTypeId;
            if (!propertyTypeMap.TryGetValue(ieAction.PropertyName, out propertyTypeId))
            {
                throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.", ieAction.PropertyName),
                    ErrorCodes.UnexpectedError);
            }
            xmlAction.PropertyTypeId = propertyTypeId;
            return xmlAction;
        }

        private static XmlPropertyChangeAction ToXmlModel(IePropertyChangeAction ieAction, IDictionary<string, int> propertyTypeMap,
            IDictionary<string, int> userMap, IDictionary<string, int> groupMap, int currentUserId)
        {
            var xmlAction = new XmlPropertyChangeAction
            {
                Name = ieAction.Name,
                PropertyValue = ieAction.PropertyValue,
                ValidValues = ieAction.ValidValues,
                CurrentUserId = currentUserId,
                UsersGroups = ieAction.UsersGroups != null && ieAction.UsersGroups.Any()
                    ? new List<XmlUserGroup>() : null
            };

            int propertyTypeId;
            if (!propertyTypeMap.TryGetValue(ieAction.PropertyName, out propertyTypeId))
            {
                throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of Standard Property Type '{0}' is not found.", ieAction.PropertyName),
                    ErrorCodes.UnexpectedError);
            }
            xmlAction.PropertyTypeId = propertyTypeId;

            ieAction.UsersGroups?.ForEach(ug =>
            {
                var isGroup = ug.IsGroup.GetValueOrDefault();
                var ugMap = isGroup ? groupMap : userMap;
                int ugId;
                if (!ugMap.TryGetValue(ug.Name, out ugId))
                {
                    throw new ExceptionWithErrorCode(I18NHelper.FormatInvariant("Id of {0} '{1}' is not found.",
                        isGroup ? "group" : "user", ieAction.PropertyName), ErrorCodes.UnexpectedError);
                }

                xmlAction.UsersGroups.Add(new XmlUserGroup
                {
                    IsGroup = isGroup,
                    Id = ugId
                });
            });

            return xmlAction;
        }

        private static XmlGenerateAction ToXmlModel(IeGenerateAction ieAction, IDictionary<string, int> artifactTypeMap)
        {
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
                throw new ArgumentNullException(nameof(ieCondition));
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