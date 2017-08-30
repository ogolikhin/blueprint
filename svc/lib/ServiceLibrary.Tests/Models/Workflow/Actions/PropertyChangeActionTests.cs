using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.ProjectMeta;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.VersionControl;

namespace ServiceLibrary.Models.Workflow.Actions
{
    [TestClass]
    public class PropertyChangeActionTests
    {
        private Mock<ISaveArtifactRepository> _saveRepositoryMock;
        private Mock<IReusePropertyValidator> _reuseValidatorMock;
        private List<WorkflowPropertyType> _customPropertyTypes;

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
            _customPropertyTypes = new List<WorkflowPropertyType>();
            _customPropertyTypes.Add(
                new NumberPropertyType()
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
        public void ParseDateValue_ReturnsCurrentDate_WhenDateValueIsZero()
        {
            //arrange
            const string dateValue = "0";
            var today = new DateTime(2017, 1, 1);
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(m => m.Today).Returns(today);
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, mockTimeProvider.Object);
            //assert
            Assert.AreEqual(today, date);
        }

        [TestMethod]
        public void ParseDateValue_ReturnsFutureDate_WhenDateValueIsPositiveInteger()
        {
            //arrange
            const int daysToAdd = 1;
            var dateValue = daysToAdd.ToString(CultureInfo.InvariantCulture);
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
        public void ParseDateValue_ReturnsPastDate_WhenDateValueIsNegativeInteger()
        {
            //arrange
            const int daysToAdd = -1;
            var dateValue = daysToAdd.ToString(CultureInfo.InvariantCulture);
            var today = new DateTime(2019, 3, 3);
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(m => m.Today).Returns(today);
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, mockTimeProvider.Object);
            //assert
            var expected = today.AddDays(daysToAdd);
            Assert.AreEqual(expected, date);
        }

        [TestMethod]
        public void ParseDateValue_ReturnsSpecifiedDate_WhenDateValueIsASpecificDateInTheCorrectFormat()
        {
            //arrange
            var expectedDate = new DateTime(2020, 4, 4);
            var dateValue = expectedDate.ToString(WorkflowConstants.Iso8601DateFormat, CultureInfo.InvariantCulture);
            //act
            var date = PropertyChangeAction.ParseDateValue(dateValue, new TimeProvider());
            //assert
            Assert.AreEqual(expectedDate, date);
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseDateValue_ReturnsSpecifiedDate_WhenDateValueIsASpecificDateInAnUnsupportedFormat()
        {
            //arrange
            var date = new DateTime(2021, 5, 5);
            var unsupportedDateFormat = date.ToLongDateString();
            //act
            PropertyChangeAction.ParseDateValue(unsupportedDateFormat, new TimeProvider());
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
