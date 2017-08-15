using System;
using System.Collections.Generic;
using System.Globalization;
using AdminStore.Models.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidDateFormat);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionDateOutOfRange);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberFormat);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionInvalidNumberDecimalPlaces);
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionNumberOutOfRange);
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
                    new ValidValue { Value = valueValue }
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
            ValidatePropertyValue(action, propertyType, null, null, true, null);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionChoiceValueSpecifiedAsNotValidated);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotSpecified_Failure()
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
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = " " }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotSpecified);
        }

        [TestMethod]
        public void ValidatePropertyValue_Choice_ValidValueNotFound_Failure()
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
                ValidValues = new List<IeValidValue>
                {
                    new IeValidValue { Value = "invalid valid value" }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionValidValueNotFound);
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
            var validUsers = new HashSet<string> { user };
            var action = new IePropertyChangeAction
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup { Name = user }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, validUsers, null, true, null);
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
            var validUsers = new HashSet<string> { user };
            var action = new IePropertyChangeAction
            {
                PropertyValue = "  "
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, validUsers, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionRequiredPropertyValueEmpty);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserOrGroupNotSpecified_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            const string user = "user";
            var validUsers = new HashSet<string> { user };
            var action = new IePropertyChangeAction
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup
                    {
                        Name = " ",
                        IsGroup = false
                    }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, validUsers, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionUserOrGroupNotSpecified);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_UserNotFound_Failure()
        {
            //Arrange

            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var validUsers = new HashSet<string>();
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
            ValidatePropertyValue(action, propertyType, validUsers, null, false, WorkflowDataValidationErrorCodes.PropertyChangeActionUserNotFound);
        }

        [TestMethod]
        public void ValidatePropertyValue_User_GroupNotFound_Failure()
        {
            //Arrange
            var propertyType = new PropertyType
            {
                PrimitiveType = PropertyPrimitiveType.User,
                IsRequired = true
            };
            var group = Tuple.Create("user", (int?)99);
            var validGroups = new HashSet<Tuple<string, int?>>();
            var action = new IePropertyChangeAction
            {
                UsersGroups = new List<IeUserGroup>
                {
                    new IeUserGroup
                    {
                        Name = group.Item1,
                        GroupProjectId = group.Item2,
                        IsGroup = true
                    }
                }
            };

            //Act and Assert
            ValidatePropertyValue(action, propertyType, null, validGroups, false, WorkflowDataValidationErrorCodes.PropertyChangeActionGroupNotFound);
        }

        #endregion

        #region Private methods

        private static void ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<Tuple<string, int?>> validGroups, bool expectedResult,
            WorkflowDataValidationErrorCodes? expectedErrorCode)
        {
            //Arrange
            var pvValidator = new WorkflowActionPropertyValueValidator();

            //Act
            WorkflowDataValidationErrorCodes? actualErrorCode;
            var actualResult = pvValidator.ValidatePropertyValue(action, propertyType, validUsers, validGroups, out actualErrorCode);

            //Assert
            Assert.AreEqual(expectedResult, actualResult);
            Assert.AreEqual(expectedErrorCode, actualErrorCode);
        }

        #endregion

    }
}
