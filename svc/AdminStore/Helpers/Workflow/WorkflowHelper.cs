using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.Workflow;
using AdminStore.Models.DiagramWorkflow;
using AdminStore.Models.Enums;
using ServiceLibrary.Models.Enums;

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
            { WorkflowConstants.PropertyTypeFakeIdName, WorkflowConstants.PropertyNameName },
            { WorkflowConstants.PropertyTypeFakeIdDescription, WorkflowConstants.PropertyNameDescription }
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

        public static DWorkflow MapIeWorkflowToDWorkflow(IeWorkflow sourceWorkflow)
        {
            var destinationWorkflow = new DWorkflow();
            destinationWorkflow.Id = sourceWorkflow.Id;
            destinationWorkflow.Name = sourceWorkflow.Name;
            destinationWorkflow.Description = sourceWorkflow.Description;
            destinationWorkflow.IsActive = sourceWorkflow.IsActive;
            destinationWorkflow.IsContainsProcessArtifactType = sourceWorkflow.IsContainsProcessArtifactType;
            destinationWorkflow.NewArtifactEvents = sourceWorkflow.NewArtifactEvents?.Select(ieNewArtifactEvent =>
                new DNewArtifactEvent
                {
                    Id = ieNewArtifactEvent.Id,
                    Name = ieNewArtifactEvent.Name,
                    Triggers = MapToDTriggers(ieNewArtifactEvent.Triggers)
                }).ToList();

            destinationWorkflow.Projects = sourceWorkflow.Projects?.Select(ieProject => new DProject
            {
                Id = ieProject.Id,
                ArtifactTypes = ieProject.ArtifactTypes?.Select(ieArtifactType => new DArtifactType
                {
                    Id = ieArtifactType.Id,
                    Name = ieArtifactType.Name
                })
            }).ToList();

            destinationWorkflow.PropertyChangeEvents = sourceWorkflow.PropertyChangeEvents?.Select(
                iePropertyChangeEvent => new DPropertyChangeEvent
                {
                    Id = iePropertyChangeEvent.Id,
                    Name = iePropertyChangeEvent.Name,
                    PropertyId = iePropertyChangeEvent.PropertyId,
                    PropertyName = iePropertyChangeEvent.PropertyName,
                    Triggers = MapToDTriggers(iePropertyChangeEvent.Triggers.ToList())
                }).ToList();

            destinationWorkflow.States = sourceWorkflow.States?.Select(ieState => new DState
                {
                    Id = ieState.Id,
                    Name = ieState.Name,
                    IsInitial = ieState.IsInitial,
                    Location = ieState.Location
                })
                .ToList();

            destinationWorkflow.TransitionEvents = sourceWorkflow.TransitionEvents?.Select(ieTransitionEvent =>
                new DTransitionEvent
                {
                    Id = ieTransitionEvent.Id,
                    Name = ieTransitionEvent.Name,
                    FromState = ieTransitionEvent.FromState,
                    FromStateId = ieTransitionEvent.FromStateId,
                    ToState = ieTransitionEvent.ToState,
                    ToStateId = ieTransitionEvent.ToStateId,
                    SkipPermissionGroups = ieTransitionEvent.SkipPermissionGroups,
                    PortPair = ieTransitionEvent.PortPair != null
                        ? new DPortPair
                        {
                            FromPort = ieTransitionEvent.PortPair.FromPort,
                            ToPort = ieTransitionEvent.PortPair.ToPort
                        }
                        : null,
                    Triggers = MapToDTriggers(ieTransitionEvent.Triggers),
                    PermissionGroups = ieTransitionEvent.PermissionGroups?.Select(ieGroup => new DGroup
                        {
                            Id = ieGroup.Id,
                            Name = ieGroup.Name
                        })
                        .ToList()
                }).ToList();

            return destinationWorkflow;
        }

        public static IeWorkflow MapDWorkflowToIeWorkflow(DWorkflow sourceWorkflow)
        {
            var destinationWorkflow = new IeWorkflow
            {
                Id = sourceWorkflow.Id,
                Name = sourceWorkflow.Name,
                Description = sourceWorkflow.Description,
                IsActive = sourceWorkflow.IsActive,
                IsContainsProcessArtifactType = sourceWorkflow.IsContainsProcessArtifactType,
                NewArtifactEvents = sourceWorkflow.NewArtifactEvents?.Select(dNewArtifactEvent => new IeNewArtifactEvent
                    {
                        Id = dNewArtifactEvent.Id,
                        Name = dNewArtifactEvent.Name,
                        Triggers = MapToIeTriggers(dNewArtifactEvent.Triggers)
                    })
                    .ToList() ?? new List<IeNewArtifactEvent>(),
                Projects = sourceWorkflow.Projects?.Select(dProject => new IeProject
                    {
                        Id = dProject.Id,
                        Path = dProject.Path,
                        ArtifactTypes = dProject.ArtifactTypes
                            ?.Select(
                                dArtifactType => new IeArtifactType
                                {
                                    Id = dArtifactType.Id,
                                    Name = dArtifactType.Name
                                })
                            .ToList() ?? new List<IeArtifactType>()
                })
                    .ToList() ?? new List<IeProject>(),
                PropertyChangeEvents = sourceWorkflow.PropertyChangeEvents?.Select(dPropertyChangeEvent => new IePropertyChangeEvent
                    {
                        Id = dPropertyChangeEvent.Id,
                        Name = dPropertyChangeEvent.Name,
                        PropertyId = dPropertyChangeEvent.PropertyId,
                        PropertyName = dPropertyChangeEvent.PropertyName,
                        Triggers = MapToIeTriggers(dPropertyChangeEvent.Triggers)
                    })
                    .ToList() ?? new List<IePropertyChangeEvent>(),
                States = sourceWorkflow.States?.Select(dState => new IeState
                    {
                        Id = dState.Id,
                        Name = dState.Name,
                        IsInitial = dState.IsInitial,
                        Location = dState.Location
                    })
                    .ToList() ?? new List<IeState>(),
                TransitionEvents = sourceWorkflow.TransitionEvents?.Select(dTransitionEvent => new IeTransitionEvent
                    {
                        Id = dTransitionEvent.Id,
                        Name = dTransitionEvent.Name,
                        FromState = dTransitionEvent.FromState,
                        FromStateId = dTransitionEvent.FromStateId,
                        ToState = dTransitionEvent.ToState,
                        ToStateId = dTransitionEvent.ToStateId,
                        SkipPermissionGroups = dTransitionEvent.SkipPermissionGroups,
                        PortPair = dTransitionEvent.PortPair != null
                            ? new IePortPair
                            {
                                FromPort = Enum.IsDefined(typeof(DiagramPort), dTransitionEvent.PortPair.FromPort) ? dTransitionEvent.PortPair.FromPort : DiagramPort.None,
                                ToPort = Enum.IsDefined(typeof(DiagramPort), dTransitionEvent.PortPair.ToPort) ? dTransitionEvent.PortPair.ToPort : DiagramPort.None
                            }
                            : null,
                        Triggers = MapToIeTriggers(dTransitionEvent.Triggers),
                        PermissionGroups = dTransitionEvent.PermissionGroups?.Select(dGroup => new IeGroup
                            {
                                Id = dGroup.Id,
                                Name = dGroup.Name
                            })
                            .ToList() ?? new List<IeGroup>()
                })
                    .ToList() ?? new List<IeTransitionEvent>()
            };

            return destinationWorkflow;
        }

        private static List<IeTrigger> MapToIeTriggers(IEnumerable<DTrigger> dTriggers)
        {
            return dTriggers?.Select(dTrigger => new IeTrigger
                {
                    Name = dTrigger.Name,
                    Action = MapToIeAction(dTrigger.Action),
                    Condition = dTrigger.Condition != null
                        ? new IeStateCondition
                        {
                            StateId = dTrigger.Condition.StateId,
                            State = dTrigger.Condition.State
                        }
                        : null
                })
                .ToList() ?? new List<IeTrigger>();
        }

        private static List<DTrigger> MapToDTriggers(List<IeTrigger> ieTriggers)
        {
            return ieTriggers?.Select(ieTrigger => new DTrigger
                {
                    Name = ieTrigger.Name,
                    Action = MapToDAction(ieTrigger.Action),
                    Condition = ieTrigger.Condition is IeStateCondition
                        ? new DStateCondition
                        {
                            StateId = ((IeStateCondition)ieTrigger.Condition).StateId,
                            State = ((IeStateCondition)ieTrigger.Condition).State
                        }
                        : null
                }).ToList();
        }

        private static IeBaseAction MapToIeAction(DBaseAction dBaseAction)
        {
            switch (dBaseAction?.ActionType)
            {
                case ActionTypes.EmailNotification:
                    var dEmailNotificationAction = dBaseAction as DEmailNotificationAction;
                    if (dEmailNotificationAction != null)
                        return new IeEmailNotificationAction
                        {
                            Name = dEmailNotificationAction.Name,
                            Emails = dEmailNotificationAction.Emails?.ToList(),
                            Message = dEmailNotificationAction.Message,
                            PropertyId = dEmailNotificationAction.PropertyId,
                            PropertyName = dEmailNotificationAction.PropertyName
                        };
                    break;
                case ActionTypes.Generate:
                    var dGenerateAction = dBaseAction as DGenerateAction;
                    if (dGenerateAction != null)
                        return new IeGenerateAction
                        {
                            Name = dGenerateAction.Name,
                            ArtifactType = dGenerateAction.ArtifactType,
                            ArtifactTypeId = dGenerateAction.ArtifactTypeId,
                            ChildCount = dGenerateAction.ChildCount,
                            GenerateActionType = dGenerateAction.GenerateActionType
                        };
                    break;
                case ActionTypes.PropertyChange:
                    var dPropertyChangeAction = dBaseAction as DPropertyChangeAction;
                    if (dPropertyChangeAction != null)
                        return new IePropertyChangeAction
                        {
                            Name = dPropertyChangeAction.Name,
                            PropertyName = dPropertyChangeAction.PropertyName,
                            PropertyId = dPropertyChangeAction.PropertyId,
                            PropertyValue = dPropertyChangeAction.PropertyValue,
                            UsersGroups = dPropertyChangeAction.UsersGroups != null
                                ? new IeUsersGroups
                                {
                                    IncludeCurrentUser = dPropertyChangeAction.UsersGroups?.IncludeCurrentUser,
                                    UsersGroups = dPropertyChangeAction.UsersGroups?.UsersGroups?.Select(
                                                          dUserGroup => new IeUserGroup
                                                          {
                                                              Id = dUserGroup.Id,
                                                              Name = dUserGroup.Name,
                                                              GroupProjectId = dUserGroup.GroupProjectId,
                                                              GroupProjectPath = dUserGroup.GroupProjectPath,
                                                              IsGroup = dUserGroup.IsGroup
                                                          })
                                                      .ToList() ?? new List<IeUserGroup>()
                                }
                                : null,
                            ValidValues = dPropertyChangeAction.ValidValues?.Select(dValidValue => new IeValidValue
                                {
                                    Id = dValidValue.Id,
                                    Value = dValidValue.Value
                                })
                                .ToList()
                        };
                    break;
                default:
                    return null;
            }
            return null;
        }

        private static DBaseAction MapToDAction(IeBaseAction ieBaseAction)
        {
            switch (ieBaseAction.ActionType)
            {
                case ActionTypes.EmailNotification:
                    var ieEmailNotificationAction = ieBaseAction as IeEmailNotificationAction;
                    if (ieEmailNotificationAction != null)
                    {
                        return new DEmailNotificationAction
                        {
                            Name = ieEmailNotificationAction.Name,
                            Emails = ieEmailNotificationAction.Emails?.ToList(),
                            Message = ieEmailNotificationAction.Message,
                            PropertyId = ieEmailNotificationAction.PropertyId,
                            PropertyName = ieEmailNotificationAction.PropertyName
                        };
                    }
                    break;
                case ActionTypes.Generate:
                    var ieGenerateAction = ieBaseAction as IeGenerateAction;
                    if (ieGenerateAction != null)
                    {
                        return new DGenerateAction
                        {
                            Name = ieGenerateAction.Name,
                            ArtifactType = ieGenerateAction.ArtifactType,
                            ArtifactTypeId = ieGenerateAction.ArtifactTypeId,
                            ChildCount = ieGenerateAction.ChildCount,
                            GenerateActionType = ieGenerateAction.GenerateActionType
                        };
                    }
                    break;
                case ActionTypes.PropertyChange:
                    var iePropertyChangeAction = ieBaseAction as IePropertyChangeAction;
                    if (iePropertyChangeAction != null)
                    {
                        return new DPropertyChangeAction
                        {
                            Name = iePropertyChangeAction.Name,
                            PropertyName = iePropertyChangeAction.PropertyName,
                            PropertyId = iePropertyChangeAction.PropertyId,
                            PropertyValue = iePropertyChangeAction.PropertyValue,
                            UsersGroups = new DUsersGroups
                            {
                                IncludeCurrentUser = iePropertyChangeAction.UsersGroups?.IncludeCurrentUser,
                                UsersGroups = iePropertyChangeAction.UsersGroups?.UsersGroups?.Select(ieUserGroup =>
                                    new DUserGroup
                                    {
                                        Id = ieUserGroup.Id,
                                        Name = ieUserGroup.Name,
                                        GroupProjectId = ieUserGroup.GroupProjectId,
                                        GroupProjectPath = ieUserGroup.GroupProjectPath,
                                        IsGroup = ieUserGroup.IsGroup
                                    }).ToList()
                            },
                            ValidValues = iePropertyChangeAction.ValidValues?.Select(ieValidValue => new DValidValue
                            {
                                Id = ieValidValue.Id,
                                Value = ieValidValue.Value
                            }).ToList()
                        };
                    }
                    break;
                default:
                    return null;
            }
            return null;
        }
    }
}