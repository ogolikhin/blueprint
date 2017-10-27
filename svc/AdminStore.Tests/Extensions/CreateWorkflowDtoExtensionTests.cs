using System;
using AdminStore.Helpers.Workflow;
using AdminStore.Models.DTO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;

namespace AdminStore.Extensions
{
    [TestClass]
    public class CreateWorkflowDtoExtensionTests
    {

        [TestMethod]
        public void Validate_NameToShort_BadRequest()
        {
            // arrange
            Exception exception = null;
            var model = new CreateWorkflowDto() { Name = "a" };

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
            var model = new CreateWorkflowDto() { Name = "Lorem ipsum dolor sit ame" }; // 25 symbols - only max 24 is Ok

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
        public void Validate_DescriptionLimitReached_BadRequest()
        {
            // arrange
            Exception exception = null;
            var model = new CreateWorkflowDto()
            {
                Name = "aasdff",
                Description =
                    "Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Aenean commodo ligula eget dolor. Aenean massa. Cum sociis natoque penatibus et magnis dis parturient montes, nascetur ridiculus mus. Donec quam felis, ultricies nec, pellentesque eu, pretium quis, sem. Nulla consequat massa quis enim. Donec pede justo, fringilla vel, aliquet nec, vulputate eget, arcu. In enim justo, rhoncus ut, imperdiet a, venenati"
            };

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
            Assert.AreEqual(ErrorMessages.WorkflowDescriptionLimit, exception.Message);
        }
    }
}
