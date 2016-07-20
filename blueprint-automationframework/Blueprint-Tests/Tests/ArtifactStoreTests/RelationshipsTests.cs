using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Utilities;

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

        /// <summary>
        /// Compares two Trace objects and asserts that each of their properties are equal.
        /// </summary>
        /// <param name="trace1">The first Trace to compare.</param>
        /// <param name="trace2">The second Trace to compare.</param>
        private static void AssertTracesAreEqual(ITrace trace1, ITrace trace2)
        {
            Assert.AreEqual(trace1.ProjectId, trace2.ProjectId, "The Project IDs of the traces don't match!");
            Assert.AreEqual(trace1.ArtifactId, trace2.ArtifactId, "The Artifact IDs of the traces don't match!");
            Assert.AreEqual(trace1.Direction, trace2.Direction, "The Trace Directions don't match!");
            Assert.AreEqual(trace1.TraceType, trace2.TraceType, "The Trace Types don't match!");
            Assert.AreEqual(trace1.IsSuspect, trace2.IsSuspect, "One trace is marked suspect but the other isn't!");
        }

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestRail(153694)]
        [Description("Create manual trace between 2 artifacts, get relationships.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceDirection_ReturnsCorrectTraces(TraceDirection direction)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, direction, _user);

            Assert.AreEqual(traces[0].IsSuspect, false,
                "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(153698)]
        [Description("Create manual trace between 2 artifacts & set the trace as suspect, get relationships.  Verify returned trace has expected value.")]
        public void GetRelationships_ManualTraceHasSuspect_ReturnsCorrectTraces(bool suspected)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user, isSuspect: suspected);

            Assert.AreEqual(traces[0].IsSuspect, suspected,
                "IsSuspected should be {0} after adding a trace without specifying a value for isSuspect!", suspected);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
        }

        [TestCase]
        [TestRail(153702)]
        [Description("Create manual trace between 2 artifacts, delete the source artifact, get relationships.  Verify no traces are returned.")]
        public void GetRelationships_DeleteSourceArtifact_ReturnsNoTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            sourceArtifact.Delete(_user);
            sourceArtifact.Publish(_user);

            Relationships relationships = null;

            // Execute:
            // NOTE: This should probably return a 404, but they decided to keep the behavior the same as Silverlight.
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a deleted artifact.");

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships shouldn't have manual traces when the source artifact is deleted.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
        }

        [TestCase]
        [TestRail(153795)]
        [Description("Create manual trace between 2 artifacts, delete the target artifact, get relationships.  Verify no traces are returned.")]
        public void GetRelationships_DeleteTargetArtifact_ReturnsNoTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            targetArtifact.Delete(_user);
            targetArtifact.Publish(_user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships shouldn't have manual traces when the target artifact is deleted.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
        }

        [TestCase(false, 0)]
        [TestCase(true, 1)]
        [TestCase(null, 1)]
        [TestRail(153703)]
        [Description("Create manual trace between 2 Saved (but unpublished) artifacts, get relationships (with all possible values for the 'addDrafts' query parameter).  Verify no traces are returned.")]
        public void GetRelationships_SavedNeverPublishedArtifact_ReturnsCorrectTraces(bool? addDrafts, int expectedManualTracesCount)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            IArtifact targetArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            sourceArtifact.Save(_user);
            targetArtifact.Save(_user);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, addDrafts: addDrafts);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid Unpublished Draft artifact.");

            // Verify:
            Assert.AreEqual(expectedManualTracesCount, relationships.ManualTraces.Count,
                "Relationships should have {0} manual trace.", expectedManualTracesCount);
            Assert.AreEqual(0, relationships.OtherTraces.Count,
                "Relationships shouldn't have other traces.");

            if (expectedManualTracesCount > 0)
            {
                AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
            }
        }

        [TestCase]
        [TestRail(153691)]
        [Description("Create manual trace between 2 artifacts, get relationships with a user that doesn't have permission to the artifacts.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceUserHasNoAccessToTarget_403Forbidden()
        {
            // Setup:
            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.AccessControlToken, InstanceAdminRole.BlueprintAnalytics);

            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user);

            Assert.AreEqual(traces[0].IsSuspect, false,
                "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            // Execute & Verify:
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(user2, sourceArtifact);
            }, "GetArtifactRelationships should return 403 Forbidden if the user doesn't have permission to access the artifact.");
        }

    }
}
