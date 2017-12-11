using System;
using System.Collections.Generic;
using System.Linq;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    [TestClass]
    public class UserPropertyValueValidatorTests
    {
        private WorkflowDataValidationResult _result;
        private PropertyType _propertyType;
        private List<SqlUser> _users;
        private List<SqlGroup> _groups;

        [TestInitialize]
        public void Initialize()
        {
            _result = new WorkflowDataValidationResult();
            _propertyType = new PropertyType { PrimitiveType = PropertyPrimitiveType.User };
            _users = new List<SqlUser>();
            _groups = new List<SqlGroup>();
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UsersGroups_Required_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            const string user = "user";
            _users.Add(new SqlUser { Login = user });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = user }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_Required_IncludeCurrentUser_Success()
        {
            // Arrange
            _propertyType.IsRequired = true;
            const string user = "user";
            _users.Add(new SqlUser { Login = user });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    IncludeCurrentUser = true
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_HasValidValues_Failure()
        {
            // Arrange
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue()
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionNotChoicePropertyValidValuesNotApplicable, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_RequiredHasPropertyValue_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction { PropertyValue = "" };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredUserPropertyPropertyValueNotApplicable, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_Required_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            const string user = "user";
            _users.Add(new SqlUser { Login = user });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups()
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserNotFoundByName_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup
                        {
                            Name = "user",
                            IsGroup = false
                        }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserNotFoundById_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup
                        {
                            Id = 22,
                            Name = "user",
                            IsGroup = false
                        }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_GroupNotFoundByName_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var group = Tuple.Create("user", (int?)99);
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup
                        {
                            Id = 22,
                            Name = group.Item1,
                            GroupProjectId = group.Item2,
                            IsGroup = true
                        }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_GroupNotFoundById_Failure()
        {
            // Arrange
            _propertyType.IsRequired = true;
            var group = Tuple.Create("user", (int?)99);
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup
                        {
                            Id = 22,
                            Name = group.Item1,
                            GroupProjectId = group.Item2,
                            IsGroup = true
                        }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_UniqueUsers_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _users.Add(new SqlUser { Login = "value2", UserId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1" },
                        new IeUserGroup { Name = "value2" }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_UniqueGroups_Success()
        {
            // Arrange
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1", IsGroup = true },
                        new IeUserGroup { Name = "value2", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_UniqueUsersAndGroups_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 1 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1" },
                        new IeUserGroup { Name = "value2", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_DuplicateUsers_Failure()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _users.Add(new SqlUser { Login = "value2", UserId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1" },
                        new IeUserGroup { Name = "value1" }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateUserOrGroupFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_DuplicateGroups_Failure()
        {
            // Arrange
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1", IsGroup = true },
                        new IeUserGroup { Name = "value1", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateUserOrGroupFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_DuplicateUsersAndGroups_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Name = "value1" },
                        new IeUserGroup { Name = "value1", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, true);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_UniqueUsers_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _users.Add(new SqlUser { Login = "value2", UserId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1" },
                        new IeUserGroup { Id = 2, Name = "value2" }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_UniqueGroups_Success()
        {
            // Arrange
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1", IsGroup = true },
                        new IeUserGroup { Id = 2, Name = "value2", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_UniqueGroupsWithSameName_Success()
        {
            // Arrange
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1", IsGroup = true },
                        new IeUserGroup { Id = 2, Name = "value1", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_UniqueUsersAndGroups_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1" },
                        new IeUserGroup { Id = 2, Name = "value2", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_DuplicateUsers_Failure()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _users.Add(new SqlUser { Login = "value2", UserId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1" },
                        new IeUserGroup { Id = 1, Name = "value1" }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateUserOrGroupFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_DuplicateGroups_Failure()
        {
            // Arrange
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            _groups.Add(new SqlGroup { Name = "value2", GroupId = 2 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1", IsGroup = true },
                        new IeUserGroup { Id = 1, Name = "value1", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsTrue(_result.HasErrors);
            Assert.AreEqual(WorkflowDataValidationErrorCodes.PropertyChangeActionDuplicateUserOrGroupFound, _result.Errors.Single().ErrorCode);
        }

        [TestMethod]
        public void ValidatePropertyValue_WithIds_DuplicateUsersAndGroups_Success()
        {
            // Arrange
            _users.Add(new SqlUser { Login = "value1", UserId = 1 });
            _groups.Add(new SqlGroup { Name = "value1", GroupId = 1 });
            var action = new IePropertyChangeAction
            {
                UsersGroups = new IeUsersGroups
                {
                    UsersGroups = new List<IeUserGroup>
                    {
                        new IeUserGroup { Id = 1, Name = "value1" },
                        new IeUserGroup { Id = 1, Name = "value1", IsGroup = true }
                    }
                }
            };

            // Act
            Validate(action, false);

            // Assert
            Assert.IsFalse(_result.HasErrors);
        }

        private void Validate(IePropertyChangeAction action, bool ignoreIds)
        {
            var factory = new PropertyValueValidatorFactory();
            var validator = factory.Create(_propertyType, _users, _groups, ignoreIds);
            validator.Validate(action, _propertyType, _result);
        }
    }
}
