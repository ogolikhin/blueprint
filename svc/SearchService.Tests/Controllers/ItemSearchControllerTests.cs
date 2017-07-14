using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using SearchService.Repositories;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories.ConfigControl;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SearchService.Controllers
{
    [TestClass]
    public class ItemSearchControllerTests
    {
        #region SearchFullText

        [TestMethod]
        public async Task SearchFullText_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = null, ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "12", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
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
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.SearchFullText(searchCriteria, 10, 1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(10, result.Page);
        }

        [TestMethod]
        public async Task SearchFullText_RepoThrowsException_LogShouldBeCalled()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var logMock = new Mock<IServiceLogRepository>(MockBehavior.Strict);
            logMock.Setup(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.Delay(1));
            var exceptionToBeThrown = new Exception("MyException");
            var controller = CreateControllerForExceptionCases(logMock, exceptionToBeThrown);
            Exception actualException = null;

            // Act
            try
            {
                await controller.SearchFullText(searchCriteria, 10, 1);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }
            

            // Assert
            Assert.IsNotNull(actualException);
            Assert.AreEqual(exceptionToBeThrown.Message, actualException.Message, "Incorrect exception was thrown");
            logMock.Verify(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(1));
        }

        #endregion SearchFullText

        #region FullTextMetaData

        [TestMethod]
        public async Task FullTextMetaData_PageSizeNegative_ReturnsConstant()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var controller = CreateController();

            // Act
            var result = await controller.FullTextMetaData(searchCriteria, -1);

            // Assert
            Assert.IsNotNull(result, "Result was not retrieved");
            Assert.AreEqual(ServiceConstants.SearchPageSize, result.PageSize);
        }

        [TestMethod]
        public async Task FullTextMetaData_RepoThrowsException_LogShouldBeCalled()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria { Query = "empty", ProjectIds = new[] { 1 } };
            var logMock = new Mock<IServiceLogRepository>(MockBehavior.Strict);
            logMock.Setup(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.Delay(1));
            var exceptionToBeThrown = new Exception("MyException");
            var controller = CreateControllerForExceptionCases(logMock, exceptionToBeThrown);
            Exception actualException = null;

            // Act
            try
            {
                await controller.SearchFullText(searchCriteria, 10, 1);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }


            // Assert
            Assert.IsNotNull(actualException);
            Assert.AreEqual(exceptionToBeThrown.Message, actualException.Message, "Incorrect exception was thrown");
            logMock.Verify(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(1));
        }

        #endregion FullTextMetaData

        #region SearchName

        [TestMethod]
        public async Task SearchName_Success()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 20;
            var controller = CreateController(1);

            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.PageItemCount);
        }

        [TestMethod]
        public async Task SearchName_Forbidden()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
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
            var searchCriteria = new ItemNameSearchCriteria { Query = "" };
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
            var searchCriteria = new ItemNameSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 200;
            var controller = CreateController();

            // Act
            var result = await controller.SearchName(searchCriteria, startOffset, pageSize);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task SearchName_RepoThrowsException_LogShouldBeCalled()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria { Query = "Test", ProjectIds = new List<int> { 5 } };
            var startOffset = 0;
            var pageSize = 200;
            
            var logMock = new Mock<IServiceLogRepository>(MockBehavior.Strict);
            logMock.Setup(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>())).Returns(Task.Delay(1));
            var exceptionToBeThrown = new Exception("MyException");
            var controller = CreateControllerForExceptionCases(logMock, exceptionToBeThrown);
            Exception actualException = null;

            // Act
            try
            {
                var result = await controller.SearchName(searchCriteria, startOffset, pageSize);
            }
            catch (Exception ex)
            {
                actualException = ex;
            }


            // Assert
            Assert.IsNotNull(actualException);
            Assert.AreEqual(exceptionToBeThrown.Message, actualException.Message, "Incorrect exception was thrown");
            logMock.Verify(t => t.LogError(It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()), Times.Exactly(1));
        }

        #endregion SearchName

        private ItemSearchController CreateController(int itemNameResultCount = 0)
        {
            var itemSearchRepository = new Mock<IItemSearchRepository>();
            itemSearchRepository.Setup(a => a.SearchFullText(It.IsAny<int>(), It.IsAny<FullTextSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>())).
                Returns((int userId, SearchCriteria searchCriteria, int page, int pageSize) => Task.FromResult(new FullTextSearchResultSet
                {
                    Items = new List<FullTextSearchResult>(),
                    Page = page,
                    PageItemCount = 0,
                    PageSize = pageSize
                }));
            itemSearchRepository.Setup(a => a.FullTextMetaData(It.IsAny<int>(), It.IsAny<FullTextSearchCriteria>())).
                ReturnsAsync(new MetaDataSearchResultSet { Items = new List<MetaDataSearchResult>() });
            var itemNameSearchResult = new List<ItemNameSearchResult>();
            for (var i = 0; i < itemNameResultCount; i++)
            {
                itemNameSearchResult.Add(new ItemNameSearchResult
                {
                    Id = i,
                    ItemId = i,
                    LockedByUserId = i,
                    ProjectId = 1
                });
            }
            itemSearchRepository.Setup(m => m.SearchName(It.IsAny<int>(), It.IsAny<ItemNameSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new ItemNameSearchResultSet { Items = itemNameSearchResult, PageItemCount = itemNameSearchResult.Count });

            var configuration = new Mock<ISearchConfiguration>();
            var logMock = new Mock<IServiceLogRepository>();

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });
            return new ItemSearchController(itemSearchRepository.Object, configuration.Object, logMock.Object)
            {
                Request = request
            };
        }

        private ItemSearchController CreateControllerForExceptionCases(Mock<IServiceLogRepository> logMock, Exception exceptionToBeThrown)
        {
            var itemSearchRepository = new Mock<IItemSearchRepository>();
            itemSearchRepository.Setup(a => a.SearchFullText(It.IsAny<int>(), It.IsAny<FullTextSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(exceptionToBeThrown);
            itemSearchRepository.Setup(a => a.FullTextMetaData(It.IsAny<int>(), It.IsAny<FullTextSearchCriteria>()))
                .Throws(exceptionToBeThrown);
            itemSearchRepository.Setup(m => m.SearchName(It.IsAny<int>(), It.IsAny<ItemNameSearchCriteria>(), It.IsAny<int>(), It.IsAny<int>()))
                .Throws(exceptionToBeThrown);

            var configuration = new Mock<ISearchConfiguration>();

            var request = new HttpRequestMessage();
            request.Properties.Add(ServiceConstants.SessionProperty, new Session { UserId = 1 });
            return new ItemSearchController(itemSearchRepository.Object, configuration.Object, logMock.Object)
            {
                Request = request
            };
        }
    }
}
