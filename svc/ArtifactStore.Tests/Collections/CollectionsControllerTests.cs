﻿using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http.Results;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.Collections.Models;
using ArtifactStore.Models.Review;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Exceptions;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

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
        private AddArtifactsToCollectionResult _addArtifactsResult;
        private RemoveArtifactsFromCollectionResult _removeArtifactsFromCollectionResult;
        private Pagination _pagination;
        private CollectionArtifacts _expectedCollectionArtifacts;
        private GetColumnsDto _columns;
        private ProfileColumnsDto _profileColumnsDto;


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

            _artifactIds = new HashSet<int> { 1, 2, 3 };

            _collectionId = 1;
            _addArtifactsResult = new AddArtifactsToCollectionResult
            {
                AddedCount = 1,
                Total = 1
            };

            _removeArtifactsFromCollectionResult = new RemoveArtifactsFromCollectionResult
            {
                RemovedCount = 1,
                Total = 3
            };

            _profileColumnsDto = new ProfileColumnsDto
            {
                Items = new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.CustomGroup, PropertyPrimitiveType.Text, 2)
                }
            };

            _expectedCollectionArtifacts = new CollectionArtifacts
            {
                ItemsCount = 2,
                ArtifactListSettings = new ArtifactListSettings
                {
                    Columns = new List<ProfileColumn>
                    {
                        new ProfileColumn
                        {
                            Predefined = PropertyTypePredefined.Name,
                            PrimitiveType = PropertyPrimitiveType.Text,
                            PropertyName = "Name",
                            PropertyTypeId = 80
                        },
                        new ProfileColumn
                        {
                            Predefined = PropertyTypePredefined.Description,
                            PrimitiveType = PropertyPrimitiveType.Text,
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

            _columns = new GetColumnsDto
            {
                SelectedColumns = new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.Name,  PropertyPrimitiveType.Number, 3)
                },
                UnselectedColumns = new List<ProfileColumn>
                {
                    new ProfileColumn("Custom", PropertyTypePredefined.Name,  PropertyPrimitiveType.Number, 3)
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

            var result = await _collectionsController.AddArtifactsToCollectionAsync(_collectionId, "add", _artifactIds) as OkNegotiatedContentResult<AddArtifactsToCollectionResult>;

            Assert.IsNotNull(result);
            Assert.AreEqual(_addArtifactsResult, result.Content);
        }

        #endregion AddArtifactsToCollectionAsync

        #region RemoveArtifactsFromCollectionAsync

        [TestMethod]
        public async Task RemoveArtifactsFromCollectionAsync_AllParametersAreValid_Success()
        {
            var removalParameters =
                new ReviewItemsRemovalParams
                {
                    ItemIds = new List<int> { 1, 2, 3 },
                    SelectionType = SelectionType.Selected
                };

            _collectionsServiceMock.Setup(svc => svc.RemoveArtifactsFromCollectionAsync(_collectionId, removalParameters, _sessionUserId)).ReturnsAsync(_removeArtifactsFromCollectionResult);

            var result = await _collectionsController.RemoveArtifactsFromCollectionAsync(_collectionId, "remove", removalParameters) as OkNegotiatedContentResult<RemoveArtifactsFromCollectionResult>;

            Assert.IsNotNull(result);
            Assert.AreEqual(_removeArtifactsFromCollectionResult, result.Content);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidItemIds_ItemIdsIsNull_BadRequestException()
        {
            var removalParameters =
                new ReviewItemsRemovalParams
                {
                    ItemIds = null,
                    SelectionType = SelectionType.Selected
                };

            _collectionsServiceMock.Setup(svc => svc.RemoveArtifactsFromCollectionAsync(_collectionId, removalParameters, _sessionUserId)).ReturnsAsync(_removeArtifactsFromCollectionResult);

            var result = await _collectionsController.RemoveArtifactsFromCollectionAsync(_collectionId, "remove", removalParameters) as OkNegotiatedContentResult<RemoveArtifactsFromCollectionResult>;
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_InvalidItemIds_ItemIdsIsEmpty_BadRequestException()
        {
            var removalParameters =
                new ReviewItemsRemovalParams
                {
                    ItemIds = new List<int>(),
                    SelectionType = SelectionType.Selected
                };

            _collectionsServiceMock.Setup(svc => svc.RemoveArtifactsFromCollectionAsync(_collectionId, removalParameters, _sessionUserId)).ReturnsAsync(_removeArtifactsFromCollectionResult);

            var result = await _collectionsController.RemoveArtifactsFromCollectionAsync(_collectionId, "remove", removalParameters) as OkNegotiatedContentResult<RemoveArtifactsFromCollectionResult>;
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task RemoveArtifactsFromCollectionAsync_ReviewItemsRemovalParamsIsNull_BadRequestException()
        {
            ReviewItemsRemovalParams removalParameters = null;

            _collectionsServiceMock.Setup(svc => svc.RemoveArtifactsFromCollectionAsync(_collectionId, removalParameters, _sessionUserId)).ReturnsAsync(_removeArtifactsFromCollectionResult);

            var result = await _collectionsController.RemoveArtifactsFromCollectionAsync(_collectionId, "remove", removalParameters) as OkNegotiatedContentResult<RemoveArtifactsFromCollectionResult>;
        }
        #endregion

        #region SaveColumnsSettingsAsync

        [TestMethod]
        public async Task SaveColumnsSettingsAsync_AllParametersAreValid_Success()
        {
            var result = await _collectionsController.SaveColumnsSettingsAsync(_collectionId, _profileColumnsDto);

            Assert.IsNotNull(result);
            Assert.AreEqual(result.StatusCode, HttpStatusCode.NoContent);
        }

        [TestMethod]
        [ExpectedException(typeof(BadRequestException))]
        public async Task SaveColumnsSettingsAsync_EmptyItems_ThrowsException()
        {
            _profileColumnsDto.Items = null;
            await _collectionsController.SaveColumnsSettingsAsync(_collectionId, _profileColumnsDto);
        }

        #endregion SaveColumnsSettingsAsync

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

        #region GetColumnsAsync

        [TestMethod]
        public async Task GetColumnsAsync_AllParametersAreValid_Success()
        {
            // Arrange

            _collectionsServiceMock.Setup(q => q.GetColumnsAsync(_collectionId, _sessionUserId, null))
                .ReturnsAsync(_columns);

            // act
            var actualResult =
                await _collectionsController.GetColumnsAsync(_collectionId, null) as OkNegotiatedContentResult<GetColumnsDto>;

            // assert
            Assert.IsNotNull(actualResult);
            Assert.AreEqual(_columns, actualResult.Content);
            Assert.AreEqual(_columns.UnselectedColumns.Count(), actualResult.Content.UnselectedColumns.Count());
            Assert.AreEqual(_columns.SelectedColumns.Count(), actualResult.Content.SelectedColumns.Count());
        }

        #endregion GetColumnsAsync
    }
}
