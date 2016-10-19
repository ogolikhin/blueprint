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

        [TestCase]
        [TestRail(1)]
        [Description(".")]
        public void AddTrace_Between2PublishedArtifacts_TraceHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            Assert.DoesNotThrow(() => { ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact, traceDirection: TraceDirection.To,
                changeType: 0, artifactStore: Helper.ArtifactStore); },
                "...");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "..");
        }

        [TestCase]
        [TestRail(10)]
        [Description(".")]
        public void AddTrace_SubArtifactArtifact_TraceHasExpectedValue()
        {
            //IArtifact artifact = ArtifactFactory.CreateArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement, 12656);
            //IArtifact targetArtifact = ArtifactFactory.CreateArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase, 115);
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
                "...");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "..");
        }

        [TestCase]
        [TestRail(2)]
        [Description(".")]
        public void DeleteTrace_ToManualTrace_ReturnsCorrectTraces()
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
                "...");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase(TraceDirection.From, TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay, TraceDirection.To)]
        [TestCase(TraceDirection.To, TraceDirection.TwoWay)]
        [TestRail(3)]
        [Description(".")]
        public void ChangeTraceDirection_ManualTrace_ReturnsCorrectTraces(TraceDirection initialDirection,
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
                "...");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(finalDirection, relationships.ManualTraces[0].Direction, "Relationships should have expected direction.");
        }

        [TestCase]
        [TestRail(4)]
        [Description(".")]
        public void AddTrace_ArtifactSubArtifact_TraceHasExpectedValue()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, targetArtifact.Id);

            artifact.Lock(_authorUser);
            var updatedArtifactDetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact, TraceDirection.From,
                0, subArtifacts[0]);

            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, updatedArtifactDetails); },
                "...");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "..");
        }

        
        /// <summary>
        /// ....
        /// </summary>
        /// <param name="artifactDetails">.</param>
        /// <param name="traceTarget">.</param>
        /// <param name="traceDirection">.</param>
        /// <param name="changeType">changeType.</param>
        /// <param name="traceTargetSubArtifact">IArtifactStore.</param>
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