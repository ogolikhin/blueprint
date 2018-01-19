﻿using System.Net.Http;
using ArtifactStore.Services.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SearchEngineLibrary.Service;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private const int UserId = 1;
        private Session _session;

        private Mock<ICollectionsService> _collectionsServiceMock;
        private Mock<ISearchEngineService> _mockSearchEngineService;
        private CollectionsController _collectionsController;

        [TestInitialize]
        public void Initialize()
        {
            _session = new Session { UserId = UserId };

            _collectionsServiceMock = new Mock<ICollectionsService>();
            _mockSearchEngineService = new Mock<ISearchEngineService>();

            _collectionsController = new CollectionsController(_collectionsServiceMock.Object, _mockSearchEngineService.Object)
            {
                Request = new HttpRequestMessage()
            };

            _collectionsController.Request.Properties[ServiceConstants.SessionProperty] = _session;
        }
    }
}