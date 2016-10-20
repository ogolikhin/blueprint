using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.NovaModel;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Common;
using Utilities.Factories;
using Utilities;
using Model.ArtifactModel.Impl;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SaveTracesTests : TestBase
    {
        private IUser _adminUser = null;
        private IUser _authorUser = null;
        private IProject _projectTest = null;
        private List<IProject> _projects = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _projects = ProjectFactory.GetAllProjects(_adminUser, shouldRetrievePropertyTypes: true);
            _projectTest = _projects[0];
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _projectTest);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183600)]
        [Description("Create trace between 2 published artifacts, check that trace has expected direction.")]
        public void AddTrace_Between2PublishedArtifacts_TraceHasExpectedValue(TraceDirection direction)
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            Assert.DoesNotThrow(() => { ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                traceDirection: direction, changeType: 0, artifactStore: Helper.ArtifactStore); },
                "Trace creation shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(direction, relationships.ManualTraces[0].Direction, "Trace should have expected direction.");
        }

        [TestCase]
        [TestRail(183601)]
        [Description("Create trace between SubArtifact and Artifact, check that trace has expected direction.")]
        public void AddTrace_SubArtifactArtifact_TraceHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace = new NovaTrace();
            trace.ProjectId = targetArtifact.ProjectId;
            trace.ArtifactId = targetArtifact.Id;
            trace.ChangeType = 0;
            trace.Direction = TraceDirection.From;
            trace.IsSuspect = false;
            trace.ItemId = targetArtifact.Id;
            trace.TraceType = TraceTypes.Manual;

            subArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = subArtifacts;
            artifactDetails.Traces = new List<NovaTrace>();
                        
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, artifactDetails); },
                "trace creation shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            Assert.AreEqual(trace.Direction, relationships.ManualTraces[0].Direction, "Trace should have expected direction.");
        }

        [TestCase]
        [TestRail(183602)]
        [Description("Delete trace from artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_ArtifactWithManualTrace_TraceWasDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, artifact,
                targetArtifact, TraceDirection.To, _adminUser);
            artifact.Publish(_adminUser);
            Assert.AreEqual(1, traces.Count);

            // Execute:
            Assert.DoesNotThrow(() => { ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact,
                targetArtifact, changeType: 2, artifactStore: Helper.ArtifactStore, traceDirection: TraceDirection.To); },
                "trace deletion shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase(TraceDirection.From, TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay, TraceDirection.To)]
        [TestCase(TraceDirection.To, TraceDirection.TwoWay)]
        [TestRail(183603)]
        [Description("Change trace direction, check that direction was changed.")]
        public void ChangeTraceDirection_ArtifactWithTrace_TraceHasExpecteddirection(TraceDirection initialDirection,
            TraceDirection finalDirection)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);

            var traces = OpenApiArtifact.AddTrace(Helper.BlueprintServer.Address, artifact,
                targetArtifact, initialDirection, _adminUser);
            artifact.Publish(_adminUser);
            Assert.AreEqual(1, traces.Count);

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: 1, artifactStore: Helper.ArtifactStore, traceDirection: finalDirection);
            },
                "Changing trace direction shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(finalDirection, relationships.ManualTraces[0].Direction, "Relationships should have expected direction.");
        }

        [TestCase]
        [TestRail(1836044)]
        [Description("Add trace between Artifact and SubArtifact, check that trace has expected direction.")]
        public void AddTrace_ArtifactSubArtifact_TraceHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, targetArtifact.Id);

            artifact.Lock(_authorUser);
            TraceDirection direction = TraceDirection.From;
            var updatedArtifactDetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact, direction,
                0, subArtifacts[0]);

            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, updatedArtifactDetails); },
                "Trace creation shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "relationships should have 1 manual trace.");
            Assert.AreEqual(direction, relationships.ManualTraces[0].Direction, "Relationships should have expected direction.");
        }

        [TestCase]
        [Explicit(IgnoreReasons.TestBug)]//unknown error during the test
        [TestRail(183605)]
        [Description("Create trace between 2 SubArtifact, Artifact and other Artifacts, check that operation throw no errors.")]
        public void AddTrace_Between2SubArtifactsArtifactAndArtifact_TraceHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact targetArtifact1 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact2 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact3 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace1 = new NovaTrace();
            trace1.ProjectId = targetArtifact1.ProjectId;
            trace1.ArtifactId = targetArtifact1.Id;
            trace1.ChangeType = 0;
            trace1.Direction = TraceDirection.From;
            trace1.IsSuspect = false;
            trace1.ItemId = targetArtifact1.Id;
            trace1.TraceType = TraceTypes.Manual;

            subArtifacts[0].Traces = new List<NovaTrace> { trace1 };

            NovaTrace trace2 = new NovaTrace();
            trace2.ProjectId = targetArtifact2.ProjectId;
            trace2.ArtifactId = targetArtifact2.Id;
            trace2.ChangeType = 0;
            trace2.Direction = TraceDirection.To;
            trace2.IsSuspect = false;
            trace2.ItemId = targetArtifact2.Id;
            trace2.TraceType = TraceTypes.Manual;

            subArtifacts[1].Traces = new List<NovaTrace> { trace2 };

            artifactDetails.SubArtifacts = subArtifacts;
            artifactDetails.Traces = new List<NovaTrace>();

            var updatedArtifactdetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact3, TraceDirection.TwoWay,
                changeType: 0);

            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, updatedArtifactdetails); },
                "trace creation shouldn't throw any error.");
            Relationships subArtifact1Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Relationships subArtifact2Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[1].Id,
                addDrafts: true);
            Relationships subArtifact3Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[2].Id,
                addDrafts: true);
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, subArtifact1Relationships.ManualTraces.Count, "1 manual trace should be created.");
            Assert.AreEqual(targetArtifact1.Id, subArtifact1Relationships.ManualTraces[0].ArtifactId,
                "Id should have expected value.");
            Assert.AreEqual(1, subArtifact2Relationships.ManualTraces.Count, "1 manual trace should be created.");
            Assert.AreEqual(targetArtifact2.Id, subArtifact1Relationships.ManualTraces[1].ArtifactId,
                "Id should have expected value.");
            Assert.AreEqual(0, subArtifact3Relationships.ManualTraces.Count, "No manual trace should be created for the 3rd subartifact.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            Assert.AreEqual(targetArtifact3.Id, relationships.ManualTraces[0].ArtifactId,
                "Id should have expected value.");
            //Assert.AreEqual(trace.Direction, relationships.ManualTraces[0].Direction, "Trace should have expected direction.");
        }


        /// <summary>
        /// Updates NovaArtifactDetails with trace.
        /// </summary>
        /// <param name="artifactDetails">NovaArtifactDetails to update.</param>
        /// <param name="traceTarget">Target artifact of the trace.</param>
        /// <param name="traceDirection">direction of the trace.</param>
        /// <param name="changeType">changeType.</param>
        /// <param name="traceTargetSubArtifact">SubArtifact (if we need to created trace to subartifact).</param>
        /// <returns>NovaArtifactDetails with updated list of traces. </returns>
        private static NovaArtifactDetails AddArtifactTraceToArtifactDetails(NovaArtifactDetails artifactDetails, IArtifact traceTarget,
            TraceDirection traceDirection, int changeType, INovaSubArtifact traceTargetSubArtifact = null)
        {
            if (traceTargetSubArtifact != null)
            {
                Assert.AreEqual(traceTarget.Id, traceTargetSubArtifact.ParentId, "...");
            }
            NovaTrace traceToCreate = new NovaTrace();
            traceToCreate.ArtifactId = traceTarget.Id;
            traceToCreate.ProjectId = traceTarget.ProjectId;
            traceToCreate.Direction = traceDirection;
            traceToCreate.TraceType = TraceTypes.Manual;
            traceToCreate.ItemId = traceTargetSubArtifact?.Id ?? traceTarget.Id;
            traceToCreate.ChangeType = changeType; // TODO: replace with enum create = 0, 1 = update, 2 = delete

            List<NovaTrace> updatedTraces = new List<NovaTrace> { traceToCreate };
            
            artifactDetails.Traces = updatedTraces;

            return artifactDetails;
        }
    }
}