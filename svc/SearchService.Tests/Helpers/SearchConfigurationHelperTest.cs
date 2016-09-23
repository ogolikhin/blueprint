using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using System.Globalization;

namespace SearchService.Helpers
{
    [TestClass]
    public class SearchConfigurationHelperTest
    {
        #region MaxSearcItems tests
        [TestMethod]
        public void MaxSearchItems_InvalidValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.MaxItems).Returns("abcd");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxItems = helper.MaxItems;

            Assert.AreEqual(maxItems, ServiceConstants.MaxSearchItems);
        }
        [TestMethod]
        public void MaxSearchItems_NegativeValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.MaxItems).Returns("-1");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxItems = helper.MaxItems;

            Assert.AreEqual(maxItems, ServiceConstants.MaxSearchItems);
        }

        [TestMethod]
        public void MaxSearchItems_ValidValue_ReturnsValue()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.MaxItems).Returns("5");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxItems = helper.MaxItems;

            Assert.AreNotEqual(maxItems, ServiceConstants.MaxSearchItems);
            Assert.AreEqual(maxItems, 5);
        }
        #endregion
        #region MaxSearchableValueStringSize
        [TestMethod]
        public void MaxSearchableValueStringSize_InvalidValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.MaxSearchableValueStringSize).Returns("abcd");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxSearchableValueStringValue = helper.MaxSearchableValueStringSize;

            Assert.AreEqual(maxSearchableValueStringValue, ServiceConstants.MaxSearchableValueStringSize);
        }

        [TestMethod]
        public void MaxSearchableValueStringSize_NegativeValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.MaxSearchableValueStringSize).Returns("-1");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxSearchableValueStringValue = helper.MaxSearchableValueStringSize;

            Assert.AreEqual(maxSearchableValueStringValue, ServiceConstants.MaxSearchableValueStringSize);
        }

        [TestMethod]
        public void MaxSearchableValueStringSize_ValidValue_ReturnsValue()
        {
            var configuration = new Mock<ISearchConfiguration>();
            var value = 10;
            configuration.Setup(a => a.MaxSearchableValueStringSize).Returns(value.ToString(CultureInfo.InvariantCulture));

            var helper = new SearchConfigurationProvider(configuration.Object);

            var maxSearchableValueStringValue = helper.MaxSearchableValueStringSize;

            Assert.AreEqual(maxSearchableValueStringValue, value);
        }
        #endregion
        #region PageSize tests
        [TestMethod]
        public void PageSize_InvalidValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.PageSize).Returns("abcd");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var pageSize = helper.PageSize;

            Assert.AreEqual(pageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public void PageSize_NegativeValue_ReturnsServerConstant()
        {
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(a => a.PageSize).Returns("-1");

            var helper = new SearchConfigurationProvider(configuration.Object);

            var pageSize = helper.PageSize;

            Assert.AreEqual(pageSize, ServiceConstants.SearchPageSize);
        }

        [TestMethod]
        public void PageSize_ValidValue_ReturnsValue()
        {
            var configuration = new Mock<ISearchConfiguration>();
            var value = 10;
            configuration.Setup(a => a.PageSize).Returns(value.ToString(CultureInfo.InvariantCulture));

            var helper = new SearchConfigurationProvider(configuration.Object);

            var pageSize = helper.PageSize;

            Assert.AreEqual(pageSize, value);
        }        
        #endregion
    }
}
