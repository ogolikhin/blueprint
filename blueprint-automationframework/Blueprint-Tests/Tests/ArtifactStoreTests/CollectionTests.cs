using System;
using System.Collections.Generic;
using System.Net;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestCommon;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;
using System.Globalization;

namespace ArtifactStoreTests
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]    // Ignore for now.
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class CollectionTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _project.GetAllNovaArtifactTypes(Helper.ArtifactStore, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase()]
        [TestRail(1)]
        [Description(".")]
        public void CreateCollection_GetCollectionContent_Validate()
        {
            // Setup:
            //var authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var collectionFolder = _project.GetDefaultCollectionFolder(Helper.ArtifactStore.Address, _user);
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute:
            var collectionArtifact = Helper.CreateWrapAndSaveNovaArtifact(_project, _user,
                ItemTypePredefined.ArtifactCollection, collectionFolder.Id);
            Collection collection = null;
            Assert.DoesNotThrow(() =>
                collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id),
                "!");

            // Verify:
            collection.UpdateArtifacts(artifactsIdsToAdd: new List<int> { artifact.Id });
            var collectionArtifactBase = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Glossary, artifactId: collection.Id);
            Artifact.Lock(collectionArtifactBase, Helper.BlueprintServer.Address, _user);
            Artifact.UpdateArtifact(collectionArtifactBase, _user, collection);
            collection = Helper.ArtifactStore.GetCollection(_user, collectionArtifact.Id);
        }
    }
}