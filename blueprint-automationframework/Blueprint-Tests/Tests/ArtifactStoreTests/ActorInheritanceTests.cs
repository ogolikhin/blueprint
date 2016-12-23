using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.Factories;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ActorInheritanceTests : TestBase
    {
        private IUser _user = null;

        private IProject _project = null;
        private List<IProject> _allProjects = null;

        private int inheritedActorId = 16;
        private int baseActorId = 15;
        private string customDataProjectName = "Custom Data";

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _allProjects = ProjectFactory.GetAllProjects(_user);
            _project = _allProjects.First();
            _project.GetAllArtifactTypes(ProjectFactory.Address, _user);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165800)]
        [Description("Gets ArtifactDetails for the actor artifact with non-empty Inherited From field. Verify the inherited from object has expected information.")]
        public void GetActorInheritance_CustomProject_ReturnsActorInheritance()
        {
            // Setup:
            IProject projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, projectCustomData);
            var artifactDetails = Helper.ArtifactStore.GetArtifactDetails(viewer, inheritedActorId);
            ActorInheritanceValue actorInheritance = null;

            // Execution & Verify:
            actorInheritance = artifactDetails.ActorInheritance;
            Assert.AreEqual(baseActorId, actorInheritance.ActorId, "Inherited From artifact should have id {0}, but it has id {1}", baseActorId, actorInheritance.ActorId);
            Assert.AreEqual(customDataProjectName, actorInheritance.PathToProject[0], "PathToProject[0] - name of project which contains Inherited From actor.");
        }

        #endregion Custom Data

        #region 200 OK Tests

        [TestCase]
        [TestRail(182329)]
        [Description("Create 2 Actors, set one Actor inherits from another Actor, check that inheritance has expected values.")]
        public void SetActorInheritance_Actor_ReturnsActorInheritance()
        {
            // Setup:
            IArtifact baseActor= Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            // Execute & Verify:
            Assert.DoesNotThrow(() => SetActorInheritance(actor, baseActor, author), "Saving artifact shouldn't throw any exception, but it does.");
            CheckActorHasExpectedActorInheritace(actor, baseActor, author);
            CheckActorHasExpectedTraces(actor, baseActor, author);
        }

        [TestCase]
        [TestRail(182331)]
        [Description("Create 2 Actors, one Actor inherits from another Actor, delete inheritance, check that inheritance is empty.")]
        public void DeleteActorInheritance_ActorWithInheritance_ReturnsActorNoInheritance()
        {
            // Setup:
            IArtifact baseActor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);
            SetActorInheritance(actor, baseActor, author);

            // Execute & Verify:
            Assert.DoesNotThrow(() => DeleteActorInheritance(actor, author), "Deleting Actor inheritance shouldn't throw any exception, but it does.");
            CheckActorHasNoActorInheritace(actor, author);
            CheckActorHasNoOtherTraces(actor, author);
        }

        #endregion 200 OK Tests

        #region 409 Conflict Tests

        [TestCase]
        [TestRail(182332)]
        [Description("Create Actor2 inherits from Actor1, try to set inheritance Actor1 from Actor2, it should return 409.")]
        public void SetActor1Inheritance_Actor2InheritedFromActor1_Returns409CyclicReference()
        {
            // Setup:
            IArtifact actor1 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Author, _project);

            SetActorInheritance(actor2, actor1, author);

            // Execute & Verify:
            Assert.Throws<Http409ConflictException>(() => SetActorInheritance(actor1, actor2, author),
                "Attempt to create cyclic reference Actor1 -> Actor2 -> Actor1 should throw 409, but it doesn't.");
        }

        #endregion 409 Conflict Tests

        #region Private Functions

        /// <summary>
        /// Sets Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Acrtor artifact.</param>
        /// <param name="baseActor">Acrtor to use for Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void SetActorInheritance(IArtifact actor, IArtifact baseActor, IUser user)
        {
            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            ActorInheritanceValue actorInheritance = new ActorInheritanceValue();
            actorInheritance.ActorId = baseActor.Id;
            actorDetails.ActorInheritance = actorInheritance;

            actor.Lock(user);
            
            Artifact.UpdateArtifact(actor, user, actorDetails, address: Helper.BlueprintServer.Address);
        }

        /// <summary>
        /// Deletes Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Acrtor artifact.</param>
        /// <param name="user">User to perform operation.</param>
        private void DeleteActorInheritance(IArtifact actor, IUser user)
        {
            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            actorDetails.ActorInheritance = null;

            actor.Lock(user);

            Artifact.UpdateArtifact(actor, user, actorDetails, address: Helper.BlueprintServer.Address);
        }

        /// <summary>
        /// Check that Actor has empty Inherits From value.
        /// </summary>
        /// <param name="actor">Acrtor to check.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasNoActorInheritace(IArtifact actor, IUser user)
        {
            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNull(actorDetails.ActorInheritance, "ActorInheritance must be empty");
        }

        /// <summary>
        /// Check that Actor has expected Inherits From value.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="expectedBaseActor">Actor expected in Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasExpectedActorInheritace(IArtifact actor, IArtifact expectedBaseActor, IUser user)
        {
            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNotNull(actorDetails.ActorInheritance, "Actor Inheritance shouldn't be null, but it does.");
            Assert.AreEqual(expectedBaseActor.Id, actorDetails.ActorInheritance.ActorId, "ArtifactId must be the same, but it isn't.");
            Assert.AreEqual(expectedBaseActor.Name, actorDetails.ActorInheritance.ActorName, "Name must be the same, but it isn't.");
            Assert.AreEqual(expectedBaseActor.Project.Name, actorDetails.ActorInheritance.PathToProject[0], "Base Actor should have expected project name, but it doesn't.");
        }

        /// <summary>
        /// Check that Actor has trace to BaseActor in Relationships\Other Traces.
        /// </summary>
        /// <param name="actor">Acrtor to check.</param>
        /// <param name="expectedBaseActor">Actor expected in Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasExpectedTraces(IArtifact actor, IArtifact expectedBaseActor, IUser user)
        {
            Relationships actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor);
            
            Assert.AreEqual(1, actorRelationships.OtherTraces.Count, "Actor should have 1 'other' trace, but it doesn't.");
            var actorInheritanceTrace = actorRelationships.OtherTraces[0];

            Assert.AreEqual(expectedBaseActor.Id, actorInheritanceTrace.ArtifactId, "ArtifactId must be the same, but it doesn't.");
            Assert.AreEqual(TraceType.ActorInherits.ToString(), actorInheritanceTrace.TraceType.ToString(), "Trace should have Actor Inheritance trace type, but it doesn't.");
            Assert.AreEqual(TraceDirection.To, actorInheritanceTrace.Direction, "Trace should have 'To' trace direction, but it doesn't.");
            Assert.AreEqual(expectedBaseActor.Name, actorInheritanceTrace.ArtifactName, "Trace should have expected Base Actor name, but it doesn't.");

            var actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNotNull(actorDetails.ActorInheritance, "Actor Inheritance shouldn't be null, but it does.");
            Assert.AreEqual(actorDetails.ActorInheritance.HasAccess, actorInheritanceTrace.HasAccess, "Trace should have expected 'HasAccess' value, but it doesn't.");
            Assert.AreEqual(expectedBaseActor.Project.Name, actorInheritanceTrace.ProjectName, "Base Actor should have expected project name, but it doesn't.");
        }

        /// <summary>
        /// Check that Actor has no traces in Relationships\Other Traces.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasNoOtherTraces(IArtifact actor, IUser user)
        {
            Relationships actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor);
            Assert.AreEqual(0, actorRelationships.OtherTraces.Count, "Actor shouldn't have 'other' traces, but it has.");
        }

        #endregion Private Functions

    }
}
