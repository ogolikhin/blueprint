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
            ItemSearchResult[] queryResult =
            {
                new ItemSearchResult()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
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
            ItemSearchResult[] queryResult =
            {
                new ItemSearchResult()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
        }

        #endregion SearchName

        #region SearchFullText

        [TestMethod]
        public async Task SearchFullText_WithItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            FullTextSearchResult[] queryResult =
            {
                new FullTextSearchResult()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.SearchFullText(UserId, searchCriteria, Page, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(Page, result.Page);
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
            Assert.AreEqual(PageSize, result.PageSize);
        }

        [TestMethod]
        public async Task SearchFullText_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            FullTextSearchResult[] queryResult =
            {
                new FullTextSearchResult()
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult);

            // Act
            var result = await itemSearchRepository.SearchFullText(UserId, searchCriteria, Page, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(Page, result.Page);
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
            Assert.AreEqual(PageSize, result.PageSize);
        }

        #endregion SearchFullText

        #region FullTextMetaData

        [TestMethod]
        public async Task FullTextMetaData_WithItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            MetaDataSearchResult[] queryResult =
            {
                new MetaDataSearchResult()
            };
            int?[] queryResult2 =
            {
                1
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult, queryResult2);

            // Act
            var result = await itemSearchRepository.FullTextMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.ToList(), result.Items.ToList());
            Assert.AreEqual(queryResult2[0], result.TotalCount);
        }

        [TestMethod]
        public async Task FullTextMetaData_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            MetaDataSearchResult[] queryResult =
            {
                new MetaDataSearchResult()
            };
            int?[] queryResult2 =
            {
                1
            };
            var itemSearchRepository = CreateRepository(searchCriteria, queryResult, queryResult2);

            // Act
            var result = await itemSearchRepository.FullTextMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.ToList(), result.Items.ToList());
            Assert.AreEqual(queryResult2[0], result.TotalCount);
        }

        #endregion SearchMetaData

        private static IItemSearchRepository CreateRepository<T>(ItemSearchCriteria searchCriteria, ICollection<T> queryResult, ICollection<int?> queryResult2 = null)
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
