using System;
using System.Collections.Generic;
using System.Globalization;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow
{
    [TestClass]
    public class WorkflowActionPropertyValueValidatorTests
    {
        #region Text

        [TestMethod]
        public void ValidatePropertyValue_Text_Required_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Text,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "value"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true,null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Text_Required_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Text,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        #endregion

        #region Date

        [TestMethod]
        public void ValidatePropertyValue_Date_Required_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "2017-07-21"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_Required_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_InvalidFormat_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true,
                IsValidated = false
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "2017-07-21T15:00:00Z"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_NotRequiredEmpty_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = false,
                IsValidated = false
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = " "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_InRange_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true,
                IsValidated = true,
                MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "2017-08-30"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_LessThanMin_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true,
                IsValidated = true,
                MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "2017-07-31"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange);
        }

        [TestMethod]
        public void ValidatePropertyValue_Date_GreaterThanMax_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Date,
                IsRequired = true,
                IsValidated = true,
                MinDate = DateTime.ParseExact("2017-08-01", "yyyy-MM-dd", CultureInfo.InvariantCulture),
                MaxDate = DateTime.ParseExact("2017-08-30", "yyyy-MM-dd", CultureInfo.InvariantCulture)
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "2017-08-31"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange);
        }

        #endregion

        #region Number

        [TestMethod]
        public void ValidatePropertyValue_Number_Required_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "11.22"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_Required_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_InvalidFormat_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true,
                IsValidated = false
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "aaa"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_NotRequiredEmpty_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = false,
                IsValidated = false
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = " "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_InvalidDecimalPlaces_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true,
                IsValidated = true,
                DecimalPlaces = 2,
                MinNumber = -10.10m,
                MaxNumber = 20.20m
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "11.222"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_InRange_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true,
                IsValidated = true,
                DecimalPlaces = 2,
                MinNumber = -10.10m,
                MaxNumber = 20.20m
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "20.20"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_LessThanMin_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true,
                IsValidated = true,
                DecimalPlaces = 2,
                MinNumber = -10.10m,
                MaxNumber = 20.20m
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "-10.11"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange);
        }

        [TestMethod]
        public void ValidatePropertyValue_Number_GreaterThanMax_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Number,
                IsRequired = true,
                IsValidated = true,
                DecimalPlaces = 2,
                MinNumber = -10.10m,
                MaxNumber = 20.20m
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "20.21"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange);
        }

        #endregion

        #region Choice

        [TestMethod]
        public void ValidatePropertyValue_Choice_Required_Success()
        {
            //Arrange
            const string valueValue = "valid value";
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Choice,
                IsRequired = true,
                IsValidated = true,
                ValidValues = new List<ValidValue>
                {
                    new ValidValue { Id = 11, Value = valueValue }
                }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = valueValue }
                }

            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_Required_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Choice,
                IsRequired = true,
                IsValidated = false
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidatedSpecifiedAsNotValidated_Failure()
        {
            //Arrange
            const string valueValue = "valid value";
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Choice,
                IsRequired = true,
                IsValidated = true,
                ValidValues = new List<ValidValue>
                {
                    new ValidValue { Value = valueValue }
                }
            };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "value"
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotFoundByValue_Failure()
        {
            //Arrange
            const string valueValue = "valid value";
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Choice,
                IsRequired = true,
                IsValidated = true,
                ValidValues = new List<ValidValue>
                {
                    new ValidValue { Id = 22, Value = valueValue }
                }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = "invalid valid value" }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundByValue);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotFoundById_Failure()
        {
            //Arrange
            const string valueValue = "valid value";
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.Choice,
                IsRequired = true,
                IsValidated = true,
                ValidValues = new List<ValidValue>
                {
                    new ValidValue { Id = 22, Value = valueValue }
                }
            };
            var action = new IePropertyChangeAction
            {
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Id = 33, Value = "invalid valid value" }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, false, false, WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFoundById);
        }

        #endregion

        #region User

        [TestMethod]
        public void ValidatePropertyValue_User_Required_Success()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            const string user = "user";
            var users = new List<SqlUser> { new SqlUser { Login = user } };
            var groups = new List<SqlGroup>();
            var action = new IePropertyChangeAction
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup { Name = user }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, groups, true, true, null);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_Required_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            const string user = "user";
            var users = new List<SqlUser> { new SqlUser { Login = user } };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, null, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserNotFoundByName_Failure()
        {
            //Arrange

            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var users = new List<SqlUser>();
            var groups = new List<SqlGroup>();
            var action = new IePropertyChangeAction
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup
                    {
                        Name = "user",
                        IsGroup = false
                    }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, groups, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundByName);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserNotFoundById_Failure()
        {
            //Arrange

            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var users = new List<SqlUser>();
            var groups = new List<SqlGroup>();
            var action = new IePropertyChangeAction
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
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, groups, false, false, WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFoundById);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_GroupNotFoundByName_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var group = Tuple.Create("user", (int?) 99);
            var groups = new List<SqlGroup>();
            var users = new List<SqlUser>();
            var action = new IePropertyChangeAction
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
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, groups, true, false, WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundByName);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_GroupNotFoundById_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var group = Tuple.Create("user", (int?)99);
            var groups = new List<SqlGroup>();
            var users = new List<SqlUser>();
            var action = new IePropertyChangeAction
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
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, users, groups, false, false, WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFoundById);
        }

        #endregion

        #region Private methods

        private static void ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds,bool expectedResult,
            WorkflowDataValidationErrorCodes? expectedErrorCode)
        {
            //Arrange
            var pvValidator = new WorkflowActionPropertyValueValidator();

            //Act
            WorkflowDataValidationErrorCodes? actualErrorCode;
            var actualResult = pvValidator.ValidatePropertyValue(action, propertyType, users, groups, ignoreIds, out actualErrorCode);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expectedErrorCode, actualErrorCode);
        }

        #endregion

    }
}
