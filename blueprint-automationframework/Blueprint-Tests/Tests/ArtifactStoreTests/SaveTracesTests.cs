using System.Collections.Generic;
using CustomAttributes;
using Helper;
using Model;
using Model.Impl;
using Model.ArtifactModel;
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

        #region Positive Tests
        [TestCase(TraceDirection.From)]
        [TestCase(TraceDirection.To)]
        [TestCase(TraceDirection.TwoWay)]
        [TestRail(183600)]
        [Description("Create trace between 2 published artifacts, check that trace has expected parameters.")]
        public void AddTrace_Between2PublishedArtifacts_TraceHasExpectedValue(TraceDirection direction)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            // Execute:
            Assert.DoesNotThrow(() => { ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                traceDirection: direction, changeType: ArtifactUpdateChangeType.Add, artifactStore: Helper.ArtifactStore); },
                "Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact);
            ValidateTrace(targetRelationships.ManualTraces[0], artifact);
        }

        [TestCase]
        [TestRail(183601)]
        [Description("Create trace between SubArtifact and Artifact, check that trace has expected direction.")]
        public void AddTrace_SubArtifactArtifact_TraceHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace = new NovaTrace(targetArtifact);

            subArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = subArtifacts;
            
            // Execute:
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, artifactDetails); },
                "Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact);
        }

        [TestCase]
        [TestRail(183602)]
        [Description("Delete trace from artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_PublishedArtifactWithTrace_TraceWasDeleted()
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
                targetArtifact, changeType: ArtifactUpdateChangeType.Delete, artifactStore: Helper.ArtifactStore, traceDirection: TraceDirection.To); },
                "Trace deletion shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase]
        [TestRail(185252)]
        [Description("Delete trace from draft artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_DraftArtifactWithTrace_TraceWasDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);

            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: ArtifactUpdateChangeType.Add, artifactStore: Helper.ArtifactStore);

            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Artifact should have 1 trace.");

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: ArtifactUpdateChangeType.Delete, artifactStore: Helper.ArtifactStore);
            }, "Trace delete shouldn't throw any error.");

            // Verify:
            relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have traces.");
        }

        [TestCase]
        [TestRail(185260)]
        [Description("Delete trace from draft never published artifact with manual trace, check that trace was deleted.")]
        public void DeleteTrace_DraftNeverPublishedArtifactWithTrace_TraceWasDeleted()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndSaveArtifact(_projectTest, _authorUser, BaseArtifactType.Glossary);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.DomainDiagram);

            ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: ArtifactUpdateChangeType.Add, artifactStore: Helper.ArtifactStore);

            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Artifact should have 1 trace.");

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: ArtifactUpdateChangeType.Delete, artifactStore: Helper.ArtifactStore);
            }, "Trace delete shouldn't throw any error.");

            // Verify:
            relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have traces.");
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
                    changeType: ArtifactUpdateChangeType.Update, artifactStore: Helper.ArtifactStore, traceDirection: finalDirection);
            },
                "Changing trace direction shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            Assert.AreEqual(finalDirection, relationships.ManualTraces[0].Direction, "Relationships should have expected direction.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact);
        }

        [TestCase]
        [TestRail(1836044)]
        [Description("Add trace between Artifact and SubArtifact, check that trace has expected direction.")]
        public void AddTrace_ArtifactSubArtifact_TraceHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, targetArtifact.Id);

            artifact.Lock(_authorUser);
            TraceDirection direction = TraceDirection.From;
            var updatedArtifactDetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact, direction,
                changeType: ArtifactUpdateChangeType.Add, traceTargetSubArtifact: subArtifacts[0]);

            // Execute:
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, updatedArtifactDetails); },
                "Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual trace.");
            ValidateTrace(relationships.ManualTraces[0], subArtifacts[0]);
        }

        [TestCase]
        [TestRail(183605)]
        [Description("Create trace between 2 SubArtifacts, Artifact and 3 other Artifacts, check that operation throw no errors.")]
        public void AddTrace_From2SubArtifactsAndArtifactTo3OtherArtifacts_TracesHaveExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact targetArtifact1 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact2 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact3 = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            subArtifacts[0].Traces = new List<NovaTrace> { new NovaTrace(targetArtifact1) };
            subArtifacts[1].Traces = new List<NovaTrace> { new NovaTrace(targetArtifact2, TraceDirection.To) };

            artifactDetails.SubArtifacts = subArtifacts;
            artifactDetails.Traces = new List<NovaTrace>();

            var updatedArtifactdetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact3, TraceDirection.TwoWay,
                changeType: ArtifactUpdateChangeType.Add);

            // Execute:
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, updatedArtifactdetails); },
                "Trace creation shouldn't throw any error.");

            // Verify:
            Relationships subArtifact1Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Relationships subArtifact2Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[1].Id,
                addDrafts: true);
            Relationships subArtifact3Relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[2].Id,
                addDrafts: true);
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(1, subArtifact1Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(subArtifact1Relationships.ManualTraces[0], targetArtifact1);
            Assert.AreEqual(1, subArtifact2Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(subArtifact2Relationships.ManualTraces[0], targetArtifact2);
            Assert.AreEqual(0, subArtifact3Relationships.ManualTraces.Count, "No manual trace should be created for the 3rd subartifact.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact3);
        }

        [TestCase]
        [TestRail(185256)]
        [Description("Create trace between 2 SubArtifacts, Artifact and other Artifact, check that operation throw no errors.")]
        public void AddTrace_Between2SubArtifactsArtifactAndArtifact_TracesWereCreated()
        {
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace = new NovaTrace(targetArtifact);
            trace.TraceType = TraceTypes.ActorInherits;

            subArtifacts[0].Traces = new List<NovaTrace> { trace };
            subArtifacts[1].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = subArtifacts;

            var updatedArtifactdetails = AddArtifactTraceToArtifactDetails(artifactDetails, targetArtifact, TraceDirection.TwoWay,
                changeType: ArtifactUpdateChangeType.Add);

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
            ValidateTrace(subArtifact1Relationships.ManualTraces[0], targetArtifact);
            Assert.AreEqual(1, subArtifact2Relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(subArtifact2Relationships.ManualTraces[0], targetArtifact);
            Assert.AreEqual(0, subArtifact3Relationships.ManualTraces.Count, "No manual trace should be created for the 3rd subartifact.");
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact);
        }

        [TestCase]
        [TestRail(185241)]
        [Description("Create trace between SubArtifact and Artifact, check that trace has expected direction.")]
        public void AddTrace_ArtifactAndItsSubArtifact_TraceHasExpectedValue()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace = new NovaTrace(artifact);

            subArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = subArtifacts;

            // Execute:
            Assert.DoesNotThrow(() => { Artifact.UpdateArtifact(artifact, _authorUser, artifactDetails); },
                "Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, subArtifacts[0].Id,
                addDrafts: true);
            Assert.AreEqual(1, relationships.ManualTraces.Count, "1 manual trace should be created.");
            ValidateTrace(relationships.ManualTraces[0], artifact);
        }

        [TestCase]
        [TestRail(185257)]
        [Description("Add trace when trace's target is locked by other user, check that trace was created.")]
        public void AddTrace_TraceTargetLockedByOtherUser_TraceWasCreated()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.Actor);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);

            targetArtifact.Lock(_adminUser);
            
            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    changeType: ArtifactUpdateChangeType.Add, artifactStore: Helper.ArtifactStore, traceDirection: TraceDirection.TwoWay);
            }, "Trace adding shouldn't throw any error.");
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);

            // Verify:
            Assert.AreEqual(1, relationships.ManualTraces.Count, "Relationships should have 1 manual traces.");
            Assert.AreEqual(1, targetRelationships.ManualTraces.Count, "Relationships should have 1 manual traces.");
            ValidateTrace(relationships.ManualTraces[0], targetArtifact);
            ValidateTrace(targetRelationships.ManualTraces[0], artifact);
        }
        #endregion Positive Tests

        [TestCase]
        [TestRail(185206)]//now it returns 409. by deisgn?
        [Description("Create trace between artifact and deleted artifact, trace shouldn't be created and a 409 Conflict should be returned.")]
        public void AddTrace_BetweenArtifactAndDeletedArtifact_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            Helper.ArtifactStore.DeleteArtifact(targetArtifact, _adminUser);
            Helper.ArtifactStore.PublishArtifact(targetArtifact, _adminUser);

            // Execute:
            Assert.DoesNotThrow(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    traceDirection: TraceDirection.TwoWay, changeType: 0, artifactStore: Helper.ArtifactStore);
            },"Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase]
        [TestRail(185207)]
        [Description("Tries to add trace between artifact and itself, 409 exception should be thrown, no trace should be created.")]
        public void AddTrace_BetweenArtifactAndItself_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifact.Id);

            artifact.Lock(_authorUser);

            NovaTrace trace = new NovaTrace(artifact);

            artifactDetails.Traces = new List<NovaTrace> { trace};

            IServiceErrorMessage traceToItselfMessage = new ServiceErrorMessage("Cannot add a trace to item itself", 123);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                Artifact.UpdateArtifact(artifact, _authorUser, artifactDetails, traceToItselfMessage);
            }, "Trace creation should throw 409 error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact should have no traces.");
        }

        [TestCase]
        [TestRail(185242)]//https://trello.com/c/dI1XaYSz
        [Description("Tries to add SubArtifact trace for Artifact without SubArtifacts support (update subartifacts of TextualRequirement with trace).")]
        public void AddSubArtifactTrace_ArtifactWithoutSubArtifacts_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.UseCase);
            IArtifact artifactWithNoSubArtifactSupport = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            var subArtifacts = Helper.ArtifactStore.GetSubartifacts(_authorUser, artifact.Id);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_authorUser, artifactWithNoSubArtifactSupport.Id);

            artifactWithNoSubArtifactSupport.Lock(_authorUser);

            NovaTrace trace = new NovaTrace(artifact);

            subArtifacts[0].Traces = new List<NovaTrace> { trace };

            artifactDetails.SubArtifacts = subArtifacts;// Add SubArtifacts to Artifact details of without SubArtifacts supports

            IServiceErrorMessage addSubArtifactsToNoSubartifactsSupportArtifactMessage = new ServiceErrorMessage("Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.", 123);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                Artifact.UpdateArtifact(artifactWithNoSubArtifactSupport, _authorUser,
                    artifactDetails, expectedServiceErrorMessage: addSubArtifactsToNoSubartifactsSupportArtifactMessage);
            }, "Trace creation should throw 409 error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifactWithNoSubArtifactSupport,
                addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "No traces should be created.");
        }

        [TestCase]//https://trello.com/c/fr3IOGF4 message should be updated
        [TestRail(185244)]
        [Description("Try to create trace, user has no access for artifact he tries to update, check 409 exception.")]
        public void AddTrace_UserHasNoTracePermissionForArtifacts_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read | RolePermissions.Edit | RolePermissions.Delete,
                _projectTest, artifact);

            IServiceErrorMessage traceToItselfMessage = new ServiceErrorMessage("Cannot perform save, the artifact provided is attempting to override a read-only property.", 116);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    traceDirection: TraceDirection.From, changeType: ArtifactUpdateChangeType.Add,
                    artifactStore: Helper.ArtifactStore, expectedErrorMessage: traceToItselfMessage);
                },"Trace creation shouldn't throw any error.");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase]
        [TestRail(185255)]
        [Description("Try to create trace, user has no Edit permission for trace target, check 409 exception.")]
        public void AddTrace_UserHasNoEditPermissionForArtifact_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read | RolePermissions.Delete, _projectTest,
                targetArtifact);

            IServiceErrorMessage traceToItselfMessage = new ServiceErrorMessage(
                "Cannot perform save, the artifact provided is attempting to override a read-only property.",
                InternalApiErrorCodes.CannotSaveDueToReuseReadOnly);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    traceDirection: TraceDirection.From, changeType: ArtifactUpdateChangeType.Add,
                    artifactStore: Helper.ArtifactStore, expectedErrorMessage: traceToItselfMessage);
            }, "Adding a trace when the user doesn't have Edit permission for the trace target should return 409 Conflict!");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        [TestCase]
        [TestRail(185245)]
        [Description("Try to create trace, user has no access for artifact he tries to update, check 409 exception.")]
        public void AddTrace_UserHasNoTracePermissionForTargetArtifacts_Returns409()
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact targetArtifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);

            Helper.AssignProjectRolePermissionsToUser(_authorUser, RolePermissions.Read, _projectTest, targetArtifact);

            IServiceErrorMessage traceToItselfMessage = new ServiceErrorMessage(
                "Cannot perform save, the artifact provided is attempting to override a read-only property.",
                InternalApiErrorCodes.CannotSaveDueToReuseReadOnly);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_authorUser, artifact, targetArtifact,
                    traceDirection: TraceDirection.From, changeType: ArtifactUpdateChangeType.Add,
                    artifactStore: Helper.ArtifactStore, expectedErrorMessage: traceToItselfMessage);
            }, "Adding a trace with a user that has no access to the target artifact should return 409 Conflict!");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_authorUser, artifact, addDrafts: true);
            Relationships targetRelationships = Helper.ArtifactStore.GetRelationships(_authorUser, targetArtifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Relationships should have no manual traces.");
            Assert.AreEqual(0, targetRelationships.ManualTraces.Count, "Relationships should have no manual traces.");
        }

        #region Custom Data

        [TestCase(4)]//Project 'Custom Data'
        [TestCase(6)]//'Collections' folder
        [TestCase(87)]//Collection
        [TestCase(5)]//'Baseline and Reviews' folder
        [Category(Categories.CustomData)]
        [TestRail(185246)]
        [Description("Tries to create trace to Project/Collection/Collection Folder/Baseline Folder.")]
        public void AddTrace_BetweenArtifactAndNonValidItem_Returns409(int nonValidItemId)
        {
            // Setup:
            IArtifact artifact = Helper.CreateAndPublishArtifact(_projectTest, _adminUser, BaseArtifactType.TextualRequirement);
            IArtifact projectArtifact = ArtifactFactory.CreateArtifact(_projectTest, _adminUser, BaseArtifactType.Glossary, nonValidItemId);

            IServiceErrorMessage wrongArtifactTypeMessage = new ServiceErrorMessage(
                "Exception of type 'BluePrintSys.RC.Business.Internal.Models.InternalApiBusinessException' was thrown.",
                InternalApiErrorCodes.CannotSaveOverDependencies);

            // Execute:
            Assert.Throws<Http409ConflictException>(() => {
                ArtifactStoreHelper.UpdateManualArtifactTraceAndSave(_adminUser, artifact, projectArtifact, traceDirection: TraceDirection.To, changeType: ArtifactUpdateChangeType.Add,
                    artifactStore: Helper.ArtifactStore, expectedErrorMessage: wrongArtifactTypeMessage);
            }, "Adding a trace to an invalid (unsupported) artifact type should return 409 Conflict!");

            // Verify:
            Relationships relationships = Helper.ArtifactStore.GetRelationships(_adminUser, artifact, addDrafts: true);
            Assert.AreEqual(0, relationships.ManualTraces.Count, "Artifact shouldn't have any traces.");
        }

        #endregion Custom Data

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
            TraceDirection traceDirection, ArtifactUpdateChangeType changeType, INovaSubArtifact traceTargetSubArtifact = null)
        {
            if (traceTargetSubArtifact != null)
            {
                Assert.AreEqual(traceTarget.Id, traceTargetSubArtifact.ParentId, "...");
            }
            NovaTrace traceToCreate = new NovaTrace(traceTarget, traceDirection, changeType: changeType);
            traceToCreate.ItemId = traceTargetSubArtifact?.Id ?? traceTarget.Id;

            List<NovaTrace> updatedTraces = new List<NovaTrace> { traceToCreate };

            artifactDetails.Traces = updatedTraces;

            return artifactDetails;
        }

        private static void ValidateTrace(NovaTrace trace, IArtifact artifact)
        {
            Assert.AreEqual(trace.ArtifactId, artifact.Id, "Id from trace and artifact should be equal to each other.");
            Assert.AreEqual(trace.ArtifactName, artifact.Name, "Name from trace and artifact should be equal to each other.");
            Assert.AreEqual(trace.ItemId, artifact.Id, "itemId from trace and artifact should be equal to each other.");
            Assert.AreEqual(trace.ProjectId, artifact.ProjectId, "ProjectId from trace and artifact should be equal to each other.");
            Assert.AreEqual(trace.ProjectName, artifact.Project.Name, "ProjectName from trace and artifact should be equal to each other.");
        }

        private static void ValidateTrace(NovaTrace trace, INovaSubArtifact subArtifact)
        {
            Assert.AreEqual(trace.ArtifactId, subArtifact.ParentId, "ArtifactId from trace and subartifact should be equal to each other.");
            Assert.AreEqual(subArtifact.Id, trace.ItemId, "ItemId from trace and subartifact should be equal to each other.");
            Assert.AreEqual(subArtifact.PredefinedType, trace.PrimitiveItemTypePredefined, "PredefinedType from trace and subartifact should be equal to each other.");
        }
    }
}
