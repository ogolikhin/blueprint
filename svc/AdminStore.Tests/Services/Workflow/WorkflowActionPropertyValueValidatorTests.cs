using System.Collections.Generic;
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
        public void ValidatePropertyValue_Text_Success()
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
        public void ValidatePropertyValue_Text_IsRequired_Failure()
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
            ValidatePropertyValue(action, propertyType, null, null, false, WorkflowDataValidationErrorCodes.PropertyValueEmpty);
        }

        #endregion

        #region Private methods

        private static void ValidatePropertyValue(IePropertyChangeAction action, PropertyType propertyType,
            ISet<string> validUsers, ISet<string> validGroups, bool expectedResult,
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
