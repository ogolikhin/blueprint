using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using System.Collections.Generic;
using TestCommon;
using Utilities;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel.Impl;

namespace ArtifactStoreTests
{

    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class RelationshipsTests : TestBase
    {
        private IUser _user = null;
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase("To")]
        [TestCase("From")]
        [TestRail(153694)]
        [Description("Create manual trace between 2 artifacts, get relationships, check that returned trace has expected value")]
        public void GetRelationships_ManualTraceDirection_ReturnsCorrectTraces(string direction)
        {
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, direction, _user);
            Relationships relationships = null;

            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(sourceArtifact.Id, _user);
            }, "GetArtifactRelationships shouldn't throw any error.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces");
            Assert.IsTrue(traces[0].Equals(relationships.ManualTraces[0]), "Returned trace should have expected values");
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(153698)]
        [Description("Create manual trace between 2 artifacts, get relationships, check that returned trace has expected value")]
        public void GetRelationships_ManualTraceHasSuspect_ReturnsCorrectTraces(bool suspected)
        {
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, "To", user: _user, isSuspect: suspected);
            Relationships relationships = null;

            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(sourceArtifact.Id, _user);
            }, "GetArtifactRelationships shouldn't throw any error.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces");
            Assert.IsTrue(traces[0].Equals(relationships.ManualTraces[0]), "Returned trace should have expected values");
        }

        [TestCase]
        [TestRail(153702)]
        [Description("Create manual trace between 2 artifacts, delete 1 artifact, get relationships, check that no traces returned")]
        public void GetRelationships_DeletedArtifact_ReturnsNoTraces()
        {
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, "From", _user);
            sourceArtifact.Delete(_user);
            sourceArtifact.Publish(_user);

            Relationships relationships = null;

            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(sourceArtifact.Id, _user);
            }, "GetArtifactRelationships shouldn't throw any error.");
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships shouldn't have manual traces");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces");
        }

        [TestCase]
        [TestRail(153703)]
        [Description("Create manual trace between 2 artifacts, delete 1 artifact, get relationships, check that no traces returned")]
        public void GetRelationships_SavedNeverPublishedArtifact_ReturnsCorrectTraces()
        {
            IArtifact sourceArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            sourceArtifact.Save(_user);
            IArtifact targetArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            targetArtifact.Save(_user);
            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, "To", user: _user);
            Relationships relationships = null;

            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(sourceArtifact.Id, _user);
            }, "GetArtifactRelationships shouldn't throw any error.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces");
            Assert.IsTrue(traces[0].Equals(relationships.ManualTraces[0]), "Returned trace should have expected values");
        }
    }
}
