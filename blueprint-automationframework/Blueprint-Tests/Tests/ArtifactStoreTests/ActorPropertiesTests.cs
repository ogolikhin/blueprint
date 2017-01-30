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
using System;
using TestCommon;
using Utilities;
using Common;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ActorPropertiesTests : TestBase
    {
        private IUser _user = null;
        private IUser _author = null;

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
            _author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
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
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, projectCustomData);
            var artifactDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(viewer, inheritedActorId);
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
            var baseActor= Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            // Execute & Verify:
            Assert.DoesNotThrow(() => SetActorInheritance(actor, baseActor, _author), "Saving artifact shouldn't throw any exception, but it does.");
            CheckActorHasExpectedActorInheritace(actor, baseActor, _author);
            CheckActorHasExpectedTraces(actor, baseActor, _author);
        }

        [TestCase]
        [TestRail(182331)]
        [Description("Create 2 Actors, one Actor inherits from another Actor, delete inheritance, check that inheritance is empty.")]
        public void DeleteActorInheritance_ActorWithInheritance_ReturnsActorNoInheritance()
        {
            // Setup:
            var baseActor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            SetActorInheritance(actor, baseActor, _author);

            // Execute & Verify:
            Assert.DoesNotThrow(() => DeleteActorInheritance(actor, _author), "Deleting Actor inheritance shouldn't throw any exception, but it does.");
            CheckActorHasNoActorInheritace(actor, _author);
            CheckActorHasNoOtherTraces(actor, _author);
        }

        [TestCase]
        [TestRail(234388)]
        [Description("Create and publish Actor, set one Actor icon, check that icon has expected values.")]
        public void SetActorIcon_Actor_ValidateReturnedActorIcon()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);

            var actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            actor.Lock(_author);

            // Execute & Verify:
            SetActorIconAndValidate(_author, actor, imageFile);
        }

        [TestCase]
        [TestRail(234413)]
        [Description("Create and save Actor, set one Actor icon, check that icon has expected values.")]
        public void SetActorIcon_NeverPublishedActor_ValidateReturnedActorIcon()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);

            var actor = Helper.CreateAndSaveArtifact(_project, _author, BaseArtifactType.Actor);
            actor.Lock(_author);

            // Execute & Verify:
            SetActorIconAndValidate(_author, actor, imageFile);
        }
        
        [TestCase]
        [TestRail(234389)]
        [Description("Create and publish Actor, set one Actor icon, delete Actor icon, check that Actor has no icon.")]
        public void DeleteActorIcon_ActorWithIcon_ValidateActorHasNoIcon()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);

            var actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            actor.Lock(_author);
            SetActorIconAndValidate(_author, actor, imageFile);

            // Execute & Verify:
            DeleteActorIconAndValidate(_author, actor);
        }


        [TestCase]
        [TestRail(234417)]
        [Description("Create Actor, set Actor icon, publish it, delete Actor icon, publish changes, check that icon has expected values for version 1.")]
        public void GetActorIconWithVersion2_ActorWithIconVersionTwo_IconDeletedVersionThree_ValidateActorIcon()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);

            var actor = Helper.CreateAndPublishArtifact(_project, _author, BaseArtifactType.Actor);
            actor.Lock(_author);
            SetActorIconAndValidate(_author, actor, imageFile);
            actor.Publish(_author);
            actor.Lock(_author);
            DeleteActorIconAndValidate(_author, actor);
            actor.Publish(_author);

            // Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(_author, actor.Id, versionId: 2);

            // Verify:
            ValidateActorIcon(_author, actorDetails, isHistoricalVersion: true);
        }

        [TestCase]
        [TestRail(234419)]
        [Description("Create Actor, set Actor icon, publish it, delete Actor, publish changes, try to get Icon for version 1.")]
        public void GetHistoricalVersionActorIcon_DeletedActor_ValidateActorIcon()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);

            var actor = Helper.CreateAndPublishArtifact(_project, _author, BaseArtifactType.Actor);
            actor.Lock(_author);
            SetActorIconAndValidate(_author, actor, imageFile);
            actor.Publish(_author);
            actor.Delete(_author);
            actor.Publish(_author);

            // Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(_author, actor.Id, versionId: 2);

            // Verify:
            ValidateActorIcon(_author, actorDetails, isHistoricalVersion: true);
        }

        #endregion 200 OK Tests

        #region 40x Conflict Tests

        [TestCase]
        [TestRail(182332)]
        [Description("Create Actor2 inherits from Actor1, try to set inheritance Actor1 from Actor2, it should return 409.")]
        public void SetActor1Inheritance_Actor2InheritedFromActor1_Returns409CyclicReference()
        {
            // Setup:
            var actor1 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            var actor2 = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);

            SetActorInheritance(actor2, actor1, _author);

            // Execute & Verify:
            var ex = Assert.Throws<Http409ConflictException>(() => SetActorInheritance(actor1, actor2, _author),
                "Attempt to create cyclic reference Actor1 -> Actor2 -> Actor1 should throw 409, but it doesn't.");

            const string expectedMessage = "Cannot set the selected Actor as the Base Actor because it results in a cyclic reference.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CycleRelationship, expectedMessage);
        }

        [TestCase]
        [TestRail(234412)]
        [Description("Create and publish Actor, set one Actor icon, check that icon has expected values.")]
        public void SetActorIcon_DeletedFile_Validate404()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);
            Helper.FileStore.DeleteFile(imageFile.Guid, _author);

            var actor = Helper.CreateAndPublishArtifact(_project, _user, BaseArtifactType.Actor);
            actor.Lock(_author);

            // Execute & Verify:
            var ex = Assert.Throws<Http404NotFoundException>(() => SetActorIconAndValidate(_author, actor, imageFile),
                "Attempt to use deleted file should throw 404, but it doesn't.");

            const string expectedMessage = "File with ID:{0} does not exist";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant(expectedMessage, imageFile.Guid));
        }

        #endregion 40x Conflict Tests

        #region Private Functions

        /// <summary>
        /// Sets Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Acrtor artifact.</param>
        /// <param name="baseActor">Acrtor to use for Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void SetActorInheritance(IArtifact actor, IArtifact baseActor, IUser user)
        {
            Actor actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            var actorInheritance = new ActorInheritanceValue();
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
            Actor actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

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
            Actor actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
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
            Actor actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
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
            var actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor);
            
            Assert.AreEqual(1, actorRelationships.OtherTraces.Count, "Actor should have 1 'other' trace, but it doesn't.");
            var actorInheritanceTrace = actorRelationships.OtherTraces[0];

            Assert.AreEqual(expectedBaseActor.Id, actorInheritanceTrace.ArtifactId, "ArtifactId must be the same, but it doesn't.");
            Assert.AreEqual(TraceType.ActorInherits.ToString(), actorInheritanceTrace.TraceType.ToString(), "Trace should have Actor Inheritance trace type, but it doesn't.");
            Assert.AreEqual(TraceDirection.To, actorInheritanceTrace.Direction, "Trace should have 'To' trace direction, but it doesn't.");
            Assert.AreEqual(expectedBaseActor.Name, actorInheritanceTrace.ArtifactName, "Trace should have expected Base Actor name, but it doesn't.");

            Actor actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
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
            var actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor);
            Assert.AreEqual(0, actorRelationships.OtherTraces.Count, "Actor shouldn't have 'other' traces, but it has.");
        }

        /// <summary>
        /// Creates and uploads to FileStore random image file.
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        private IFile CreateAndUploadRandomImageFile(IUser user)
        {
            var imageFile = ArtifactStoreHelper.CreateRandomImageFile();
            DateTime expireTime = DateTime.Now.AddDays(2);
            var uploadedFile = Helper.FileStore.AddFile(imageFile, user, expireTime: expireTime, useMultiPartMime: true);
            return uploadedFile;
        }

        /// <summary>
        /// Set Actor Icon via UpdateArtifact. Artifact should be locked.
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        /// <param name="actorArtifact">Actor artifact to set icon.</param>
        /// <param name="imageFile">Icon image file.</param>
        /// <returns>Actor details</returns>
        private Actor SetActorIconAndValidate(IUser user, IArtifact actorArtifact, IFile imageFile)
        {
            // Setup & Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);
            var actorIcon = new ActorIconValue();
            actorIcon.SetIcon(imageFile.Guid);
            actorDetails.ActorIcon = actorIcon;
            Artifact.UpdateArtifact(actorArtifact, user, actorDetails, address: Helper.BlueprintServer.Address);
            actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);

            // Verify:
            Assert.IsNotNull(actorDetails.ActorIcon, "ActorIcon shouldn't be empty");

            ValidateActorIcon(user, actorDetails);

            var expirationTime = Helper.FileStore.GetSQLExpiredTime(imageFile.Guid);
            Assert.IsNotNull(expirationTime, "After saving ExpiredTime for file should be current time.");
            Assert.IsTrue(DateTimeUtilities.CompareTimePlusOrMinus(expirationTime.Value, actorDetails.LastSavedOn.Value, 1),
                "ExpirationTime should have expected value.");
            return actorDetails;
        }

        /// <summary>
        /// Delete Actor Icon via UpdateArtifact. Artifact should be locked.
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        /// <param name="actorArtifact">Actor artifact to delete icon.</param>
        /// <returns>Actor details</returns>
        private Actor DeleteActorIconAndValidate(IUser user, IArtifact actorArtifact)
        {
            // Setup & Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);
            actorDetails.ActorIcon = null;
            Artifact.UpdateArtifact(actorArtifact, user, actorDetails, address: Helper.BlueprintServer.Address);
            actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);

            // Verify:
            Assert.IsNull(actorDetails.ActorIcon, "ActorIcon should be null");

            return actorDetails;
        }

        /// <summary>
        /// Validates Actor's icon. Checks that icons url has expected format
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        /// <param name="actorDetails">Actor details</param>
        /// <param name="isHistoricalVersion">(optional) true if actorDetails is for historical version. false by default.</param>
        private void ValidateActorIcon(IUser user, Actor actorDetails, bool isHistoricalVersion = false)
        {
            string iconAddress = actorDetails.ActorIcon.GetIconAddress();

            int? versionNumber = (actorDetails.Version == -1) ? null : actorDetails.Version;

            string expectedIconAddress;
            if (isHistoricalVersion)
            {
                expectedIconAddress = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ACTORICON_id_ +
                "?versionId={1}", actorDetails.Id, versionNumber);
            }
            else
            {
                expectedIconAddress = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ACTORICON_id_ +
                "?versionId={1}&addDraft=true&lastSavedTimestamp=", actorDetails.Id, versionNumber);
            }

            if (isHistoricalVersion)
            {
                Assert.AreEqual(expectedIconAddress, iconAddress, "Icon address should have expected format.");
            }
            else
            {
                StringAssert.StartsWith(expectedIconAddress, iconAddress, "Icon address should have expected format.");
            }

            // TODO: add get size (resolution) support for image files. Currently server changes original image size to 90*90 and compress it.
            Assert.DoesNotThrow(() => Helper.ArtifactStore.GetActorIcon(user, actorDetails.Id, versionNumber),
                "Getting ActorIcon shouldn't throw an error.");
        }

        #endregion Private Functions

    }
}
