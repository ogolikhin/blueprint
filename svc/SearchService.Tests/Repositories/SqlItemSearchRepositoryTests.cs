using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    [TestClass]
    public class SqlItemSearchRepositoryTests
    {
        const int UserId = 1;
        const int Page = 0;
        const int StartOffset = 0;
        const int PageSize = 10;
        const int MaxItems = 500;
        const int MaxSearchableValueStringSize = 250;

        #region SearchName

        [TestMethod]
        public async Task SearchName_WithItemTypes_ReturnsResults()
        {
            // Arrange
            ItemSearchCriteria searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            ItemSearchResultItem[] queryResult =
            {
                new ItemSearchResultItem()
            };
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryAsync("SearchItemNameByItemTypes",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", searchCriteria.Query },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "startOffset", StartOffset },
                    { "pageSize", PageSize },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            connection.VerifyAll();
            CollectionAssert.AreEqual(queryResult, result.SearchItems.ToList());
        }

        [TestMethod]
        public async Task SearchName_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            ItemSearchCriteria searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            ItemSearchResultItem[] queryResult =
            {
                new ItemSearchResultItem()
            };
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryAsync("SearchItemName",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", searchCriteria.Query },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "startOffset", StartOffset },
                    { "pageSize", PageSize },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.SearchItems.ToList());
        }

        #endregion SearchName

        #region Search

        [TestMethod]
        public async Task Search_WithItemTypes_ReturnsResults()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            FullTextSearchItem[] queryResult =
            {
                new FullTextSearchItem()
            };
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryAsync("SearchFullTextByItemTypes",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", SqlItemSearchRepository.GetQuery(searchCriteria.Query) },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "page", Page },
                    { "pageSize", PageSize },
                    { "maxItems", MaxItems },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.Search(UserId, searchCriteria, Page, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.FullTextSearchItems.ToList());
        }

        [TestMethod]
        public async Task Search_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            FullTextSearchItem[] queryResult =
            {
                new FullTextSearchItem()
            };
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryAsync("SearchFullText",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", SqlItemSearchRepository.GetQuery(searchCriteria.Query) },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "page", Page },
                    { "pageSize", PageSize },
                    { "maxItems", MaxItems },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.Search(UserId, searchCriteria, Page, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.FullTextSearchItems.ToList());
        }

        #endregion Search

        #region SearchMetaData

        [TestMethod]
        public async Task SearchMetaData_WithItemTypes_ReturnsResults()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            var queryResult = new Tuple<IEnumerable<FullTextSearchTypeItem>, IEnumerable<int?>>(new[]
            {
                new FullTextSearchTypeItem()
            }, new[]
            {
                (int?)1
            });
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryMultipleAsync("SearchFullTextByItemTypesMetaData",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", SqlItemSearchRepository.GetQuery(searchCriteria.Query) },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.SearchMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.Item1.ToList(), result.FullTextSearchTypeItems.ToList());
            Assert.AreEqual(queryResult.Item2.FirstOrDefault(), result.TotalCount);
        }
        [TestMethod]
        public async Task SearchMetaData_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            SearchCriteria searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            var queryResult = new Tuple<IEnumerable<FullTextSearchTypeItem>, IEnumerable<int?>>(new[]
            {
                new FullTextSearchTypeItem()
            }, new[]
            {
                (int?)1
            });
            var connection = new SqlConnectionWrapperMock();
            connection.SetupQueryMultipleAsync("SearchFullTextMetaData",
                new Dictionary<string, object>
                {
                    { "userId", UserId },
                    { "query", SqlItemSearchRepository.GetQuery(searchCriteria.Query) },
                    { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                    { "maxSearchableValueStringSize", MaxSearchableValueStringSize }
                },
                queryResult);
            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToString());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToString());
            var itemSearchRepository = new SqlItemSearchRepository(connection.Object, configuration.Object);

            // Act
            var result = await itemSearchRepository.SearchMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.Item1.ToList(), result.FullTextSearchTypeItems.ToList());
            Assert.AreEqual(queryResult.Item2.FirstOrDefault(), result.TotalCount);
        }

        #endregion SearchMetaData
    }
}
