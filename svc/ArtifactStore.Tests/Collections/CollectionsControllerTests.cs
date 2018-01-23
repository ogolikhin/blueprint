using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using ArtifactStore.Collections.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using System.Linq;

namespace ArtifactStore.Collections
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private int _userId = 1;

        private Mock<ICollectionsService> _collectionsServiceMock;
        private CollectionsController _collectionsController;
        private Session _session;
        private int _sessionUserId = 1;
        private ISet<int> _artifactIds;
        private int _collectionId;
        private AddArtifactsResult _addArtifactsResult;
        private Pagination _pagination;
        private CollectionArtifacts _expectedCollectionArtifacts;


        [TestInitialize]
        public void Initialize()
        {
            _userId = 1;
            _session = new Session { UserId = _userId };
            _pagination = new Pagination { Limit = int.MaxValue, Offset = 0 };

            _collectionsServiceMock = new Mock<ICollectionsService>();

            _collectionsController = new CollectionsController(
                _collectionsServiceMock.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;

            _artifactIds = new HashSet<int>() { 1, 2, 3 };

            _collectionId = 1;
            _addArtifactsResult = new AddArtifactsResult()
            {
                AddedCount = 1,
                Total = 1
            };

            _expectedCollectionArtifacts = new CollectionArtifacts
            {
                ItemsCount = 2,
                ArtifactListSettings = new ArtifactListSettings
                {
                    Columns = new List<ArtifactListColumn>
                    {
                        new ArtifactListColumn
                        {
                            Predefined = 4098,
                            PrimitiveType = 0,
                            PropertyName = "Name",
                            PropertyTypeId = 80
                        },
                        new ArtifactListColumn
                        {
                            Predefined = 4099,
                            PrimitiveType = 0,
                            PropertyName = "Description",
                            PropertyTypeId = 81
                        }
                    },
                    Filters = new List<ArtifactListFilter>()
                },
                Items = new List<ArtifactDto>
                {
                    new ArtifactDto
                    {
                        ArtifactId = 7545,
                        ItemTypeId = 134,
                        PredefinedType = 4107,
                        PropertyInfos = new List<PropertyValueInfo>
                        {
                            new PropertyValueInfo
                            {
                                PropertyTypeId = 80,
                                Value = "Value_Name"
                            },
                            new PropertyValueInfo
                            {
                                PropertyTypeId = 81,
                                Value = "Value_Description"
                            }
                        }
                    },
                    new ArtifactDto
                    {
                        ArtifactId = 7551,
                        ItemTypeId = 132,
                        PredefinedType = 4105,
                        PropertyInfos = new List<PropertyValueInfo>
                        {
                            new PropertyValueInfo
                            {
                                PropertyTypeId = 80,
                                Value = "Value_Name_2"
                            },
                            new PropertyValueInfo
                            {
                                PropertyTypeId = 81,
                                Value = "Value_Description_2"
                            }
                        }
                    }
                }
            };
        }

        #region AddArtifactsToCollectionAsync

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddArtifactsToCollectionAsync_InvalidScope_ThrowsException()
        {

            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            _artifactIds = null;

            await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task AddArtifactsToCollectionAsync_EmptyScope_ThrowsException()
        {

            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            _artifactIds = new HashSet<int>();

            await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds);
        }

        [TestMethod]
        public async Task AddArtifactsToCollectionAsync_AllParametersAreValid_Success()
        {
            _collectionsServiceMock.Setup(svc => svc.AddArtifactsToCollectionAsync(_collectionId, _artifactIds, _sessionUserId)).ReturnsAsync(_addArtifactsResult);

            var result = await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds) as OkNegotiatedContentResult<AddArtifactsResult>;

            Assert.IsNotNull(result);
            Assert.AreEqual(_addArtifactsResult, result.Content);
        }

        #endregion AddArtifactsToCollectionAsync

        #region GetArtifactsInCollectionAsync

        [TestMethod]
        public async Task GetArtifactsInCollectionAsync_AllParametersAreValid_Success()
        {
            // Arrange

            _collectionsServiceMock.Setup(q => q.GetArtifactsInCollectionAsync(_collectionId, _pagination, _sessionUserId))
                .ReturnsAsync(_expectedCollectionArtifacts);

            // act
            var actualResult =
                await _collectionsController.GetArtifactsInCollectionAsync(_collectionId,
                        _pagination) as OkNegotiatedContentResult<CollectionArtifacts>;

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_expectedCollectionArtifacts, actualResult.Content);
            Assert.AreEqual(_expectedCollectionArtifacts.ItemsCount, actualResult.Content.ItemsCount);
            Assert.AreEqual(_expectedCollectionArtifacts.Items.Count(), actualResult.Content.Items.Count());
        }

        #endregion GetArtifactsInCollectionAsync
    }
}