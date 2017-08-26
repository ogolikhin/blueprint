using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;
using ServiceLibrary.Models.Workflow;
using ServiceLibrary.Models.Workflow.Actions;

namespace ArtifactStore.Models.Workflow.Actions
{
    [TestClass]
    public class PropertyChangeActionTests
    {
        private Mock<ISaveArtifactRepository> _saveRepositoryMock;
        private Mock<IReusePropertyValidator> _reuseValidatorMock;
        private List<DPropertyType> _customPropertyTypes;

        private IExecutionParameters _executionParameters;
        private PropertyChangeAction _propertyChangeAction;

        private const int DefaultInstancePropertyTypeId = 123;
        private const string DefaultValue = "99";

        [TestInitialize]
        public void TestInitialize()
        {
            _propertyChangeAction = new PropertyChangeAction()
            {
                InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                PropertyValue = DefaultValue
            };
            _saveRepositoryMock = new Mock<ISaveArtifactRepository>();
            _reuseValidatorMock = new Mock<IReusePropertyValidator>();
            _customPropertyTypes = new List<DPropertyType>();
            _customPropertyTypes.Add(
                new DNumberPropertyType()
                {
                    InstancePropertyTypeId = DefaultInstancePropertyTypeId,
                    PrimitiveType = PropertyPrimitiveType.Number
                });
        }

        [TestMethod]
        [ExpectedException(typeof(ConflictException))]
        public async Task Execute_PropertyTypeIdNotFound_ThrowsException()
        {
            //Arrange
            _propertyChangeAction.InstancePropertyTypeId = 5;
            _executionParameters = new ExecutionParameters(
                1,
                new VersionControlArtifactInfo(),
                null,
                _customPropertyTypes,
                _saveRepositoryMock.Object,
                null,
                new List<IPropertyValidator>(),
                _reuseValidatorMock.Object);

            //Act
            await _propertyChangeAction.Execute(_executionParameters);
        }

        [TestMethod]
        public async Task Execute_NumberPropertyChangeAction_NumberIsPopulated()
        {
            //Arrange
            _executionParameters = new ExecutionParameters(
                1,
                new VersionControlArtifactInfo(),
                null,
                _customPropertyTypes,
                _saveRepositoryMock.Object,
                null,
                new List<IPropertyValidator>(), 
                _reuseValidatorMock.Object);

            //Act
            var result = await _propertyChangeAction.Execute(_executionParameters);
            var propertyLiteValue = _propertyChangeAction.PropertyLiteValue;

            //Assert
            Assert.IsTrue(result);
            Assert.IsTrue(propertyLiteValue.NumberValue.HasValue);
        }

        [TestMethod]
        public void ParseDateValue_ReturnsCurrentDate_WhenDateValueIsCurrentDate()
        {
            //arrange
            const string dateValue = PropertyChangeAction.CurrentDate;
            var today = new DateTime(2017, 1, 1);
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(m => m.Today).Returns(today);
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, mockTimeProvider.Object);
            //assert
            Assert.AreEqual(today, date);
        }

        [TestMethod]
        public void ParseDateValue_ReturnsFutureDate_WhenDateValueIsCurrentDatePlusInteger()
        {
            //arrange
            const int daysToAdd = 1;
            var dateValue = PropertyChangeAction.CurrentDate + PropertyChangeAction.Plus + daysToAdd;
            var today = new DateTime(2018, 2, 2);
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(m => m.Today).Returns(today);
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, mockTimeProvider.Object);
            //assert
            var expected = today.AddDays(daysToAdd);
            Assert.AreEqual(expected, date);
        }

        [TestMethod]
        public void ParseDateValue_ReturnsSpecifiedDate_WhenDateValueIsASpecificDate()
        {
            //arrange
            var expectedDate = new DateTime(2019, 3, 3);
            var dateValue = expectedDate.ToShortDateString();
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, new TimeProvider());
            //assert
            Assert.AreEqual(expectedDate, date);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseDateValue_ThrowsFormatException_WhenDateValueIsInvalid()
        {
            //arrange
            const string dateValue = "invalid date";
            //act
            PropertyChangeAction.ParseDateValue(dateValue, new TimeProvider());
        }
    }
}
