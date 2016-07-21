using Helper;
using Model;
using NUnit.Framework;
using CustomAttributes;
using TestCommon;
using Model.ArtifactModel;
using Model.Factories;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;
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
        [Description("Create manual trace between 2 artifacts, delete the source artifact, get relationships.  Verify 404 Not Found is returned.")]
        public void GetRelationships_DeleteSourceArtifact_404NotFound()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            sourceArtifact.Delete(_user);
            sourceArtifact.Publish(_user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return a 404 Not Found when given a deleted artifact.");
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

        [TestCase(false, 0), Explicit(IgnoreReasons.ProductBug)]   // Trello bug: https://trello.com/c/K2Nnx5im/1321-get-relationships-returns-400-for-sub-artifactid-of-0-but-404-for-artifactid-of-0-they-should-be-consistent
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

        // TODO: Fix this test.
        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopment)]  // XXX: Complains about Artifact Ids being different.
        [TestRail(153700)]
        [Description("Create manual trace between an artifact and a sub-artifact.  Get relationships.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceArtifactToSubartifact_ReturnsCorrectTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);
            IProcess process = Helper.Storyteller.GetProcess(_user, targetArtifact.Id);
            var userTasks = process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 0, "No User Tasks were found!");

            int subArtifactId = userTasks[0].Id;

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user, subArtifactId: subArtifactId);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, targetArtifact, subArtifactId);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(2, relationships.OtherTraces.Count, "There should be 2 'other' traces.");

//            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);     // XXX: This complains about Artifact Ids being different.

            ITrace trace1 = traces[0];
            ITrace trace2 = relationships.OtherTraces[1];   // XXX: I'm not sure what this 'Other' trace is, but it's the only one that matches.

            Assert.AreEqual(trace1.ProjectId, trace2.ProjectId, "The Project IDs of the traces don't match!");
            Assert.AreEqual(trace1.ArtifactId, trace2.ArtifactId, "The Artifact IDs of the traces don't match!");
            Assert.AreEqual(trace1.Direction, trace2.Direction, "The Trace Directions don't match!");
//            Assert.AreEqual(trace1.TraceType, trace2.TraceType, "The Trace Types don't match!");
            Assert.AreEqual(trace1.IsSuspect, trace2.IsSuspect, "One trace is marked suspect but the other isn't!");
        }

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopment)]  // XXX: Is it possible to create Traces between two sub-artifacts with OpenAPI?
        [TestRail(153741)]
        [Description("Create manual trace between two sub-artifacts.  Get relationships.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceBetweenTwoSubArtifacts_ReturnsCorrectTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);
            IArtifact targetArtifact = Helper.Storyteller.CreateAndPublishProcessArtifact(_project, _user);

            IProcess sourceProcess = Helper.Storyteller.GetProcess(_user, targetArtifact.Id);
            var sourceUserTasks = sourceProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(sourceUserTasks.Count > 0, "No User Tasks were found in the source Process!");

            IProcess targetProcess = Helper.Storyteller.GetProcess(_user, targetArtifact.Id);
            var targetUserTasks = targetProcess.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(targetUserTasks.Count > 0, "No User Tasks were found in the target Process!");

            int sourceSubArtifactId = sourceUserTasks[0].Id;    // TODO: Is it possible to create Traces between two sub-artifacts with OpenAPI?
//            int targetSubArtifactId = targetUserTasks[0].Id;

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user, subArtifactId: sourceSubArtifactId);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, targetArtifact, sourceSubArtifactId);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(2, relationships.OtherTraces.Count, "There should be 2 'other' traces.");
            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);     // XXX: This complains about Direction being different.
        }

        [TestCase(0)]
        [TestCase(int.MaxValue)]
        [TestRail(153840)]
        [Description("Try to Get Relationships for an artifact ID that doesn't exist.  Verify 404 Not Found is returned.")]
        public void GetRelationships_InvalidArtifactId_404NotFound(int fakeArtifactId)
        {
            // Setup:
            // Hack: Create a fake artifact to wrap the subArtifact ID.
            var sourceArtifact = ArtifactFactory.CreateArtifact(_project, _user, BaseArtifactType.Actor, fakeArtifactId);

            // Verify the artifact doesn't exist.
            Assert.Throws<Http404NotFoundException>(() => OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, sourceArtifact.Id, _user),
                "An artifact with ID: {0} was found, but it shouldn't exist!", sourceArtifact.Id);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return 404 Not Found if the artifact ID doesn't exist.");
        }

        [TestCase(0), Explicit(IgnoreReasons.ProductBug)]   // Trello bug: https://trello.com/c/K2Nnx5im/1321-get-relationships-returns-400-for-sub-artifactid-of-0-but-404-for-artifactid-of-0-they-should-be-consistent
        [TestCase(int.MaxValue)]
        [TestRail(153841)]
        [Description("Try to Get Relationships for a sub-artifact ID that doesn't exist.  Verify 404 Not Found is returned.")]
        public void GetRelationships_InvalidSubArtifactId_404NotFound(int fakeSubArtifactId)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Verify the artifact doesn't exist.
            Assert.Throws<Http404NotFoundException>(() => OpenApiArtifact.GetArtifact(Helper.BlueprintServer.Address, _project, fakeSubArtifactId, _user),
                "A sub-artifact with ID: {0} was found, but it shouldn't exist!", fakeSubArtifactId);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, subArtifactId: fakeSubArtifactId);
            }, "GetArtifactRelationships should return 404 Not Found if the sub-artifact ID doesn't exist.");
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(153842)]
        [Description("Try to Get Relationships for an unpublished artifact ID that was created by a different user.  Verify 404 Not Found is returned.")]
        public void GetRelationships_UnpublishedArtifactByOtherUser_404NotFound(bool addDrafts)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            sourceArtifact.Save();
            targetArtifact.Save();

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user);

            Assert.That(traces.Count > 0, "No traces were added!");

            IUser user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(user2, sourceArtifact, addDrafts: addDrafts);
            }, "GetArtifactRelationships should return 404 Not Found for unpublished artifacts created by different users.");
        }

        [TestCase]
        [TestRail(153843)]
        [Description("Try to Get Relationships for an artifact ID that has multiple traces.  Verify all traces are returned.")]
        public void GetRelationships_ArtifactWithMultipleTraces_ReturnsAllTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            IArtifact thirdArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user);
            Assert.AreEqual(1, traces.Count, "No traces were added!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                thirdArtifact, TraceDirection.From, _user));
            Assert.AreEqual(2, traces.Count, "No traces were added!");

            Relationships relationships = null;

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return 404 Not Found for unpublished artifacts created by different users.");

            Assert.AreEqual(2, relationships.ManualTraces.Count, "There should be 2 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
            AssertTracesAreEqual(traces[1], relationships.ManualTraces[1]);
        }

        [TestCase]
        [TestRail(153846)]
        [Description("Try to Get Relationships for an artifact ID that has no traces.  Verify no traces are returned.")]
        public void GetRelationships_ArtifactWithNoTraces_ReturnsNoTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Relationships relationships = null;

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return 404 Not Found for unpublished artifacts created by different users.");

            Assert.AreEqual(0, relationships.ManualTraces.Count, "There should be 0 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
        }

        // TODO: Test with "Other" traces.
        // TODO: Test with 2 users; user1 creates artifacts & traces; user2 only has permission to see one of the artifacts and tries to GetRelationships for each artifact.
        // TODO: Test with deleted subArtifact that has a trace to it.
        // TODO: Try to create a cyclic Trace dependency and GetRelationships...
        // TODO: Test with a 'Both' Trace direction.
    }
}
