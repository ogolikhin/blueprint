using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
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
        private IProject _project = null;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _adminUser = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_adminUser);
            _authorUser = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Positive Tests

        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183600)]
        [Description("Create trace between 2 published artifacts, check that trace has expected parameters.")]
        public void AddTrace_Between2PublishedArtifacts_TraceHasExpectedValue(TraceDirection direction)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);

            sourceArtifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.AddManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value,
                    traceDirection: direction);
            }, "Trace creation shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Relationships should have 1 manual trace.");

            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact);
            ArtifactStoreHelper.ValidateTrace(targetRelationships.ManualTraces[0], sourceArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(183601)]
        [Description("Create trace between SubArtifact and Artifact, check that trace has expected direction.")]
        public void AddTrace_SubArtifactArtifact_TraceHasExpectedValue()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, sourceArtifact.Id);

            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(sourceArtifact.Id, subArtifacts, _authorUser);

            sourceArtifact.Lock(_authorUser);

            var trace = new NovaTrace(targetArtifact);
            novaSubArtifacts[0].Traces = new List<NovaTrace> { trace };
            sourceArtifact.SubArtifacts = novaSubArtifacts;

            // Execute:
            Assert.DoesNotThrow(() => sourceArtifact.Update(_authorUser, sourceArtifact.Artifact),
                "Trace creation shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[0].Id, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[0].Id.Value);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(183602)]
        [Description("Delete trace from artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_PublishedArtifactWithTrace_TraceWasDeleted()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            sourceArtifact.Lock(_adminUser);

            ArtifactStoreHelper.AddManualArtifactTraceAndSave(_adminUser, sourceArtifact.Id, targetArtifact.Id, _project.Id,
                TraceDirection.To);

            sourceArtifact.Publish(_adminUser);
            sourceArtifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.DeleteManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value);
            }, "Trace deletion shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact.Id, addDrafts: true);

            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185252)]
        [Description("Delete trace from draft artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_DraftArtifactWithTrace_TraceWasDeleted()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            sourceArtifact.Lock(_authorUser);
            ArtifactStoreHelper.AddManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id,
                targetArtifact.ProjectId.Value);

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Artifact should have 1 trace.");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.DeleteManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value);
            }, "Trace delete shouldn't throw any error.");

            // Verify:
            relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185260)]
        [Description("Delete trace from draft never published artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_DraftNeverPublishedArtifactWithTrace_TraceWasDeleted()
        {
            // Setup:
            var sourceArtifact = Helper.CreateNovaArtifact(_authorUser, _project, ItemTypePredefined.Glossary);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.DomainDiagram);

            ArtifactStoreHelper.AddManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value);

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Artifact should have 1 trace.");

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.DeleteManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value);
            }, "Trace delete shouldn't throw any error.");

            // Verify:
            relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase(TraceDirection.From, TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay, TraceDirection.To)]
        [TestCase(TraceDirection.To, TraceDirection.TwoWay)]
        [TestRail(183603)]
        [Description("Change trace direction, check that direction was changed.")]
        public void ChangeTraceDirection_ArtifactWithTrace_TraceHasExpecteddirection(TraceDirection initialDirection, TraceDirection finalDirection)
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            sourceArtifact.Lock(_adminUser);

            ArtifactStoreHelper.AddManualArtifactTraceAndSave(_adminUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value, initialDirection);
            sourceArtifact.Publish(_adminUser);
            sourceArtifact.Lock(_authorUser);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id,
                    targetArtifact.Id, targetArtifact.ProjectId.Value, changeType: ChangeType.Update, artifactStore: Helper.ArtifactStore,
                    traceDirection: finalDirection);
            }, "Changing trace direction shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(finalDirection, relationships.ManualTraces[0].Direction, "Relationships should have expected direction.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        [TestCase]
        [TestRail(1836044)]
        [Description("Add trace between Artifact and SubArtifact, check that trace has expected direction.")]
        public void AddTrace_ArtifactSubArtifact_TraceHasExpectedValue()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, targetArtifact.Id);
            var subArtifact = Helper.ArtifactStore.GetSubartifact(_authorUser, targetArtifact.Id, subArtifacts[0].Id);

            sourceArtifact.Lock(_authorUser);
            ArtifactStoreHelper.AddManualArtifactTrace(_authorUser, sourceArtifact, targetArtifact.Id, targetArtifact.ProjectId.Value,
                targetSubArtifactId: subArtifact.Id);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                sourceArtifact.Update(_authorUser, sourceArtifact.Artifact);
            }, "Trace creation shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            ValidateTrace(relationships.ManualTraces[0], subArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, subArtifact.Id.Value);
        }

        [TestCase]
        [TestRail(183605)]
        [Description("Create trace between 2 SubArtifacts, Artifact and 3 other Artifacts, check that operation throw no errors.")]
        public void AddTrace_From2SubArtifactsAndArtifactTo3OtherArtifacts_TracesHaveExpectedValue()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var targetArtifact1 = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);
            var targetArtifact2 = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);
            var targetArtifact3 = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, sourceArtifact.Id);
            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(sourceArtifact.Id, subArtifacts, _authorUser);

            sourceArtifact.Lock(_authorUser);

            novaSubArtifacts[0].Traces = new List<NovaTrace> { new NovaTrace(targetArtifact1) };
            novaSubArtifacts[1].Traces = new List<NovaTrace> { new NovaTrace(targetArtifact2, direction: TraceDirection.To) };

            sourceArtifact.SubArtifacts = novaSubArtifacts;
            sourceArtifact.Traces = new List<NovaTrace>();

            ArtifactStoreHelper.AddManualArtifactTrace(_authorUser, sourceArtifact, targetArtifact3.Id, targetArtifact3.ProjectId.Value,
                TraceDirection.TwoWay);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                sourceArtifact.Update(_authorUser, sourceArtifact.Artifact);
            }, "Trace creation shouldn't throw any error.");

            // Verify:
            var subArtifact1Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[0].Id, addDrafts: true);
            var subArtifact2Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[1].Id, addDrafts: true);
            var subArtifact3Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[2].Id, addDrafts: true);
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, subArtifact1Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(subArtifact1Relationships.ManualTraces[0], targetArtifact1);
            Assert.AreEqual(1, subArtifact2Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(subArtifact2Relationships.ManualTraces[0], targetArtifact2);
            Assert.AreEqual(0, subArtifact3Relationships.ManualTraces.Count, "No manual trace should be created for the 3rd subartifact.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact3);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact1.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact2.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact3.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[0].Id.Value);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[1].Id.Value);
        }

        [TestCase]
        [TestRail(185256)]
        [Description("Create trace between 2 SubArtifacts, Artifact and other Artifact, check that operation throw no errors.")]
        public void AddTrace_Between2SubArtifactsArtifactAndArtifact_TracesWereCreated()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, sourceArtifact.Id);
            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(sourceArtifact.Id, subArtifacts, _authorUser);

            sourceArtifact.Lock(_authorUser);

            var trace = new NovaTrace(targetArtifact);
            trace.TraceType = TraceType.ActorInherits;

            novaSubArtifacts[0].Traces = new List<NovaTrace> { trace };
            novaSubArtifacts[1].Traces = new List<NovaTrace> { trace };

            sourceArtifact.SubArtifacts = novaSubArtifacts;

            ArtifactStoreHelper.AddManualArtifactTrace(_authorUser, sourceArtifact, targetArtifact.Id, targetArtifact.ProjectId.Value,
                TraceDirection.TwoWay);

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                sourceArtifact.Update(_authorUser, sourceArtifact.Artifact);
            }, "Trace creation shouldn't throw any error.");

            // Verify:
            var subArtifact1Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[0].Id, addDrafts: true);
            var subArtifact2Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[1].Id, addDrafts: true);
            var subArtifact3Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[2].Id, addDrafts: true);
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, subArtifact1Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(subArtifact1Relationships.ManualTraces[0], targetArtifact);
            Assert.AreEqual(1, subArtifact2Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(subArtifact2Relationships.ManualTraces[0], targetArtifact);
            Assert.AreEqual(0, subArtifact3Relationships.ManualTraces.Count, "No manual trace should be created for the 3rd subartifact.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[0].Id.Value);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[1].Id.Value);
        }

        [TestCase]
        [TestRail(185241)]
        [Description("Create trace between SubArtifact and Artifact, check that trace has expected direction.")]
        public void AddTrace_ArtifactAndItsSubArtifact_TraceHasExpectedValue()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, sourceArtifact.Id);
            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(sourceArtifact.Id, subArtifacts, _authorUser);

            sourceArtifact.Lock(_authorUser);

            var trace = new NovaTrace(sourceArtifact);

            novaSubArtifacts[0].Traces = new List<NovaTrace> { trace };

            sourceArtifact.SubArtifacts = novaSubArtifacts;

            // Execute:
            Assert.DoesNotThrow(() =>
            {
                sourceArtifact.Update(_authorUser, sourceArtifact.Artifact);
            }, "Trace creation shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, subArtifacts[0].Id, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], sourceArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces, novaSubArtifacts[0].Id.Value);
        }

        [TestCase]
        [TestRail(185257)]
        [Description("Add trace when trace's target is locked by other user, check that trace was created.")]
        public void AddTrace_TraceTargetLockedByOtherUser_TraceWasCreated()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.Actor);
            var targetArtifact = Helper.CreateAndPublishNovaArtifact(_adminUser, _project, ItemTypePredefined.UseCase);

            targetArtifact.Lock(_adminUser);
            sourceArtifact.Lock(_authorUser);
            
            // Execute:
            Assert.DoesNotThrow(() =>
            {
                ArtifactStoreHelper.AddManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id, targetArtifact.ProjectId.Value,
                    traceDirection: TraceDirection.TwoWay);
            }, "Trace adding shouldn't throw any error.");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact.Id, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact.Id, addDrafts: true);

            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual traces.");
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Relationships should have 1 manual traces.");
            ArtifactStoreHelper.ValidateTrace(relationships.ManualTraces[0], targetArtifact);
            ArtifactStoreHelper.ValidateTrace(targetRelationships.ManualTraces[0], sourceArtifact);

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, ItemIndicatorFlags.HasManualReuseOrOtherTraces);
        }

        #endregion Positive Tests

        [TestCase]
        [TestRail(185206)]//now it returns 409. by design?
        [Description("Create trace between artifact and deleted artifact, trace shouldn't be created and a 409 Conflict should be returned.")]
        public void AddTrace_BetweenArtifactAndDeletedArtifact_Returns409()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            Helper.ArtifactStore.DeleteArtifact(targetArtifact, _adminUser);
            Helper.ArtifactStore.PublishArtifact(targetArtifact, _adminUser);

            sourceArtifact.Lock(_authorUser);
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id,
                    targetArtifact.ProjectId, traceDirection: TraceDirection.TwoWay, changeType: 0,
                    artifactStore: Helper.ArtifactStore); }, "Adding a trace to a deleted artifact should return 409 Conflict!");

            // Verify:
            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveOverDependencies,
                "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185207)]
        [Description("Tries to add trace between artifact and itself, 409 exception should be thrown, no trace should be created.")]
        public void AddTrace_BetweenArtifactAndItself_Returns409()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            var trace = new NovaTrace(artifact);

            artifactDetails.Traces = new List<NovaTrace> { trace };

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Artifact.UpdateArtifact(artifact, _authorUser, artifactDetails);
            }, "Trace creation should throw 409 error.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveOverDependencies,
                "Cannot add a trace to item itself");

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact should have no traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185242)]
        [Description("Tries to add SubArtifact trace for Artifact without SubArtifacts support (update subartifacts of TextualRequirement with trace).")]
        public void AddSubArtifactTrace_ArtifactWithoutSubArtifacts_Returns409()
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.UseCase);
            var artifactWithNoSubArtifactSupport = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifactWithNoSubArtifactSupport.Id);
            var novaSubArtifacts = ArtifactStoreHelper.GetDetailsForAllSubArtifacts(Helper.ArtifactStore, artifact, subArtifacts, _authorUser);

            artifactWithNoSubArtifactSupport.Lock(_authorUser);

            var trace = new NovaTrace(artifact);

            novaSubArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = novaSubArtifacts;  // Add SubArtifacts to Artifact details of without SubArtifacts supports

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                Artifact.UpdateArtifact(artifactWithNoSubArtifactSupport, _authorUser, artifactDetails);
            }, "Trace creation should throw 409 error.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveOverDependencies,
                "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.");   // Bug: 5107  Needs a better error message.

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifactWithNoSubArtifactSupport,
                addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "No traces should be created.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, expectedIndicatorFlags: null, subArtifactId: (int)novaSubArtifacts[0].Id);
        }

        [TestCase]
        [TestRail(185244)]
        [Description("Try to create trace, user has no access for artifact he tries to update, check 409 exception.")]
        public void AddTrace_UserHasNoTracePermissionForArtifacts_Returns409()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read | RolePermissions.Edit | RolePermissions.Delete, _project, sourceArtifact);

            sourceArtifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id,
                    targetArtifact.ProjectId, traceDirection: TraceDirection.From, changeType: ChangeType.Create,
                    artifactStore: Helper.ArtifactStore); },"Trace creation shouldn't throw any error.");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveDueToReadOnly,
                "Cannot perform save, the artifact provided is attempting to override a read-only property.");

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185255)]
        [Description("Try to create trace, user has no Edit permission for trace target, check 409 exception.")]
        public void AddTrace_UserHasNoEditPermissionForArtifact_Returns409()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read | RolePermissions.Delete, _project, targetArtifact);

            sourceArtifact.Lock(_authorUser);
            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id,
                    targetArtifact.ProjectId, traceDirection: TraceDirection.From, changeType: ChangeType.Create,
                    artifactStore: Helper.ArtifactStore); },
                    "Adding a trace when the user doesn't have Edit permission for the trace target should return 409 Conflict!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveDueToReadOnly,
                "Cannot perform save, the artifact provided is attempting to override a read-only property.");

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        [TestCase]
        [TestRail(185245)]
        [Description("Try to create trace, user has no access for artifact he tries to update, check 409 exception.")]
        public void AddTrace_UserHasNoTracePermissionForTargetArtifacts_Returns409()
        {
            // Setup:
            var sourceArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            var targetArtifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read, _project, targetArtifact);

            sourceArtifact.Lock(_authorUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, sourceArtifact.Id, targetArtifact.Id,
                    targetArtifact.ProjectId, traceDirection: TraceDirection.From, changeType: ChangeType.Create,
                    artifactStore: Helper.ArtifactStore);
            }, "Adding a trace with a user that has no access to the target artifact should return 409 Conflict!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveDueToReadOnly,
                "Cannot perform save, the artifact provided is attempting to override a read-only property.");

            var relationships = Helper.ArtifactStore.GetRelationships(_authorUser, sourceArtifact, addDrafts: true);
            var targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, sourceArtifact.Id, expectedIndicatorFlags: null);
            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, targetArtifact.Id, expectedIndicatorFlags: null);
        }

        #region Custom Data

        [TestCase(4)]   //Project 'Custom Data'
        [TestCase(6)]   //'Collections' folder
        [TestCase(87)]  //Collection
        [TestCase(5)]   //'Baseline and Reviews' folder
        [Category(Categories.CustomData)]
        [TestRail(185246)]
        [Description("Tries to create trace to Project/Collection/Collection Folder/Baseline Folder.")]
        public void AddTrace_BetweenArtifactAndNonValidItem_Returns409(int nonValidItemId)
        {
            // Setup:
            var artifact = Helper.CreateAndPublishArtifact(_project, _adminUser, BaseArtifactType.TextualRequirement);
            var projectArtifact = ArtifactFactory.CreateArtifact(_project, _adminUser, BaseArtifactType.Glossary, nonValidItemId);

            artifact.Lock(_adminUser);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_adminUser, artifact.Id, projectArtifact.Id,
                    projectArtifact.Id, traceDirection: TraceDirection.To, changeType: ChangeType.Create,
                    artifactStore: Helper.ArtifactStore);
            }, "Adding a trace to an invalid (unsupported) artifact type should return 409 Conflict!");

            // Verify:
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CannotSaveOverDependencies,
                "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.");   // Bug: 5107  Needs a better error message.

            var relationships = Helper.ArtifactStore.GetRelationships(_adminUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have any traces.");

            ArtifactStoreHelper.VerifyIndicatorFlags(Helper, _authorUser, artifact.Id, expectedIndicatorFlags: null);
        }

        #endregion Custom Data

        #region Private functions

        /// <summary>
        /// Validates that the trace properties are correct.
        /// </summary>
        /// <param name="trace">The trace to validate.</param>
        /// <param name="subArtifact">The sub-artifact that contains this trace.</param>
        private static void ValidateTrace(INovaTrace trace, NovaSubArtifact subArtifact)
        {
            Assert.NotNull(subArtifact.Id, "The SubArtifact Id shouldn't be null!");
            Assert.NotNull(subArtifact.PredefinedType, "The SubArtifact PredefinedType shouldn't be null!");

            Assert.AreEqual(trace.ArtifactId, subArtifact.ParentId, "ArtifactId from trace and subartifact should be equal to each other.");
            Assert.AreEqual(subArtifact.Id.Value, trace.ItemId, "ItemId from trace and subartifact should be equal to each other.");
            Assert.AreEqual((int)subArtifact.PredefinedType.Value, trace.PrimitiveItemTypePredefined, "PredefinedType from trace and subartifact should be equal to each other.");
        }

        #endregion Private functions
    }
}
