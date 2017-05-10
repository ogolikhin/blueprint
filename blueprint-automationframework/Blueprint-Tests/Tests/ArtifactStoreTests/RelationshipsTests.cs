using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Model.OpenApiModel.Services;
using Model.StorytellerModel.Enums;
using TestCommon;
using Utilities;
using Model.ArtifactModel.Enums;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    public class RelationshipsTests : TestBase
    {
        private IUser _user = null;
        private IUser _viewer = null;
        private IProject _project = null;

        private const int INVALID_VERSIONID = -1;
        private const int NONEXSITING_VERSIONID = int.MaxValue;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, _project);
    }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Private Functions

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
                ArtifactStoreHelper.AssertTracesAreEqual(traces[i], relationship.ManualTraces[i]);
                Assert.That(relationship.ManualTraces[i].ArtifactName.Equals(foundArtifactName), "Name '{0}' from target artifact does not match with Name '{1}' from manual trace of relationships.", foundArtifactName, relationship.ManualTraces[i].ArtifactName);
            }
        }

        #endregion Private Functions

        #region 200 OK Tests

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183545)]
        [Description("Create and publish artifact with a trace to target. Update and publish the artifact with the updated trace pointing to another target.  " +
            "Verify that GetRelationship call returns correct trace for each version of artifact.")]
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
                relationshipsV2 = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact, versionId: 2);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Validation: Validates trace properties from relationships for each version
            TraceValidation(relationshipsV1, tracesV1, new List<IArtifact> { targetArtifact1 });
            TraceValidation(relationshipsV2, tracesV2, new List<IArtifact> { targetArtifact2 });

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact1.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact2.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(153694)]
        [Description("Create manual trace between 2 artifacts, get relationships.  Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceDirection_ReturnsCorrectTraces(TraceDirection direction)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, direction, _user);

            Assert.AreEqual(false, traces[0].IsSuspect, "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            Relationships relationships = null;

            sourceArtifact.Publish(_user);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase(true)]
        [TestCase(false)]
        [TestRail(153698)]
        [Description("Create manual trace between 2 artifacts & set the trace as suspect, get relationships.  Verify returned trace has expected value.")]
        public void GetRelationships_ManualTraceHasSuspect_ReturnsCorrectTraces(bool suspected)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact,
                targetArtifact, TraceDirection.To, _user, isSuspect: suspected);

            Assert.AreEqual(traces[0].IsSuspect, suspected,
                "IsSuspected should be {0} after adding a trace without specifying a value for isSuspect!", suspected);

            Relationships relationships = null;

            sourceArtifact.Publish(_user);
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(153795)]
        [Description("Create manual trace between 2 artifacts, delete the target artifact, get relationships.  Verify no traces are returned.")]
        public void GetRelationships_DeleteTargetArtifact_ReturnsNoTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            targetArtifact.Delete(_user);
            targetArtifact.Publish(_user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships shouldn't have manual traces when the target artifact is deleted.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have other traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(154426)]
        [Description("Create manual trace between an artifact and a sub-artifact, delete the sub-artifact.  Get relationships.  Verify no traces are returned.")]
        public void GetRelationships_DeleteSubArtifact_ReturnsNoTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var novaProcess = StorytellerTestHelper.CreateAndGetDefaultNovaProcessWithTwoSequentialUserTasks(_project, _user);

            var userTasks = novaProcess.Process.GetProcessShapesByShapeType(ProcessShapeType.UserTask);
            Assert.That(userTasks.Count > 1, "There should be more than one User Task!");

            var targetArtifact = new ArtifactBase {Id = novaProcess.Id, ProjectId = novaProcess.Process.ProjectId};

            int subArtifactId = userTasks[0].Id;

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, _user, subArtifactId: subArtifactId);

            // Publish the Trace we added.
            sourceArtifact.Publish();

            // Delete the first User Task and publish.
            novaProcess.Process.DeleteUserAndSystemTask(userTasks[0]);
            StorytellerTestHelper.UpdateVerifyAndPublishNovaProcess(novaProcess.NovaProcess, _user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be no 'other' traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase(true)]
        [TestCase(null)]
        [TestRail(153703)]
        [Description("Create manual trace between 2 Saved (but unpublished) artifacts, get relationships (with and without the 'addDrafts=true' query parameter).  " +
            "Verify manual trace is returned.")]
        public void GetRelationships_SavedNeverPublishedArtifactWithAddDraftsTrue_ReturnsCorrectTraces(bool? addDrafts)
        {
            // Setup:
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            var sourceArtifact = Helper.CreateArtifact(_project, author, BaseArtifactType.UseCase);
            var targetArtifact = Helper.CreateArtifact(_project, author, BaseArtifactType.UseCase);
            sourceArtifact.Save(author);
            targetArtifact.Save(author);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, author);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(author, sourceArtifact, addDrafts: addDrafts);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid Unpublished Draft artifact and {0} addDrafts=true.",
                addDrafts.HasValue ? "with" : "without");

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have any other traces.");
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, author, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, author, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(153700)]
        [Description("Create manual trace between an artifact and a sub-artifact from a different artifact.  Get relationships. " +  
            "Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceArtifactToSubartifactInDifferentArtifact_ReturnsCorrectTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var targetSubArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, targetArtifact.Id);

            sourceArtifact.Lock(_user);

            var trace = new NovaTrace(targetArtifact, targetSubArtifacts[0].Id);

            var sourceArtifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);

            sourceArtifactDetails.Traces = new List<NovaTrace> { trace };

            Artifact.UpdateArtifact(sourceArtifact, _user, sourceArtifactDetails);

            Relationships relationships = null;

            sourceArtifact.Publish(_user);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact, addDrafts: true);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // VerifY:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have any other traces.");
            ArtifactStoreHelper.AssertTracesAreEqual(trace, relationships.ManualTraces[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, targetSubArtifacts[0].Id);
        }

        [TestCase]
        [TestRail(153741)]
        [Description("Create manual trace between two sub-artifacts from different artifacts.  Get relationships. " +
            "Verify that returned trace has expected value.")]
        public void GetRelationships_ManualTraceBetweenTwoSubArtifactsInDifferentArtifacts_ReturnsCorrectTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var sourceSubArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, sourceArtifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, sourceArtifact.Id);
            var sourceNovaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(Helper.ArtifactStore, sourceArtifact, sourceSubArtifacts, _user);

            var targetSubArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, targetArtifact.Id);

            sourceArtifact.Lock(_user);

            var trace = new NovaTrace(targetArtifact, targetSubArtifacts[0].Id);

            sourceNovaSubArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = sourceNovaSubArtifacts;

            Artifact.UpdateArtifact(sourceArtifact, _user, artifactDetails);

            sourceArtifact.Publish(_user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact, sourceSubArtifacts[0].Id, addDrafts: true);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid sub-artifact.");

            // VerifY:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "Relationships shouldn't have any other traces.");

            ArtifactStoreHelper.AssertTracesAreEqual(trace, relationships.ManualTraces[0]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, sourceSubArtifacts[0].Id);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, targetSubArtifacts[0].Id);
        }

        [TestCase]
        [TestRail(153843)]
        [Description("Try to Get Relationships for an artifact ID that has multiple traces.  Verify all traces are returned.")]
        public void GetRelationships_ArtifactWithMultipleTraces_ReturnsAllTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var thirdArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, _user);
            Assert.AreEqual(1, traces.Count, "No traces were added!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, thirdArtifact, TraceDirection.From, _user));
            Assert.AreEqual(2, traces.Count, "No traces were added!");

            Relationships relationships = null;

            sourceArtifact.Publish(_user);

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with multiple traces.");

            Assert.AreEqual(2, relationships.ManualTraces.Count, "There should be 2 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
            ArtifactStoreHelper.AssertTracesAreEqual(traces[1], relationships.ManualTraces[1]);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, thirdArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(153846)]
        [Description("Try to Get Relationships for an artifact ID that has no traces.  Verify no traces are returned.")]
        public void GetRelationships_ArtifactWithNoTraces_ReturnsNoTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            Relationships relationships = null;

            // Execute & Verify:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with no traces.");

            Assert.AreEqual(0, relationships.ManualTraces.Count, "There should be 0 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(153909)]
        [Description("Try to Get Relationships for an artifact ID that is in a dependency loop.  Verify all traces for the artifact are returned.")]
        public void GetRelationships_CyclicTraceDependency_ReturnsAllTraces()
        {
            // Setup:
            var firstArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var secondArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);
            var thirdArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, firstArtifact,
                secondArtifact, TraceDirection.To, _user);
            Assert.AreEqual(1, traces.Count, "No traces were added between first & second artifact!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, secondArtifact, thirdArtifact, TraceDirection.To, _user));
            Assert.AreEqual(2, traces.Count, "No traces were added between second & third artifact!");

            traces.AddRange(OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, thirdArtifact, firstArtifact, TraceDirection.To, _user));
            Assert.AreEqual(3, traces.Count, "No traces were added between third & first artifact!");

            Relationships relationships = null;

            firstArtifact.Publish(_user);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationships = Helper.ArtifactStore.GetRelationships(_viewer, firstArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given an artifact with a cyclic trace dependency.");

            // Verify:
            Assert.AreEqual(2, relationships.ManualTraces.Count, "There should be 2 manual traces!");
            Assert.AreEqual(0, relationships.OtherTraces.Count, "There should be 0 other traces!");
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationships.ManualTraces[0]);
            ArtifactStoreHelper.AssertTracesAreEqual(traces[1], relationships.ManualTraces[1], checkDirection: false);
            Assert.AreEqual(TraceDirection.From, relationships.ManualTraces[1].Direction, "The 2nd manual trace should be 'From' the third artifact!");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, firstArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, secondArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, thirdArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(154699)]
        [Description("Try to get relationships using credentials of user which has no access to the target artifact.  " +
            "Verify that relationships returns empty artifact name and HasAccess false.")]
        public void GetRelationships_NoAccessToTargetArtifact_ReturnsRelationshipsWithHasAccessFalseAndNullName()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            sourceArtifact.Publish(_user);

            Assert.AreEqual(false, traces[0].IsSuspect, "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            Helper.AssignProjectRolePermissionsToUser(_viewer, TestHelper.ProjectRole.None, _project, targetArtifact);

            Relationships relationshipsForUserWithFullAccessToTargetArtifact = null;
            Relationships relationshipsForUserWithNoAccessToTargetArtifact = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                relationshipsForUserWithNoAccessToTargetArtifact = Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
                relationshipsForUserWithFullAccessToTargetArtifact = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships shouldn't throw any error when given a valid artifact.");

            // Verify:
            ArtifactStoreHelper.AssertTracesAreEqual(traces[0], relationshipsForUserWithFullAccessToTargetArtifact.ManualTraces[0]);

            Assert.AreEqual(1, relationshipsForUserWithNoAccessToTargetArtifact.ManualTraces.Count, "There should be 1 manual trace!");
            var trace = relationshipsForUserWithNoAccessToTargetArtifact.ManualTraces[0];
            Assert.AreEqual(targetArtifact.Id, trace.ArtifactId, "Returned trace must have proper artifactId.");
            Assert.IsFalse(trace.HasAccess, "User with no access rights should have no access to the target artifact.");
            Assert.IsNull(trace.ArtifactName, "User with no access rights should receive empty target artifact name.");
            Assert.AreEqual(false, trace.IsSuspect, "Returned trace mustn't be suspected.");
            Assert.AreEqual(traces[0].TraceType.ToString(), trace.TraceType.ToString(), "Returned trace must have proper TraceType.");
            Assert.AreEqual(traces[0].Direction, trace.Direction, "Returned trace must have proper Direction.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _viewer, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(154763)]
        [Description("Get relationshipsdetails for artifact, check that artifact path has expected value.")]
        public void GetRelationshipsDetails_ManualTrace_ReturnsCorrectTraceDetails()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            TraceDetails traceDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                traceDetails = Helper.ArtifactStore.GetRelationshipsDetails(_viewer, artifact);
            }, "GetRelationshipsDetails shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(2, traceDetails.PathToProject.Count, "PathToProject must have 2 items.");
            Assert.AreEqual(artifact.Id, traceDetails.ArtifactId, "Id must be correct.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, artifact.Id, expectedIndicatorFlags: null);
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
            var artifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor, parent: parentArtifact);

            TraceDetails traceDetails = null;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                traceDetails = Helper.ArtifactStore.GetRelationshipsDetails(_viewer, artifact);
            }, "GetRelationshipsDetails shouldn't throw any error when given a valid artifact.");

            // Verify:
            Assert.AreEqual(5, traceDetails.PathToProject.Count, "PathToProject must have 5 items.");
            Assert.AreEqual(artifact.Id, traceDetails.ArtifactId, "Id must be correct.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(267470)]
        [Description("Create and publish two artifacts, create trace between artifacts and save changes, add saved artifact" +
            " to newly created baseline, publish this artifact, get relationships for this artifact providing BaselineId" +
            " as param, check that returned traces have expected values.")]
        public void GetRelationshipsDetails_ArtifactAddedToBaselineProvideBaselineId_ValidateTraces()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Document);
            sourceArtifact.Lock(_user);
            // TODO: create UpdateManualArtifactTraceAndSave which will work with ArtifactWrapper
            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_user, sourceArtifact.Id, targetArtifact.Id,
                targetArtifact.Artifact.ProjectId.Value, ChangeType.Create, Helper.ArtifactStore);
            var baselineArtifact = Helper.CreateBaseline(_user, _project, artifactToAddId: sourceArtifact.Id);
            sourceArtifact.Publish(_user);

            Relationships relationships = null;

            // Execute:
            Assert.DoesNotThrow(() => {
                relationships = Helper.ArtifactStore.GetRelationships(_user, sourceArtifact.Id, baselineId: baselineArtifact.Id);
            }, "Getting relationships with valid baselineId shouldn't throw any error.");

            // Verify:
            Assert.AreEqual(1, relationships?.ManualTraces?.Count, "Relationships should have expected number of manual traces.");
            Assert.AreEqual(targetArtifact.Id, relationships.ManualTraces[0].ArtifactId, "Trace target should have expected artifact Id.");
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
            Assert.Throws<Http400BadRequestException>(() =>
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: INVALID_VERSIONID),
                "Calling GET {0} with invalid version ID should return 400 Bad Request!",
                RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS);
        }

        [TestCase(0)]
        [TestRail(153841)]
        [Description("Try to Get Relationships for a invalid sub-artifact ID .  Verify 400 Bad Request is returned.")]
        public void GetRelationships_InvalidSubArtifactId_400BadRequest(int fakeSubArtifactId)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Execute & Verify:
            Assert.Throws<Http400BadRequestException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, subArtifactId: fakeSubArtifactId);
            }, "GetArtifactRelationships should return 400 BadRequest if the sub-artifact ID is invalid.");
        }

        #endregion 400 Bad Request Tests

        #region 403 Forbidden Tests

        [TestCase]
        [TestRail(153691)]
        [Description("Create manual trace between 2 artifacts, get relationships with a user that doesn't have permission to source artifacts.  " +
            "Verify returns 403 Forbidden.")]
        public void GetRelationships_ManualTraceUserHasNoAccessToSourceArtifact_403Forbidden()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, _user);

            Assert.AreEqual(false, traces[0].IsSuspect, "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            sourceArtifact.Publish(_user);

            Helper.AssignProjectRolePermissionsToUser(_viewer, TestHelper.ProjectRole.None, _project, sourceArtifact);

            // Execute: 
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_viewer, sourceArtifact);
            }, "GetArtifactRelationships should return 403 Forbidden if the user doesn't have permission to access the artifact.");

            // Verify:
            // TODO: Error code and error message verification - Not possible at the moment Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=4859
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _viewer, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(234312)]
        [Description("Create manual trace between 2 artifacts, get relationship details with a user that doesn't have permission to target artifacts.  " +
            "Verify returns 403 Forbidden.")]
        public void GetRelationshipsDetails_ManualTraceUserHasNoAccessToTargetArtifact_403Forbidden()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, _user);

            Assert.AreEqual(false, traces[0].IsSuspect, "IsSuspected should be false after adding a trace without specifying a value for isSuspect!");

            sourceArtifact.Publish(_user);

            Helper.AssignProjectRolePermissionsToUser(_viewer, TestHelper.ProjectRole.None, _project, targetArtifact);

            // Execute: 
            Assert.Throws<Http403ForbiddenException>(() =>
            {
                Helper.ArtifactStore.GetRelationshipsDetails(_viewer, targetArtifact);
            }, "GetArtifactRelationships should return 403 Forbidden if the user doesn't have permission to access the artifact.");

            // Verify:
            // TODO: Error code and error message verification - Not possible at the moment Bug: http://svmtfs2015:8080/tfs/svmtfs2015/Blueprint/_workitems?_a=edit&id=4859
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _viewer, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        #endregion 403 Forbidden Tests

        #region 404 Not Found Tests

        [TestCase(NONEXSITING_VERSIONID)]
        [TestCase(10)]
        [TestRail(183563)]
        [Description("Create and publish artifact with a trace to target. Verify that GetRelationships with non-existing versionId returns 404 Not Found.")]
        public void GetRelationships_GetRelationshipsWithNonExistingVersionId_404NotFound(int nonExistingVersionId)
        {
            // Setup: Create and Publish a srouce artifact
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute: Execute GetRelationships with non-existing version ID of the source artifact
            var ex = Assert.Throws<Http404NotFoundException>(() =>
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, versionId: nonExistingVersionId),
                "Calling GET {0} with non-existing version ID should return 404 NotFound!", RestPaths.Svc.ArtifactStore.Artifacts_id_.RELATIONSHIPS);

            var serviceErrorMessage = SerializationUtilities.DeserializeObject<ServiceErrorMessage>(ex.RestResponse.Content);

            // Validation: Exception should contain proper errorCode in the response content.
            Assert.That(serviceErrorMessage.ErrorCode.Equals(ErrorCodes.ResourceNotFound),
                "GetRelationships with non-existing versionId should return {0} errorCode but {1} is returned",
                ErrorCodes.ResourceNotFound, serviceErrorMessage.ErrorCode);
        }

        [TestCase]
        [TestRail(153702)]
        [Description("Create manual trace between 2 artifacts, delete the source artifact, get relationships.  Verify 404 Not Found is returned.")]
        public void GetRelationships_DeleteSourceArtifact_404NotFound()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.UseCase);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            sourceArtifact.Delete(_user);
            sourceArtifact.Publish(_user);

            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return a 404 Not Found when given a deleted artifact.");

            // Verify:
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(153904)]
        [Description("Create manual trace between 2 Saved (but unpublished) artifacts, get relationships (with the 'addDrafts=false' query parameter).  " +
            "Verify it returns 404 Not Found.")]
        public void GetRelationships_SavedNeverPublishedArtifactWithAddDraftsFalse_404NotFound()
        {
            // Setup:
            var sourceArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            var targetArtifact = Helper.CreateArtifact(_project, _user, BaseArtifactType.UseCase);
            sourceArtifact.Save(_user);
            targetArtifact.Save(_user);

            OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.From, _user);

            // Execute:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact, addDrafts: false);
            }, "GetArtifactRelationships should return 404 Not Found when given a valid Unpublished Draft artifact and addDrafts=false.");

            // Verify:
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _user, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
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
            Assert.Throws<Http404NotFoundException>(() => OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, sourceArtifact.Id, _user),
                "An artifact with ID: {0} was found, but it shouldn't exist!", sourceArtifact.Id);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(_user, sourceArtifact);
            }, "GetArtifactRelationships should return 404 Not Found if the artifact ID doesn't exist.");
        }

        [TestCase(int.MaxValue)]
        [TestRail(153595)]
        [Description("Try to Get Relationships for a sub-artifact ID that doesn't exist.  Verify 404 Not Found is returned.")]
        public void GetRelationships_NonExstingSubArtifactId_404NotFound(int fakeSubArtifactId)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Process);

            // Verify the artifact doesn't exist.
            Assert.Throws<Http404NotFoundException>(() => OpenApi.GetArtifact(Helper.BlueprintServer.Address, _project, fakeSubArtifactId, _user),
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
            var sourceArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.Actor);
            var targetArtifact = Helper.CreateAndSaveArtifact(_project, _user, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, sourceArtifact, targetArtifact, TraceDirection.To, _user);

            Assert.That(traces.Count > 0, "No traces were added!");

            var user2 = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);

            // Execute & Verify:
            Assert.Throws<Http404NotFoundException>(() =>
            {
                Helper.ArtifactStore.GetRelationships(user2, sourceArtifact, addDrafts: addDrafts);
            }, "GetArtifactRelationships should return 404 Not Found for unpublished artifacts created by different users.");
        }

        #endregion 404 Not Found Tests

        // TODO: Test with "Other" traces.
    }
}
