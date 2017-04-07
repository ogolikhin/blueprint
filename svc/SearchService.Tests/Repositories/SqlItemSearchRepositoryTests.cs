using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchService.Helpers;
using SearchService.Models;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
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
        public async Task SearchName_WithPredefinedTypeIdsWithSqlTimeoutException_SqlTimeoutExceptionOccurs()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                PredefinedTypeIds = new[] { 4104 }
            };
            var permissionsDictionary = new Dictionary<int, RolePermissions> { { 0, RolePermissions.Read } };
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);

            Exception sqlException = SqlExceptionCreator.NewSqlException(ErrorCodes.SqlTimeoutNumber);

            var itemSearchRepository = CreateItemNameRepositoryWithExceptionExpectation<ItemNameSearchResult>(mockArtifactPermissionsRepository.Object, null, sqlException);

            SqlTimeoutException sqlTimeoutException = null;

            // Act
            try
            {
                await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);
            }
            catch (SqlTimeoutException exception)
            {
                sqlTimeoutException = exception;
            }

            // Assert
            Assert.IsNotNull(sqlTimeoutException, "sqlTimeoutException != null");
            Assert.IsTrue(sqlTimeoutException.ErrorCode == ErrorCodes.Timeout, "Timeout exception should occur");
        }

        [TestMethod]
        public async Task SearchName_WithPredefinedTypeIdsWithSqlException_SqlExceptionOccurs()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                PredefinedTypeIds = new[] { 4104 }
            };
            
            var permissionsDictionary = new Dictionary<int, RolePermissions> { { 0, RolePermissions.Read } };
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);

            Exception sqlException = SqlExceptionCreator.NewSqlException(-4);

            var itemSearchRepository = CreateItemNameRepositoryWithExceptionExpectation<ItemNameSearchResult>(mockArtifactPermissionsRepository.Object, null, sqlException);

            SqlException actualSqlException = null;

            // Act
            try
            {
                await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);
            }
            catch (SqlException exception)
            {
                actualSqlException = exception;
            }

            // Assert
            Assert.IsNotNull(actualSqlException, "sqlException != null");
            Assert.IsTrue(actualSqlException.Number == -4, "Timeout exception should occur");
        }

        [TestMethod]
        public async Task SearchName_WithPredefinedTypeIds_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                PredefinedTypeIds = new[] { 4104 }
            };
            ItemNameSearchResult[] queryResult =
            {
                new ItemNameSearchResult()
            };
            var permissionsDictionary = new Dictionary<int, RolePermissions> {{0, RolePermissions.Read}};
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);
            var itemSearchRepository = CreateItemNameRepository(searchCriteria, queryResult, mockArtifactPermissionsRepository.Object, null);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
            Assert.AreEqual(result.Items.First().Permissions, RolePermissions.Read);
        }

        [TestMethod]
        public async Task SearchName_IncludeArtifactPath_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                IncludeArtifactPath = true
            };
            ItemNameSearchResult[] queryResult =
            {
                new ItemNameSearchResult()
            };
            var permissionsDictionary = new Dictionary<int, RolePermissions> { { 0, RolePermissions.Read } };
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);

            var artifactInfo = new Artifact()
            {
                Id = 1,
                Name = "ArtifactPath"
            };
            var infoCollection = new List<Artifact> {artifactInfo};
            var navigationPaths = new Dictionary<int, IEnumerable<Artifact>> { { 0, infoCollection } };
            var mockSqlArtifactRepository = new Mock<ISqlArtifactRepository>();
            mockSqlArtifactRepository.Setup(r => r.GetArtifactsNavigationPathsAsync(1, new List<int> { 0 }, false, null, true)).ReturnsAsync(navigationPaths);

            var itemSearchRepository = CreateItemNameRepository(searchCriteria, queryResult, mockArtifactPermissionsRepository.Object, mockSqlArtifactRepository.Object);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
            Assert.AreEqual(result.Items.First().Path.Count(), 1);
            Assert.AreEqual(result.Items.First().Path.First(), "ArtifactPath");
        }

        [TestMethod]
        public async Task SearchName_WithoutPermission_DoesNotReturn()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                PredefinedTypeIds = new[] { 4104 }
            };
            ItemNameSearchResult[] queryResult =
            {
                new ItemNameSearchResult()
            };
            var permissionsDictionary = new Dictionary<int, RolePermissions>();
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);
            var itemSearchRepository = CreateItemNameRepository(searchCriteria, queryResult, mockArtifactPermissionsRepository.Object, null);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            Assert.AreEqual(0, result.PageItemCount);
        }

        [TestMethod]
        public async Task SearchName_WithoutItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            ItemNameSearchResult[] queryResult =
            {
                new ItemNameSearchResult()
            };
            var permissionsDictionary = new Dictionary<int, RolePermissions> {{0, RolePermissions.Read}};
            var mockArtifactPermissionsRepository = new Mock<IArtifactPermissionsRepository>();
            mockArtifactPermissionsRepository.Setup(r => r.GetArtifactPermissions(new List<int> { 0 }, UserId, false, int.MaxValue, true)).ReturnsAsync(permissionsDictionary);
            var itemSearchRepository = CreateItemNameRepository(searchCriteria, queryResult, mockArtifactPermissionsRepository.Object, null);

            // Act
            var result = await itemSearchRepository.SearchName(UserId, searchCriteria, StartOffset, PageSize);

            // Assert
            CollectionAssert.AreEqual(queryResult, result.Items.ToList());
            Assert.AreEqual(queryResult.Length, result.PageItemCount);
        }

        #endregion SearchName

        #region GetExcludedPredefineds

        [TestMethod]
        public void ExcludedPredefineds_DoNotShowArtifacts_CorrectResult()
        {
            // Arrange
            var searchCriteria = new ItemNameSearchCriteria
            {
                ShowArtifacts = false,
                ShowBaselinesAndReviews = true,
                ShowCollections = true
            };
            ItemTypePredefined[] expected = {
                ItemTypePredefined.Glossary,
                ItemTypePredefined.TextualRequirement,
                ItemTypePredefined.PrimitiveFolder,
                ItemTypePredefined.BusinessProcess,
                ItemTypePredefined.Actor,
                ItemTypePredefined.UseCase,
                ItemTypePredefined.DataElement,
                ItemTypePredefined.UIMockup,
                ItemTypePredefined.GenericDiagram,
                ItemTypePredefined.Document,
                ItemTypePredefined.Storyboard,
                ItemTypePredefined.DomainDiagram,
                ItemTypePredefined.UseCaseDiagram,
                ItemTypePredefined.Process
            };

            // Act
            var excludedPredefineds = SqlItemSearchRepository.GetExcludedPredefineds(searchCriteria);

            // Assert
            CollectionAssert.AreEquivalent(expected, excludedPredefineds.Cast<ItemTypePredefined>().ToList());
        }

        [TestMethod]
        public void ExcludedPredefineds_DoNotShowBaselinesAndReviews_CorrectResult()
        {
            // Arrange
            ItemTypePredefined[] expected = {
                ItemTypePredefined.BaselineFolder,
                ItemTypePredefined.ArtifactBaseline,
                ItemTypePredefined.ArtifactReviewPackage
            };
            var searchCriteria = new ItemNameSearchCriteria
            {
                ShowArtifacts = true,
                ShowBaselinesAndReviews = false,
                ShowCollections = true
            };

            // Act
            var excludedPredefineds = SqlItemSearchRepository.GetExcludedPredefineds(searchCriteria);

            // Assert
            CollectionAssert.AreEquivalent(expected, excludedPredefineds.Cast<ItemTypePredefined>().ToList());
        }

        [TestMethod]
        public void ExcludedPredefineds_DoNotShowCollections_CorrectResult()
        {
            // Arrange
            ItemTypePredefined[] expected = {
                ItemTypePredefined.CollectionFolder,
                ItemTypePredefined.ArtifactCollection
            };
            var searchCriteria = new ItemNameSearchCriteria
            {
                ShowArtifacts = true,
                ShowBaselinesAndReviews = true,
                ShowCollections = false
            };

            // Act
            var excludedPredefineds = SqlItemSearchRepository.GetExcludedPredefineds(searchCriteria);

            // Assert
            CollectionAssert.AreEquivalent(expected, excludedPredefineds.Cast<ItemTypePredefined>().ToList());
        }

        #endregion GetExcludedPredefineds

        #region SearchFullText

        [TestMethod]
        public async Task SearchFullText_WithItemTypes_ReturnsResults()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 },
                ItemTypeIds = new[] { 10, 20, 30 }
            };
            FullTextSearchResult[] queryResult =
            {
                new FullTextSearchResult()
            };
            var itemSearchRepository = CreateFullTextSearchRepository(searchCriteria, queryResult);

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
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };
            FullTextSearchResult[] queryResult =
            {
                new FullTextSearchResult()
            };
            var itemSearchRepository = CreateFullTextSearchRepository(searchCriteria, queryResult);

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
            var searchCriteria = new FullTextSearchCriteria
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
            var itemSearchRepository = CreateFullTextSearchRepository(searchCriteria, queryResult, queryResult2);

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
            var searchCriteria = new FullTextSearchCriteria
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
            var itemSearchRepository = CreateFullTextSearchRepository(searchCriteria, queryResult, queryResult2);

            // Act
            var result = await itemSearchRepository.FullTextMetaData(UserId, searchCriteria);

            // Assert
            CollectionAssert.AreEqual(queryResult.ToList(), result.Items.ToList());
            Assert.AreEqual(queryResult2[0], result.TotalCount);
        }

        [TestMethod]
        public async Task FullTextMetaData_WithoutItemTypesThrowsTimeoutException_SqlTimeoutExceptionThrown()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };

            var sqlException = SqlExceptionCreator.NewSqlException(-2);
            var itemSearchRepository = CreateFullTextSearchRepositoryWithException<MetaDataSearchResult>(sqlException);

            SqlTimeoutException sqlTimeoutException = null;

            // Act
            try
            {
                await itemSearchRepository.FullTextMetaData(UserId, searchCriteria);
            }
            catch (SqlTimeoutException exception)
            {
                sqlTimeoutException = exception;
            }

            // Assert
            Assert.IsNotNull(sqlTimeoutException, "sqlTimeoutException != null");
            Assert.IsTrue(sqlTimeoutException.ErrorCode == ErrorCodes.Timeout, "Timeout exception should occur");
        }

        [TestMethod]
        public async Task FullTextMetaData_WithoutItemTypesThrowsSqlException_SqlExceptionReThrown()
        {
            // Arrange
            var searchCriteria = new FullTextSearchCriteria
            {
                Query = "test",
                ProjectIds = new[] { 1 }
            };

            var sqlException = SqlExceptionCreator.NewSqlException(-2146232060);
            var itemSearchRepository = CreateFullTextSearchRepositoryWithException<MetaDataSearchResult>(sqlException);

            SqlException thrownException = null;

            // Act
            try
            {
                await itemSearchRepository.FullTextMetaData(UserId, searchCriteria);
            }
            catch (SqlException exception)
            {
                thrownException = exception;
            }

            // Assert
            Assert.IsNotNull(thrownException, "sqlException != null");
            Assert.IsTrue(thrownException.ErrorCode == -2146232060, "Timeout exception should occur");
        }

        #endregion SearchMetaData

        private static IItemSearchRepository CreateFullTextSearchRepository<T>(
            FullTextSearchCriteria searchCriteria, 
            ICollection<T> queryResult, 
            ICollection<int?> queryResult2 = null)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();
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
                connectionWrapper.SetupQueryAsync("SearchFullTextByItemTypes",
                    new Dictionary<string, object>
                    {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "page", Page },
                    { "pageSize", PageSize },
                    { "maxItems", MaxItems },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds) }
                    },
                    queryResult);
                connectionWrapper.SetupQueryMultipleAsync("SearchFullTextByItemTypesMetaData",
                    new Dictionary<string, object>
                    {
                    { "predefineds", SqlItemSearchRepository.Predefineds },
                    { "itemTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ItemTypeIds) }
                    },
                    new Tuple<IEnumerable<T>, IEnumerable<int?>>(queryResult, queryResult2));
            }

            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToStringInvariant());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToStringInvariant());

            return new SqlItemSearchRepository(connectionWrapper.Object, configuration.Object);
        }

        private static IItemSearchRepository CreateFullTextSearchRepositoryWithException<T>(Exception exception)
        {
            var connectionWrapper = new Mock<ISqlConnectionWrapper>();
            
            connectionWrapper.Setup(
                t => t.QueryMultipleAsync<T, int?>("SearchFullText", It.IsAny<object>(), It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(), It.IsAny<CommandType?>())).Throws(exception);
            connectionWrapper.Setup(
                t => t.QueryMultipleAsync<T, int?>("SearchFullTextMetaData", It.IsAny<object>(), It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(), It.IsAny<CommandType?>())).Throws(exception);

            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToStringInvariant());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToStringInvariant());

            return new SqlItemSearchRepository(connectionWrapper.Object, configuration.Object);
        }

        private static IItemSearchRepository CreateItemNameRepository<T>(
            ItemNameSearchCriteria searchCriteria,
            ICollection<T> queryResult,
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISqlArtifactRepository artifactRepository)
        {
            var connectionWrapper = new SqlConnectionWrapperMock();
            var parameters = new Dictionary<string, object>
            {
                {"userId", UserId},
                {"query", searchCriteria.Query},
                {"projectIds", SqlConnectionWrapper.ToDataTable(searchCriteria.ProjectIds)},
                {"maxSearchableValueStringSize", MaxSearchableValueStringSize},
                {"startOffset", StartOffset},
                {"pageSize", PageSize},
                {"excludedPredefineds", SqlConnectionWrapper.ToDataTable(SqlItemSearchRepository.GetExcludedPredefineds(searchCriteria))}
            };

            if (searchCriteria.PredefinedTypeIds != null)
            {
                parameters.Add("predefinedTypeIds", SqlConnectionWrapper.ToDataTable(searchCriteria.PredefinedTypeIds));
            }

            connectionWrapper.SetupQueryAsync(
                "SearchItemNameByItemTypes",
                parameters,
                queryResult);

            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToStringInvariant());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToStringInvariant());

            return new SqlItemSearchRepository(connectionWrapper.Object, configuration.Object, artifactPermissionsRepository, artifactRepository);
        }

        private static IItemSearchRepository CreateItemNameRepositoryWithExceptionExpectation<T>(
            
            IArtifactPermissionsRepository artifactPermissionsRepository,
            ISqlArtifactRepository artifactRepository,
            Exception exception)
        {
            var connectionWrapper = new Mock<ISqlConnectionWrapper>();
            connectionWrapper.Setup(
                t => t.QueryAsync<T>("SearchItemNameByItemTypes", It.IsAny<object>(), It.IsAny<IDbTransaction>(),
                    It.IsAny<int?>(), It.IsAny<CommandType?>())).Throws(exception);

            var configuration = new Mock<ISearchConfiguration>();
            configuration.Setup(c => c.MaxItems).Returns(MaxItems.ToStringInvariant());
            configuration.Setup(c => c.MaxSearchableValueStringSize).Returns(MaxSearchableValueStringSize.ToStringInvariant());

            return new SqlItemSearchRepository(connectionWrapper.Object, configuration.Object, artifactPermissionsRepository, artifactRepository);
        }
    }

    public class SqlExceptionCreator
    {
        private static T Construct<T>(params object[] p)
        {
            var ctors = typeof(T).GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance);
            return (T)ctors.First(ctor => ctor.GetParameters().Length == p.Length).Invoke(p);
        }

        internal static SqlException NewSqlException(int number = 1)
        {
            SqlErrorCollection collection = Construct<SqlErrorCollection>();
            SqlError error = Construct<SqlError>(number, (byte)2, (byte)3, "server name", "error message", "proc", 100);

            typeof(SqlErrorCollection)
                .GetMethod("Add", BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(collection, new object[] { error });


            return typeof(SqlException)
                .GetMethod("CreateException", BindingFlags.NonPublic | BindingFlags.Static,
                    null,
                    CallingConventions.ExplicitThis,
                    new[] { typeof(SqlErrorCollection), typeof(string) },
                    new ParameterModifier[] { })
                .Invoke(null, new object[] { collection, "7.0.0" }) as SqlException;
        }
    }
}
