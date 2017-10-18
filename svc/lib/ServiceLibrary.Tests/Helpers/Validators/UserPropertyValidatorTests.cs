using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Helpers.Validators
{
    [TestClass]
    public class UserPropertyValidatorTests
    {
        private UserPropertyValidator _validator;
        private IValidationContext _validationContext;
        private PropertyLite _propertyLite;
        private WorkflowPropertyType _propertyType;

        private int DefaultPropertyTypeId = 123;
        private int DefaultInstancePropertyTypeId = 10;
        private UserGroup DefaultUser = new UserGroup() {Id = 1, IsGroup = false};
        private UserGroup DefaultGroup = new UserGroup() { Id = 2, IsGroup = true };

        [TestInitialize]
        public void TestInitialize()
        {
            _validator = new UserPropertyValidator();
            _propertyLite = new PropertyLite() {PropertyTypeId = DefaultInstancePropertyTypeId};
            _propertyType = new UserPropertyType()
            {
                PropertyTypeId = DefaultPropertyTypeId,
                InstancePropertyTypeId = DefaultInstancePropertyTypeId
            };
            _validationContext = new ValidationContext(
                new List<SqlUser>() {new SqlUser() {UserId = DefaultUser.Id.Value} },
                new List<SqlGroup>() {new SqlGroup() {GroupId = DefaultGroup.Id.Value} });
        }

        #region Tests

        [TestMethod]
        public void Validate_ValidUser_Success()
        {
            // Arrange
            _propertyLite.UsersAndGroups.Add(DefaultUser);

            // Act
            var actualResult = _validator.Validate(
                _propertyLite, 
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_ValidGroup_Success()
        {
            // Arrange
            _propertyLite.UsersAndGroups.Add(DefaultGroup);

            // Act
            var actualResult = _validator.Validate(
                _propertyLite,
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_ValidUserAndGroup_Success()
        {
            // Arrange
            _propertyLite.UsersAndGroups.Add(DefaultUser);
            _propertyLite.UsersAndGroups.Add(DefaultGroup);

            // Act
            var actualResult = _validator.Validate(
                _propertyLite,
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult, null, "There should not be validation errors.");
        }

        [TestMethod]
        public void Validate_UserNotFound_ReturnsError()
        {
            // Arrange.
            _propertyLite.UsersAndGroups.Add(DefaultUser);
            _validationContext = new ValidationContext(new List<SqlUser>(), new List<SqlGroup>());

            // Act.
            var actualResult = _validator.Validate(
                _propertyLite,
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult.ErrorCode, ErrorCodes.InvalidArtifactProperty, "Invalid artifact property type error code must be returned.");
        }

        [TestMethod]
        public void Validate_GroupNotFound_ReturnsError()
        {
            // Arrange.
            _propertyLite.UsersAndGroups.Add(DefaultGroup);
            _validationContext = new ValidationContext(new List<SqlUser>(), new List<SqlGroup>());

            // Act.
            var actualResult = _validator.Validate(
                _propertyLite,
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult.ErrorCode, ErrorCodes.InvalidArtifactProperty, "Invalid artifact property type error code must be returned.");
        }

        [TestMethod]
        public void Validate_RequiredEmptyValue_ReturnsError()
        {
            // Arrange.
            _propertyType.IsRequired = true;
            _propertyLite.UsersAndGroups.Clear();

            // Act.
            var actualResult = _validator.Validate(
                _propertyLite,
                new List<WorkflowPropertyType>() { _propertyType },
                _validationContext);

            // Assert.
            Assert.AreEqual(actualResult.ErrorCode, ErrorCodes.InvalidArtifactProperty, "Invalid artifact property error code must be returned.");
        }

        #endregion
    }
}
