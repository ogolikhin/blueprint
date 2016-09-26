using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ActorPropertiesTests : TestBase
    {
        private IUser _user = null;

        private IProject _project = null;
        private List<IProject> _allProjects = null;

        private int actorInheritedFromOtherActorId = 8;
        private int parentActorId = 9;
        private string customDataProjectName = "Custom Data";

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

        #region Custom data tests

        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165800)]
        [Description("Gets ArtifactDetails for the actor with non-empty Inherited From field. Verify the inherited from object has expected information.")]
        public void GetActorInheritance_CustomProject_ReturnsActorInheritance()
        {
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, actorInheritedFromOtherActorId);
            ActorInheritanceValue actorInheritance = null;

            actorInheritance = artifactDetails.ActorInheritance;
            Assert.AreEqual(parentActorId, actorInheritance.ActorId, "Inherited From artifact should have id {0}, but it has id {1}", parentActorId, actorInheritance.ActorId);
            Assert.AreEqual(customDataProjectName, actorInheritance.PathToProject[0], "PathToProject[0] - name of project which contains Inherited From actor.");
        }

        #endregion Custom Data

        [TestCase]
        [TestRail(182329)]
        [Description("Create 2 Actors, set one Actor inherits from another Actor, check that inheritance has expected values.")]
        public void SetActorInheritance_Actor_ReturnsActorInheritance()
        {
            // Setup:
            IArtifact baseActor= Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute & Verify:
            Assert.DoesNotThrow(() => SetActorInheritance(actor, baseActor, _user), "Saving artifact shouldn't throw any exception, but it does.");
            CheckActorHasExpectedActorInheritace(actor, baseActor, _user);
        }

        [TestCase]
        [TestRail(182331)]
        [Description("Create 2 Actors, one Actor inherits from another Actor, delete inheritance, check that inheritance is empty.")]
        public void DeleteActorInheritance_ActorWithInheritance_ReturnsActorNoInheritance()
        {
            // Setup:
            IArtifact baseActor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            SetActorInheritance(actor, baseActor, _user);

            // Execute & Verify:
            Assert.DoesNotThrow(() => DeleteActorInheritance(actor, _user), "Deleting Actor inheritance shouldn't throw any exception, but it does.");
            CheckActorHasNoActorInheritace(actor, _user);
        }

        [TestCase]
        [TestRail(182332)]
        [Description("Create Actor2 inherits from Actor1, try to set inheritance Actor1 from Actor2, it should return 409.")]
        public void SetActor1Inheritance_Actor2InheritedFromActor1_Returns409CyclicReference()
        {
            // Setup:
            IArtifact actor1 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            IArtifact actor2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            SetActorInheritance(actor2, actor1, _user);

            // Execute & Verify:
            Assert.Throws<Http409ConflictException>(() => SetActorInheritance(actor1, actor2, _user),
                "Attempt to create cyclic reference Actor1 -> Actor2 -> Actor1 should throw 409, but it doesn't.");
        }

        /// <summary>
        /// Sets Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Acrtor artifact.</param>
        /// <param name="baseActor">Acrtor to use for Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void SetActorInheritance(IArtifact actor, IArtifact baseActor, IUser user)
        {
            NovaArtifactDetails actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            ActorInheritanceValue actorInheritance = new ActorInheritanceValue();
            actorInheritance.ActorId = baseActor.Id;
            actorDetails.ActorInheritance = actorInheritance;

            actor.Lock(user);
            
            Artifact.UpdateArtifact(actor, user, actorDetails, Helper.BlueprintServer.Address);
        }

        /// <summary>
        /// Deletes Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Acrtor artifact.</param>
        /// <param name="user">User to perform operation.</param>
        private void DeleteActorInheritance(IArtifact actor, IUser user)
        {
            NovaArtifactDetails actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            actorDetails.ActorInheritance = null;

            actor.Lock(user);

            Artifact.UpdateArtifact(actor, user, actorDetails, Helper.BlueprintServer.Address);
        }

        /// <summary>
        /// Check that Actor has empty Inherits From value.
        /// </summary>
        /// <param name="actor">Acrtor to check.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasNoActorInheritace(IArtifact actor, IUser user)
        {
            NovaArtifactDetails actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNull(actorDetails.ActorInheritance, "ActorInheritance must be empty");
        }

        /// <summary>
        /// Check that Actor has expected Inherits From value.
        /// </summary>
        /// <param name="actor">Acrtor to check.</param>
        /// <param name="expectedBaseActor">Actor expected in Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasExpectedActorInheritace(IArtifact actor, IArtifact expectedBaseActor, IUser user)
        {
            NovaArtifactDetails actorDetails = Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNotNull(actorDetails.ActorInheritance, "Actor Inheritance shouldn't be null, but it does.");
            Assert.AreEqual(actorDetails.ActorInheritance.ActorId, expectedBaseActor.Id, "ArtifactId must be the same, but it isn't.");
            Assert.AreEqual(actorDetails.ActorInheritance.ActorName, expectedBaseActor.Name, "Name must be the same, but it isn't.");
        }
    }
}
