using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using NUnit.Framework;
using System.Collections.Generic;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class RelationshipsTests : TestBase
    {
        private IUser _user = null;
        private IUser _userWithLimitedAccess = null;
        private IProject _project = null;
        private IGroup _authorsGroup = null;

        private IProjectRole _viewerRole = null;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();

            _authorsGroup = Helper.CreateGroupAndAddToDatabase();

            _userWithLimitedAccess = Helper.CreateUserAndAddToDatabase(instanceAdminRole: null);
            _authorsGroup.AddUser(_userWithLimitedAccess);

            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);

            _viewerRole = ProjectRoleFactory.GetDeployedProjectRole(ProjectRoleFactory.DeployedProjectRole.Viewer);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Private Functions

        /// <summary>
        /// Compares two Trace objects and asserts that each of their properties are equal.
        /// </summary>
        /// <param name="trace1">The first Trace to compare.</param>
        /// <param name="trace2">The second Trace to compare.</param>
        /// <param name="checkDirection">(optional) Pass false if you don't want to compare the Direction properties of the traces.</param>
        private static void AssertTracesAreEqual(ITrace trace1, ITrace trace2, bool checkDirection=true)
        {
            Assert.AreEqual(trace1.ProjectId, trace2.ProjectId, "The Project IDs of the traces don't match!");
            Assert.AreEqual(trace1.ArtifactId, trace2.ArtifactId, "The Artifact IDs of the traces don't match!");
            Assert.AreEqual(trace1.TraceType, trace2.TraceType, "The Trace Types don't match!");
            Assert.AreEqual(trace1.IsSuspect, trace2.IsSuspect, "One trace is marked suspect but the other isn't!");

            if (checkDirection)
            {
                Assert.AreEqual(trace1.Direction, trace2.Direction, "The Trace Directions don't match!");
            }
        }

        /// <summary>
        /// Compares traces with traces from relationship so that each of their properties are equal.
        /// </summary>
        /// <param name="traces">traces to compare.</param>
        /// <param name="relationship">relationship to compare.</param>
        /// <param name="targetArtifacts">target artifacts to compare</param>
        private static void TraceValidation(List<OpenApiTrace> traces, Relationships relationship, List<IArtifact> targetArtifacts)
        {
            var totalTraceCountFromTraces = traces.Count;
            var totalTraceCountFromRelationship = relationship.ManualTraces.Count + relationship.OtherTraces.Count;

            Assert.AreEqual(0, relationship.OtherTraces.Count, "Relationships shouldn't have other traces.");
            Assert.That(targetArtifacts.Count.Equals(totalTraceCountFromRelationship), "Total number of target artifacts should equal to total number of relationships");
            Assert.That(totalTraceCountFromTraces.Equals(totalTraceCountFromRelationship), "Total number of traces to compare is {0} but relationship contains {1} traces", totalTraceCountFromTraces, totalTraceCountFromRelationship);

            for (int i = 0; i < totalTraceCountFromTraces; i++)
            {
                var manualTraceId = relationship.ManualTraces[i].ArtifactId;
                IArtifact foundArtifact = null;
                Assert.NotNull(foundArtifact = targetArtifacts.Find(a => a.Id.Equals(manualTraceId)),"Could not find matching arifact from artifacts {0}", targetArtifacts);
                var foundArtifactName = foundArtifact.Name;
                AssertTracesAreEqual(traces[i], relationship.ManualTraces[i]);
                Assert.That(relationship.ManualTraces[i].ArtifactName.Equals(foundArtifactName), "Name '{0}' from target artifact does not match with Name '{1}' from manual trace of relationships.", foundArtifactName, relationship.ManualTraces[i].ArtifactName);
            }
        }

        #endregion Private Functions

        #region 200 OK Tests

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183545)]
        [Description("Create and publish artifact with a trace to target. Update and publish the artifact with the updated trace pointing to another target. Verify that GetRelationship call returns correct trace for each version of artifact")]
        public void GetRelationships_ChangeTraceWhenPublishingArtifacts_ReturnsCorrectRelationshipPerVersion(TraceDirection direction)
        {
            // Setup: Create and Publish Two target artifacts: target artifact 1 and target artifact 2
            // Create and publish artifact with outgoing trace to target artifact 1
            // Update and publish the same artifact with outgoing tract to target artifact 2
            var bpServerAddress = Helper.BlueprintServer.Address;
            var targetArtifact1 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UIMockup);

            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var tracesV1 = OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact1, direction, _user);
            sourceArtifact.Publish(); //creation of first version

            OpenApiArtifact.DeleteTrace(bpServerAddress, sourceArtifact, targetArtifact1, direction, _user);
            var tracesV2 = OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact2, direction, _user);
            sourceArtifact.Publish(); //creation of second version

            // Execute: Execute GetRelationship for the available versions of the source artifact
            Relationships relationshipsV1 = null;
            Assert.DoesNotThrow(() =>
            {
                relationshipsV1 = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: 1);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            Relationships relationshipsV2 = null;
            Assert.DoesNotThrow(() =>
            {
                relationshipsV2 = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: 2);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Validation: Compare properties between returned GetRelationship and traces for each version
            TraceValidation(tracesV1, relationshipsV1, new List<IArtifact> { targetArtifact1 });
            TraceValidation(tracesV2, relationshipsV2, new List<IArtifact> { targetArtifact2 });
        }

        #endregion 200 OK Tests

        // TODO: Sort existing test cases inside of this file based on test type e.g. 200 OK test etc..

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(153694)]
        [Description("Create manual trace between 2 artifacts, get relationships.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceDirection_ReturnsCorrectTraces(TraceDirection direction)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, direction, _user);

            Assert.AreEqual(false, traces[0].IsSuspect,
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

        [TestCase]
        [TestRail(154426)]
        [Description("Create manual trace between an artifact and a sub-artifact, delete the sub-artifact.  Get relationships.  Verify no traces are returned.")]
        public void GetRelationships_DeleteSubArtifact_ReturnsNoTraces()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IProcess process = StorytellerTestHelper.CreateAndGetDefaultProcessWithTwoSequentialUserTasks(Helper.Storyteller, _project, _user);

            var userTasks = process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 1, "There should be more than one User Task!");

            Assert.AreEqual(1, Helper.Storyteller.Artifacts.Count, "There should only be 1 Process artifact in Storyteller!");
            IArtifact targetArtifact = Helper.Storyteller.Artifacts[0];

            int subArtifactId = userTasks[0].Id;

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user, subArtifactId: subArtifactId);

            // Publish the Trace we added.
            sourceArtifact.Publish();

            // Delete the first User Task and publish.
            process.DeleteUserAndSystemTask(userTasks[0]);
            StorytellerTestHelper.UpdateVerifyAndPublishProcess(process, Helper.Storyteller, _user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be no 'other' traces.");
        }

        [TestCase(true)]
        [TestCase(null)]
        [TestRail(153703)]
        [Description("Create manual trace between 2 Saved (but unpublished) artifacts, get relationships (with and without the 'addDrafts=true' query parameter).  Verify no traces are returned.")]
        public void GetRelationships_SavedNeverPublishedArtifactWithAddDraftsTrue_ReturnsCorrectTraces(bool? addDrafts)
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
            }, "GetArtifactRelationships shouldn't throw any error when given a valid Unpublished Draft artifact and {0} addDrafts=true.",
                addDrafts.HasValue ? "with" : "without");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have any other traces.");

            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
        }

        [TestCase]
        [TestRail(153904)]
        [Description("Create manual trace between 2 Saved (but unpublished) artifacts, get relationships (with the 'addDrafts=false' query parameter).  Verify it returns 404 Not Found.")]
        public void GetRelationships_SavedNeverPublishedArtifactWithAddDraftsFalse_404NotFound()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            IArtifact targetArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            sourceArtifact.Save(_user);
            targetArtifact.Save(_user);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, addDrafts: false);
            }, "GetArtifactRelationships should return 404 Not Found when given a valid Unpublished Draft artifact and addDrafts=false.");
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

            Assert.AreEqual(false, traces[0].IsSuspect,
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

        [TestCase(0)]
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
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with multiple traces.");

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
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with no traces.");

            Assert.AreEqual(0, relationships.ManualTraces.Count, "There should be 0 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
        }

        [TestCase]
        [TestRail(153909)]
        [Description("Try to Get Relationships for an artifact ID that has that are in a dependency loop.  Verify all traces for the artifact are returned.")]
        public void GetRelationships_CyclicTraceDependency_ReturnsAllTraces()
        {
            // Setup:
            IArtifact firstArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact secondArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            IArtifact thirdArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, firstArtifact,
                secondArtifact, TraceDirection.To, _user);
            Assert.AreEqual(1, traces.Count, "No traces were added between first & second artifact!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, secondArtifact,
                thirdArtifact, TraceDirection.To, _user));
            Assert.AreEqual(2, traces.Count, "No traces were added between second & third artifact!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, thirdArtifact,
                firstArtifact, TraceDirection.To, _user));
            Assert.AreEqual(3, traces.Count, "No traces were added between third & first artifact!");

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, firstArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with a cyclic trace dependency.");

            // Verify:
            Assert.AreEqual(2, relationships.ManualTraces.Count, "There should be 2 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
            AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
            AssertTracesAreEqual(traces[1], relationships.ManualTraces[1], checkDirection: false);
            Assert.AreEqual(TraceDirection.From, relationships.ManualTraces[1].Direction,
                "The 2nd manual trace should be 'From' the third artifact!");
        }

        [TestCase]
        [TestRail(154699)]
        [Description("Try to get relationships using credentials of user which has no access to the target artifact. Verify that relationships returns empty artifact name and HasAccess false.")]
        public void GetRelationships_NoAccessToTargetArtifact_ReturnsCorrectRelationships()
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            _authorsGroup.AssignRoleToProjectOrArtifact(_project, _viewerRole, sourceArtifact);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.From, _user);
            targetArtifact.Publish(_user);

            Assert.AreEqual(false, traces[0].IsSuspect,
                "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");
            Helper.AdminStore.AddSession(_userWithLimitedAccess);
            
            Relationships relationshipsForUserWithFullAccessToTargetArtifact = null;
            Relationships relationshipsForUserWithNoAccessToTargetArtifact = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationshipsForUserWithNoAccessToTargetArtifact = Helper.ArtifactStore.GetRelationships(_userWithLimitedAccess, sourceArtifact);
                relationshipsForUserWithFullAccessToTargetArtifact = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            AssertTracesAreEqual(traces[0], relationshipsForUserWithFullAccessToTargetArtifact.ManualTraces[0]);

            Assert.AreEqual(1, relationshipsForUserWithNoAccessToTargetArtifact.ManualTraces.Count, "There should be 1 manual trace!");
            var trace = relationshipsForUserWithNoAccessToTargetArtifact.ManualTraces[0];
            Assert.AreEqual(targetArtifact.Id, trace.ArtifactId, "Returned trace must have proper artifactId.");
            Assert.IsFalse(trace.HasAccess, "User with no access rights should have no access to the target artifact.");
            Assert.IsNull(trace.ArtifactName, "User with no access rights should receive empty target artifact name.");
            Assert.AreEqual(false, trace.IsSuspect, "Returned trace mustn't be suspected.");
            Assert.AreEqual(traces[0].TraceType, trace.TraceType, "Returned trace must have proper TraceType.");
            Assert.AreEqual(traces[0].Direction, trace.Direction, "Returned trace must have proper Direction.");
        }

        [TestCase]
        [TestRail(154763)]
        [Description("Get relationshipsdetails for artifact, check that artifact path has expected value.")]
        public void GetRelationshipsDetails_ManualTrace_ReturnsCorrectTraceDetails()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            TraceDetails traceDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                traceDetails = Helper.ArtifactStore.GetRelationshipsDetails(_user, artifact);
            }, "GetRelationshipsDetails shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(2, traceDetails.PathToProject.Count, "PathToProject must have 2 items.");
            Assert.AreEqual(artifact.Id, traceDetails.ArtifactId, "Id must be correct.");
        }

        [TestCase]
        [TestRail(154764)]
        [Description("Get relationshipsdetails for artifact which is child of other artifact, check that artifact path has expected value.")]
        public void GetRelationshipsDetails_ManualTraceLongPath_ReturnsCorrectTraceDetails()
        {
            // Setup:
            IArtifact parentArtifact = null;
            for (int i = 0; i < 3; i++)
            {
                parentArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase, parentArtifact);
            }
            IArtifact artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor, parent: parentArtifact);

            TraceDetails traceDetails = null;
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                traceDetails = Helper.ArtifactStore.GetRelationshipsDetails(_user, artifact);
            }, "GetRelationshipsDetails shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(5, traceDetails.PathToProject.Count, "PathToProject must have 5 items.");
            Assert.AreEqual(artifact.Id, traceDetails.ArtifactId, "Id must be correct.");
        }

        // TODO: Test with "Other" traces.
        // TODO: Test with 2 users; user1 creates artifacts & traces; user2 only has permission to see one of the artifacts and tries to GetRelationships for each artifact.
    }
}
