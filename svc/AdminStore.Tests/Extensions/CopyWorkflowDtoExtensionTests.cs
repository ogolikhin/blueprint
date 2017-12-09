using System;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Extensions
{
    [TestClass]
    public class CopyWorkflowDtoExtensionTests
    {
        [TestMethod]
        public void Validate_NameIsNull_BadRequest()
        {
            // arrange
            Exception exception = null;
            var model = new CopyWorkflowDto { Name = null };

            // act
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorMessages.WorkflowNameError, exception.Message);
        }

        [TestMethod]
        public void Validate_NameIsEmptyString_BadRequest()
        {
            // arrange
            Exception exception = null;
            var model = new CopyWorkflowDto { Name = string.Empty };

            // act
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorMessages.WorkflowNameError, exception.Message);
        }

        [TestMethod]
        public void Validate_NameToLong_ReturnBadRequestException()
        {
            // arrange
            Exception exception = null;
            var model = new CopyWorkflowDto { Name = "Lorem ipsum dolor sit ame" }; // 25 symbols - only max 24 is Ok

            // act
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNotNull(exception);
            Assert.IsInstanceOfType(exception, typeof(BadRequestException));
            Assert.AreEqual(ErrorMessages.WorkflowNameError, exception.Message);
        }

        [TestMethod]
        public void Validate_NameIsValid_NoException()
        {
            // arrange
            Exception exception = null;
            var model = new CopyWorkflowDto { Name = "L" };

            // act
            try
            {
                model.Validate();
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // assert
            Assert.IsNull(exception);
        }
    }
}
