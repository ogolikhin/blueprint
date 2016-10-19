using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
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

        private const int INVALID_VERSIONID = -1;
        private const int NONEXSITING_VERSIONID = int.MaxValue;
        private const int INVALID_REVISIONID = -1;
        private const int NONEXSITING_REVISIONID = int.MaxValue;

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
        /// Validates traces with traces from relationship to verify their properties are equal.
        /// </summary>
        /// <param name="relationship">relationship to validate</param>
        /// <param name="traces">traces to compare with</param>
        /// <param name="artifacts">artifacts to compare with</param>
        private static void TraceValidation(Relationships relationship, List<OpenApiTrace> traces, List<IArtifact> artifacts)
        {
            var totalTraceCountFromTraces = traces.Count;
            var totalTraceCountFromRelationship = relationship.ManualTraces.Count + relationship.OtherTraces.Count;

            Assert.AreEqual(0, relationship.OtherTraces.Count, "Relationships shouldn't have other traces.");
            Assert.That(artifacts.Count.Equals(totalTraceCountFromRelationship), "Total number of target artifacts should equal to total number of relationships");
            Assert.That(totalTraceCountFromTraces.Equals(totalTraceCountFromRelationship), "Total number of traces to compare is {0} but relationship contains {1} traces", totalTraceCountFromTraces, totalTraceCountFromRelationship);

            for (int i = 0; i < totalTraceCountFromTraces; i++)
            {
                var manualTraceId = relationship.ManualTraces[i].ArtifactId;
                IArtifact foundArtifact = null;
                Assert.NotNull(foundArtifact = artifacts.Find(a => a.Id.Equals(manualTraceId)),"Could not find matching arifact from artifacts {0}", artifacts);
                var foundArtifactName = foundArtifact.Name;
                AssertTracesAreEqual(traces[i], relationship.ManualTraces[i]);
                Assert.That(relationship.ManualTraces[i].ArtifactName.Equals(foundArtifactName), "Name '{0}' from target artifact does not match with Name '{1}' from manual trace of relationships.", foundArtifactName, relationship.ManualTraces[i].ArtifactName);
            }
        }

        /// TODO: Refine this validation method to cover more trace details test cases
        /// <summary>
        /// Validate traceDetails with properties from artifact 
        /// </summary>
        /// <param name="traceDetails">trace details to validate</param>
        /// <param name="artifact">artifact to compare with</param>
        private static void TraceDetailsValidation(TraceDetails traceDetails, IArtifact artifact)
        {
            Assert.AreEqual(traceDetails.PathToProject.Count,2, "PathToProject must have 2 items.");
            Assert.AreEqual(traceDetails.ArtifactId,artifact.Id, "Artifact ID {0} from trace details must be equal to {1}.", traceDetails.ArtifactId, artifact.Id);
        }

        #endregion Private Functions

        #region 200 OK Tests

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183545)]
        [Description("Create and publish artifact with a trace to target. Update and publish the artifact with the updated trace pointing to another target. Verify that GetRelationship call returns correct trace for each version of artifact.")]
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

            // Validation: Validates trace properties from relationships for each version
            TraceValidation(relationshipsV1, tracesV1, new List<IArtifact> { targetArtifact1 });
            TraceValidation(relationshipsV2, tracesV2, new List<IArtifact> { targetArtifact2 });
        }

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183564)]
        [Description("Get relationshipsdetails with revision ID for artifact, check that artifact path has expected value.")]
        public void GetRelationshipsDetails_ManualTraceWithRevisionId_ReturnsCorrectTraceDetails(TraceDirection direction)
        {
            // Setup: Create and Publish Two target artifacts: target artifact
            // Create and publish artifact with outgoing trace to target artifact 
            var bpServerAddress = Helper.BlueprintServer.Address;
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var traces = OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish(); //creation of first version

            // GetRelationship for the available versions of the source artifact
            Relationships relationships = null;
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: 1);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Validates trace properties from relationships for the version
            TraceValidation(relationships, traces, new List<IArtifact> { targetArtifact });

            // Execute: Execute GetRelationshipDetails with the revision returned from GetRelationship call
            TraceDetails traceDetails = null;
            Assert.DoesNotThrow(() =>
            {
                traceDetails = ArtifactStore.GetRelationshipsDetails(bpServerAddress, _user, targetArtifact.Id, revisionId: relationships.RevisionId);
            }, "GetRelationshipsDetails shouldn't throw any error when given a valid artifact with valid revision ID.");

            // Validation: Validates trace details properties
            TraceDetailsValidation(traceDetails, targetArtifact);
        }

        #endregion 200 OK Tests

        #region 400 Bad Request Tests

        [TestCase]
        [TestRail(183571)]
        [Description("Create and publish artifact with a trace to target. Verify that GetRelationships with invalid versionId returns 400 Bad Request.")]
        public void GetRelationships_GetRelationshipsWithInvalidVersionId_400BadRequest()
        {
            // Setup: Create and Publish a srouce artifact
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute: Execute GetRelationships with invalid version ID of the source artifact (less than 1)
            Assert.Throws<Http400BadRequestException>(() => Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: INVALID_VERSIONID), "Calling GET {0} with invalid version ID should return 400 Bad Request!", RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS);
        }

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183572)]
        [Description("Create and publish artifact with a trace to target. Verify that GetRelationshipsDetails with invalid revisionId returns 400 Bad Request.")]
        public void GetRelationshipsDetails_GetRelationshipsWithInvalidRevisionId_400BadRequest(TraceDirection direction)
        {
            // Setup: Create and Publish Two target artifacts: target artifact
            // Create and publish artifact with outgoing trace to target artifact 
            var bpServerAddress = Helper.BlueprintServer.Address;
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var traces = OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish(); //creation of first version

            // GetRelationship for the available versions of the source artifact
            Relationships relationships = null;
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: 1);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Validates trace properties from relationships for the version
            TraceValidation(relationships, traces, new List<IArtifact> { targetArtifact });

            // Execute: Execute GetRelationshipDetails with the invalid revision ID (less than 1)
            Assert.Throws<Http400BadRequestException>(() => ArtifactStore.GetRelationshipsDetails(bpServerAddress, _user, targetArtifact.Id, revisionId: INVALID_REVISIONID), "Calling GET {0} with invalid revision ID should return 400 Bad Request!", RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIP_DETAILS);
        }

        #endregion 400 Bad Request Tests

        #region 404 Not Found Tests

        [TestCase(NONEXSITING_REVISIONID)]
        [TestCase(10)]
        [TestRail(183563)]
        [Description("Create and publish artifact with a trace to target. Verify that GetRelationships with non-existing versionId returns 404 Not Found.")]
        public void GetRelationships_GetRelationshipsWithNonExistingVersionId_404NotFound(int nonExistingVersionId)
        {
            // Setup: Create and Publish a srouce artifact
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute: Execute GetRelationships with invalid version ID of the source artifact
            var ex = Assert.Throws<Http404NotFoundException>(() => Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: nonExistingVersionId), "Calling GET {0} with invalid version ID should return 404 NotFound!", RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS);

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "GetRelationships with invalid versionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase(TraceDirection.To, NONEXSITING_REVISIONID)]
        [TestCase(TraceDirection.From, 10)]
        [TestCase(TraceDirection.TwoWay, 3)]
        [TestRail(183566)]
        [Description("Create and publish artifact with a trace to target. Verify that GetRelationshipsDetails with non-existing revisionId returns 404 Not Found.")]
        public void GetRelationshipsDetails_GetRelationshipsWithNonExistingRevisionId_404NotFound(TraceDirection direction, int nonExistingRevisionId)
        {
            // Setup: Create and Publish Two target artifacts: target artifact
            // Create and publish artifact with outgoing trace to target artifact 
            var bpServerAddress = Helper.BlueprintServer.Address;
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Document);
            var traces = OpenApiArtifact.AddTrace(bpServerAddress, sourceArtifact, targetArtifact, direction, _user);
            sourceArtifact.Publish(); //creation of first version

            // GetRelationship for the available versions of the source artifact
            Relationships relationships = null;
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: 1);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Validates trace properties from relationships for the version
            TraceValidation(relationships, traces, new List<IArtifact> { targetArtifact });

            // Delete the target artifact to test GetRelationshipsDetails for the target artifact with non existing revision ID
            targetArtifact.Delete();
            targetArtifact.Publish();

            // Execute: Execute GetRelationshipDetails with the invalid revision ID
            var ex = Assert.Throws<Http404NotFoundException>(() => ArtifactStore.GetRelationshipsDetails(bpServerAddress, _user, targetArtifact.Id, revisionId: nonExistingRevisionId), "Calling GET {0} with invalid revision ID should return 404 NotFound!", RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIP_DETAILS);

            var serviceErrorMessage = Deserialization.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound), "GetRelationshipsDetails with invalid revisionId should return {0} errorCode but {1} is returned", ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        #endregion 404 Not Found Tests


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
        [TestRail(153841)]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        [Description("Try to Get Relationships for a invalid sub-artifact ID .  Verify 400 Bad Request is returned.")]
        public void GetRelationships_InvalidSubArtifactId_400BadRequest(int fakeSubArtifactId)
        {
            // Setup:
            IArtifact sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, subArtifactId: fakeSubArtifactId);
            }, "GetArtifactRelationships should return 400 BadRequest if the sub-artifact ID is invalid.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(153595)]
        [Description("Try to Get Relationships for a sub-artifact ID that doesn't exist.  Verify 404 Not Found is returned.")]
        public void GetRelationships_NonExstingSubArtifactId_404NotFound(int fakeSubArtifactId)
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
