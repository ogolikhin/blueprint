using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class UserPropertyValueValidator : PropertyValueValidator
    {
        private readonly IList<SqlUser> _users;
        private readonly IList<SqlGroup> _groups;
        private readonly bool _ignoreIds;

        public UserPropertyValueValidator(IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds)
        {
            _users = users;
            _groups = groups;
            _ignoreIds = ignoreIds;
        }

        public override void Validate(IePropertyChangeAction action, PropertyType propertyType, WorkflowDataValidationResult result)
        {
            if (!action.ValidValues.IsEmpty())
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable
                });
                return;
            }

            if (action.PropertyValue != null)
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable
                });
                return;
            }

            var usersGroups = action.UsersGroups?.UsersGroups;
            var isUsersGroupsEmpty = action.UsersGroups == null || usersGroups.IsEmpty() && !action.UsersGroups.IncludeCurrentUser.GetValueOrDefault();

            if (!IsPropertyRequired(propertyType.IsRequired.GetValueOrDefault(), null, true, isUsersGroupsEmpty))
            {
                result.Errors.Add(new WorkflowDataValidationError
                {
                    Element = action.PropertyName,
                    ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty
                });
                return;
            }

            if (usersGroups.IsEmpty())
            {
                return;
            }

            var usersMap = _users.ToDictionary(u => u.UserId, u => u.Login);
            var groupsMap = _groups.ToDictionary(g => g.GroupId, g => Tuple.Create(g.Name, g.ProjectId));
            var userNames = usersMap.Values.ToHashSet();
            var groupNames = groupsMap.Values.ToHashSet();

            if (usersGroups == null || !usersGroups.Any())
            {
                return;
            }

            foreach (var userGroup in usersGroups)
            {
                if (userGroup.IsGroup.GetValueOrDefault())
                {
                    // Update Name where Id is present (to null if Id is not found)
                    if (!_ignoreIds && userGroup.Id.HasValue)
                    {
                        Tuple<string, int?> nameProject;
                        if (!groupsMap.TryGetValue(userGroup.Id.Value, out nameProject))
                        {
                            result.Errors.Add(new WorkflowDataValidationError
                            {
                                Element = action.PropertyName,
                                ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById
                            });
                            return;
                        }

                        userGroup.Name = nameProject.Item1;
                    }

                    if (!groupNames.Contains(Tuple.Create(userGroup.Name, userGroup.GroupProjectId)))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName
                        });
                        return;
                    }
                }
                else
                {
                    // Update Name where Id is present (to null if Id is not found)
                    if (!_ignoreIds && userGroup.Id.HasValue)
                    {
                        string name;
                        if (!usersMap.TryGetValue(userGroup.Id.Value, out name))
                        {
                            result.Errors.Add(new WorkflowDataValidationError
                            {
                                Element = action.PropertyName,
                                ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById
                            });
                            return;
                        }

                        userGroup.Name = name;
                    }

                    if (!userNames.Contains(userGroup.Name))
                    {
                        result.Errors.Add(new WorkflowDataValidationError
                        {
                            Element = action.PropertyName,
                            ErrorCode = WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName
                        });
                        return;
                    }
                }
            }
        }
    }
}
