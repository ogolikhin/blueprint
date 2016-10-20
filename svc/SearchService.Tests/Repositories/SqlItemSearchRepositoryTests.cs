using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    [TestClass]
    public class SqlItemSearchRepositoryTests
    {
        private const int UserId = 1;
        private const int Page = 0;
        private const int StartOffset = 0;
        private const int PageSize = 10;
        private const int MaxItems = 500;
        private const int MaxSearchableValueStringSize = 250;

        #region Constructor

        [TestMethod]
        public void Constructor_CreatesConnectionToBlueprint()
        {
            // Arrange

            // Act
            var repository = new SqlItemSearchRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.BlueprintConnectionString, repository.ConnectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region SearchName

        [TestMethod]
        public async Task SearchName_WithItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            ItemSearchResultItem[] queryResult =
            {
                new ItemSearchResultItem()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.SearchItems.ToList());
        }

        [TestMethod]
        public async Task SearchName_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            ItemSearchResultItem[] queryResult =
            {
                new ItemSearchResultItem()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

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
            var searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            FullTextSearchItem[] queryResult =
            {
                new FullTextSearchItem()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.Search(UserId, searchCriteria, Page, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.FullTextSearchItems.ToList());
        }

        [TestMethod]
        public async Task Search_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            FullTextSearchItem[] queryResult =
            {
                new FullTextSearchItem()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

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
            var searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            FullTextSearchTypeItem[] queryResult =
            {
                new FullTextSearchTypeItem()
            };
            int?[] queryResult2 =
            {
                1
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult, queryResult2);

            // Act
            var result = await itemSearchRepository.SearchMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.ToList(), result.FullTextSearchTypeItems.ToList());
            Assert.AreEqual(queryResult2[0], result.TotalCount);
        }

        [TestMethod]
        public async Task SearchMetaData_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new SearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            FullTextSearchTypeItem[] queryResult =
            {
                new FullTextSearchTypeItem()
            };
            int?[] queryResult2 =
            {
                1
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult, queryResult2);

            // Act
            var result = await itemSearchRepository.SearchMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.ToList(), result.FullTextSearchTypeItems.ToList());
            Assert.AreEqual(queryResult2[0], result.TotalCount);
        }

        #endregion SearchMetaData

        private static IItemSearchRepository CreateRepository<T>(ISearchCriteria searchCriteria, ICollection<T> queryResult, ICollection<int?> queryResult2 = null)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();
            var commonParams = new Dictionary<string, object>
            {
                { "userId", UserId },
                { "query", searchCriteria.Query },
                { "projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds, "Int32Collection", "Int32Value") },
                { "primitiveItemTypePredefineds", SqlItemSearchRepository.PrimitiveItemTypePredefineds },
                { "maxSearchableValueStringSize", MaxSearchableValueStringSize },
            };
            connectionWrapper.SetupQueryAsync("SearchItemName",
                new Dictionary<string, object>
                {
                    { "startOffset", StartOffset },
                    { "pageSize", PageSize },
                },
                queryResult);
            connectionWrapper.SetupQueryAsync("SearchFullText",
                new Dictionary<string, object>
                {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "page", Page },
                    { "pageSize", PageSize },
                    { "maxItems", MaxItems },
                },
                queryResult);
            connectionWrapper.SetupQueryMultipleAsync("SearchFullTextMetaData",
                new Dictionary<string, object>
                {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                },
                new Tuple<IEnumerable<T>, IEnumerable<int?>>(queryResult, queryResult2));
            if (searchCriteria.ItemTypeIds != null)
            {
                connectionWrapper.SetupQueryAsync("SearchItemNameByItemTypes",
                    new Dictionary<string, object>(commonParams)
                    {
                    { "startOffset", StartOffset },
                    { "pageSize", PageSize },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                    },
                    queryResult);
                connectionWrapper.SetupQueryAsync("SearchFullTextByItemTypes",
                    new Dictionary<string, object>
                    {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "page", Page },
                    { "pageSize", PageSize },
                    { "maxItems", MaxItems },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                    },
                    queryResult);
                connectionWrapper.SetupQueryMultipleAsync("SearchFullTextByItemTypesMetaData",
                    new Dictionary<string, object>
                    {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds, "Int32Collection", "Int32Value") }
                    },
                    new Tuple<IEnumerable<T>, IEnumerable<int?>>(queryResult, queryResult2));
            }

            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToStringInvariant());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToStringInvariant());

            return new SqlItemSearchRepository(connectionWrapper.Object, configuration.Object);
        }
    }
}
