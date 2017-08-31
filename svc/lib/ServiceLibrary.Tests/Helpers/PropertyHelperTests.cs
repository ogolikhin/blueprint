using System;
using System.Globalization;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models.Workflow;

namespace ServiceLibrary.Helpers
{
    [TestClass]
    public class PropertyHelperTests
    {

        [TestMethod]
        public void ParseDateValue_ReturnsCurrentDate_WhenDateValueIsZero()
        {
            //arrange
            const string dateValue = "0";
            var today = new DateTime(2017, 1, 1);
            var mockTimeProvider = new Mock<ITimeProvider>();
            mockTimeProvider.Setup(m => m.Today).Returns(today);
            //act
            var date = PropertyHelper.ParseDateValue(dateValue, mockTimeProvider.Object);
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
            var date = PropertyHelper.ParseDateValue(dateValue, mockTimeProvider.Object);
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
            var date = PropertyHelper.ParseDateValue(dateValue, mockTimeProvider.Object);
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
            var date = PropertyHelper.ParseDateValue(dateValue, new TimeProvider());
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
            PropertyHelper.ParseDateValue(unsupportedDateFormat, new TimeProvider());
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void ParseDateValue_ThrowsFormatException_WhenDateValueIsInvalid()
        {
            //arrange
            const string dateValue = "invalid date";
            //act
            PropertyHelper.ParseDateValue(dateValue, new TimeProvider());
        }
    }
}
