using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace SearchService.Controllers
{
    [TestClass]
    public class ItemSearchControllerTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesDefaultDependencies()
        {
            // Arrange

            // Act
            var controller = new ItemSearchController();

            // Assert
            Assert.IsInstanceOfType(controller._itemSearchRepository, typeof(SqlItemSearchRepository));
        }

        #endregion

        #region SearchFullText

        [TestMethod]
        public async Task SearchFullText_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, null, -1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(ServiceConstants.SearchPageSize, result.PageSize);
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsNull_BadRequest()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = null, ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsEmpty_BadRequest()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_SearchCriteriaIsLessThanThreeCharacters_BadRequest()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "12", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchFullText(searchCriteria, null, -1);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchFullText_ZeropPageSize_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, null, 0);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(ServiceConstants.SearchPageSize, result.PageSize);
        }

        [TestMethod]
        public async Task SearchFullText_50PageSize_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, null, 50);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(50, result.PageSize);
        }

        [TestMethod]
        public async Task SearchFullText_RequestNegativePage_ReturnsPageOne()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, -10, 1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(1, result.Page);
        }

        [TestMethod]
        public async Task SearchFullText_RequestZeroPage_ReturnsPageOne()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, 0, 1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(1, result.Page);
        }

        [TestMethod]
        public async Task SearchFullText_RequestPositivePage_ReturnsPageOne()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, 10, 1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(10, result.Page);
        }

        #endregion SearchFullText

        #region FullTextMetaData

        [TestMethod]
        public async Task FullTextMetaData_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.FullTextMetaData(searchCriteria, -1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(ServiceConstants.SearchPageSize, result.PageSize);
        }

        #endregion FullTextMetaData

        #region SearchName

        [TestMethod]
        public async Task SearchName_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 20;
            var controller = CreateController();

            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.PageItemCount);
        }

        [TestMethod]
        public async Task SearchName_Forbidden()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 20;
            var controller = CreateController();

            // Act
            try
            {
                await controller.SearchName(searchCriteria, startOffset, pageSize);
            }
            catch (AuthenticationException e)
            {
                // Assert
                Assert.AreEqual(ErrorCodes.UnauthorizedAccess, e.ErrorCode);
            }
        }

        [TestMethod]
        public async Task SearchName_NullSerachCriteria_BadRequest()
        {
            // Arrange
            var controller = CreateController();

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchName(null);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchName_QueryIsEmpty_BadRequest()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "" };
            var controller = CreateController();

            // Act
            BadRequestException badRequestException = null;
            try
            {
                await controller.SearchName(searchCriteria);
            }
            catch (BadRequestException e)
            {
                badRequestException = e;
            }

            // Assert
            Assert.IsNotNull(badRequestException, "Bad Request Exception should have been thrown");
            Assert.AreEqual(ErrorCodes.IncorrectSearchCriteria, badRequestException.ErrorCode, "IncorrectSearchCriteria should be provided as Error code");
        }

        [TestMethod]
        public async Task SearchName_ResultCountMoreThanMax_Success()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 200;
            var controller = CreateController();

            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
        }

        #endregion SearchName

        private ItemSearchController CreateController()
        {
            var itemSearchRepository = new Mock<IItemSearchRepository>();
            itemSearchRepository.Setup(a => a.SearchFullText(It.IsAny<int>(), It.IsAny<ItemSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).
                Returns((int userId, SearchCriteria searchCriteria, int page, int pageSize) => Task.FromResult(new FullTextSearchResultSet
                {
                    Items = new List<FullTextSearchResult>(),
                    Page = page,
                    PageItemCount = 0,
                    PageSize = pageSize
                }));
            itemSearchRepository.Setup(a => a.FullTextMetaData(It.IsAny<int>(), It.IsAny<ItemSearchCriteria>())).
                ReturnsAsync(new MetaDataSearchResultSet { Items = new List<MetaDataSearchResult>() });
            itemSearchRepository.Setup(m => m.SearchName(It.IsAny<int>(), It.IsAny<ItemSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ItemNameSearchResultSet { Items = new List<ItemSearchResult>(), PageItemCount = 0 });

            var configuration = new Mock<ISearchConfiguration>();

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });
            return new ItemSearchController(itemSearchRepository.Object, configuration.Object)
            {
                Request = request
            };
        }
    }
}
