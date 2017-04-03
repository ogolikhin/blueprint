using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Model.Factories;
using NUnit.Framework;
using System;
using TestCommon;
using Utilities;
using Common;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ActorPropertiesTests : TestBase
    {
        private IUser _user = null;
        private IUser _author = null;

        private IProject _project = null;

        private const int INHERITED_ACTOR_ID = 16;
        private const int BASE_ACTOR_ID = 15;

        #region Setup and Cleanup

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
            _project = ProjectFactory.GetProject(_user);
            _author = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.AuthorFullAccess, _project);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #endregion Setup and Cleanup

        #region Custom data tests

        [Category(Categories.GoldenData)]
        [Category(Categories.CustomData)]
        [TestCase]
        [TestRail(165800)]
        [Description("Gets ArtifactDetails for the actor artifact with non-empty Inherited From field. Verify the inherited from object has expected information.")]
        public void GetActorInheritance_CustomProject_ReturnsActorInheritance()
        {
            // Setup:
            var projectCustomData = ArtifactStoreHelper.GetCustomDataProject(_user);
            var viewer = Helper.CreateUserWithProjectRolePermissions(TestHelper.ProjectRole.Viewer, projectCustomData);

            // Execute:
            Actor artifactDetails = null;

            Assert.DoesNotThrow(() => artifactDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(viewer, INHERITED_ACTOR_ID),
                "GetArtifactDetails() should succeed for Actors that are inherited from other Actors.");

            // Verify:
            ActorInheritanceValue actorInheritance = artifactDetails.ActorInheritance;

            Assert.AreEqual(BASE_ACTOR_ID, actorInheritance.ActorId, "Inherited From artifact should have id {0}, but it has id {1}", BASE_ACTOR_ID, actorInheritance.ActorId);
            Assert.AreEqual(projectCustomData.Name, actorInheritance.PathToProject[0], "PathToProject[0] - name of project which contains Inherited From actor.");
        }

        #endregion Custom Data

        #region 200 OK Tests

        [TestCase]
        [TestRail(182329)]
        [Description("Create 2 Actors, set one Actor inherits from another Actor, check that inheritance has expected values.")]
        public void SetActorInheritance_Actor_ReturnsActorInheritance()
        {
            // Setup:
            var baseActor= Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            // Execute:
            Assert.DoesNotThrow(() => SetActorInheritance(actor, baseActor, _author),
                "Saving artifact shouldn't throw any exception, but it does.");

            // Verify:
            CheckActorHasExpectedActorInheritace(actor, baseActor, _author);
            CheckActorHasExpectedTraces(actor, baseActor, _author);
        }

        [TestCase]
        [TestRail(182331)]
        [Description("Create 2 Actors, one Actor inherits from another Actor, delete inheritance, check that inheritance is empty.")]
        public void DeleteActorInheritance_ActorWithInheritance_ReturnsActorNoInheritance()
        {
            // Setup:
            var baseActor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            SetActorInheritance(actor, baseActor, _author);

            // Execute:
            Assert.DoesNotThrow(() => DeleteActorInheritance(actor, _author),
                "Deleting Actor inheritance shouldn't throw any exception, but it does.");

            // Verify:
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

            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
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

            var actor = Helper.CreateNovaArtifact(_author, _project, ItemTypePredefined.Actor);
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

            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
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

            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
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

            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
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

        #region Negative Tests

        [TestCase]
        [TestRail(234412)]
        [Description("Create and publish Actor, set one Actor icon, check that icon has expected values.")]
        public void SetActorIcon_DeletedFile_Validate404()
        {
            // Setup:
            var imageFile = CreateAndUploadRandomImageFile(_author);
            Helper.FileStore.DeleteFile(imageFile.Guid, _author);

            var actor = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            actor.Lock(_author);

            // Execute:
            var ex = Assert.Throws<Http404NotFoundException>(() => SetActorIconAndValidate(_author, actor, imageFile),
                "Attempt to set Actor Icon to a deleted file should return 404 Not Found!");

            // Verify:
            const string expectedMessage = "File with ID:{0} does not exist";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.NotFound,
                I18NHelper.FormatInvariant(expectedMessage, imageFile.Guid));
        }

        [TestCase]
        [TestRail(182332)]
        [Description("Create Actor2 inherits from Actor1, try to set inheritance Actor1 from Actor2, it should return 409.")]
        public void SetActor1Inheritance_Actor2InheritedFromActor1_Returns409CyclicReference()
        {
            // Setup:
            var actor1 = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);
            var actor2 = Helper.CreateAndPublishNovaArtifact(_user, _project, ItemTypePredefined.Actor);

            SetActorInheritance(actor2, actor1, _author);

            // Execute:
            var ex = Assert.Throws<Http409ConflictException>(() => SetActorInheritance(actor1, actor2, _author),
                "Attempt to create cyclic reference Actor1 -> Actor2 -> Actor1 should return 409 Conflict!");

            // Verify:
            const string expectedMessage = "Cannot set the selected Actor as the Base Actor because it results in a cyclic reference.";
            TestHelper.ValidateServiceError(ex.RestResponse, InternalApiErrorCodes.CycleRelationship, expectedMessage);
        }

        #endregion Negative Tests

        #region Private Functions

        /// <summary>
        /// Sets Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Actor artifact.</param>
        /// <param name="baseActor">Actor to use for Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void SetActorInheritance(ArtifactWrapper actor, ArtifactWrapper baseActor, IUser user)
        {
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            var actorInheritance = new ActorInheritanceValue { ActorId = baseActor.Id };
            actorDetails.ActorInheritance = actorInheritance;

            actor.Lock(user);
            actor.Update(user, actorDetails);
        }

        /// <summary>
        /// Deletes Actor Inheritance value for Actor artifact.
        /// </summary>
        /// <param name="actor">Actor artifact.</param>
        /// <param name="user">User to perform operation.</param>
        private void DeleteActorInheritance(ArtifactWrapper actor, IUser user)
        {
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            actorDetails.ActorInheritance = null;

            actor.Lock(user);
            actor.Update(user, actorDetails);
        }

        /// <summary>
        /// Check that Actor has empty Inherits From value.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasNoActorInheritace(ArtifactWrapper actor, IUser user)
        {
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);
            Assert.IsNull(actorDetails.ActorInheritance, "ActorInheritance must be empty");
        }

        /// <summary>
        /// Check that Actor has expected Inherits From value.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="expectedBaseActor">Actor expected in Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasExpectedActorInheritace(ArtifactWrapper actor, ArtifactWrapper expectedBaseActor, IUser user)
        {
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            Assert.IsNotNull(actorDetails.ActorInheritance, "Actor Inheritance shouldn't be null, but it does.");
            Assert.AreEqual(expectedBaseActor.Id, actorDetails.ActorInheritance.ActorId, "ArtifactId must be the same, but it isn't.");
            Assert.AreEqual(expectedBaseActor.Name, actorDetails.ActorInheritance.ActorName, "Name must be the same, but it isn't.");
            Assert.AreEqual(expectedBaseActor.Project.Name, actorDetails.ActorInheritance.PathToProject[0], "Base Actor should have expected project name, but it doesn't.");
        }

        /// <summary>
        /// Check that Actor has trace to BaseActor in Relationships\Other Traces.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="expectedBaseActor">Actor expected in Actor Inheritance.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasExpectedTraces(ArtifactWrapper actor, ArtifactWrapper expectedBaseActor, IUser user)
        {
            var actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor.Id);
            
            Assert.AreEqual(1, actorRelationships.OtherTraces.Count, "Actor should have 1 'other' trace, but it doesn't.");
            var actorInheritanceTrace = actorRelationships.OtherTraces[0];

            Assert.AreEqual(expectedBaseActor.Id, actorInheritanceTrace.ArtifactId, "ArtifactId must be the same, but it doesn't.");
            Assert.AreEqual(TraceType.ActorInherits.ToString(), actorInheritanceTrace.TraceType.ToString(), "Trace should have Actor Inheritance trace type, but it doesn't.");
            Assert.AreEqual(TraceDirection.To, actorInheritanceTrace.Direction, "Trace should have 'To' trace direction, but it doesn't.");
            Assert.AreEqual(expectedBaseActor.Name, actorInheritanceTrace.ArtifactName, "Trace should have expected Base Actor name, but it doesn't.");

            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actor.Id);

            Assert.IsNotNull(actorDetails.ActorInheritance, "Actor Inheritance shouldn't be null, but it does.");
            Assert.AreEqual(actorDetails.ActorInheritance.HasAccess, actorInheritanceTrace.HasAccess, "Trace should have expected 'HasAccess' value, but it doesn't.");
            Assert.AreEqual(expectedBaseActor.Project.Name, actorInheritanceTrace.ProjectName, "Base Actor should have expected project name, but it doesn't.");
        }

        /// <summary>
        /// Check that Actor has no traces in Relationships\Other Traces.
        /// </summary>
        /// <param name="actor">Actor to check.</param>
        /// <param name="user">User to perform operation.</param>
        private void CheckActorHasNoOtherTraces(ArtifactWrapper actor, IUser user)
        {
            var actorRelationships = Helper.ArtifactStore.GetRelationships(user, actor.Id);
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
        private Actor SetActorIconAndValidate(IUser user, ArtifactWrapper actorArtifact, IFile imageFile)
        {
            // Setup & Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);
            var actorIcon = new ActorIconValue();

            actorIcon.SetIcon(imageFile.Guid);
            actorDetails.ActorIcon = actorIcon;

            actorArtifact.Update(user, actorDetails);
            actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);

            // Verify:
            Assert.IsNotNull(actorDetails.ActorIcon, "ActorIcon shouldn't be empty");

            ValidateActorIcon(user, actorDetails);

            var expirationTime = Helper.FileStore.GetSQLExpiredTime(imageFile.Guid);

            Assert.IsNotNull(expirationTime, "After saving ExpiredTime for file should be current time.");
            Assert.IsTrue(expirationTime.Value.CompareTimePlusOrMinusMilliseconds(actorDetails.LastSavedOn.Value, 2000),
                "ExpirationTime should have expected value.  ExpiredTime in DB is: {0}, but LastSavedOn is: {1}",
                expirationTime.Value, actorDetails.LastSavedOn.Value);

            return actorDetails;
        }

        /// <summary>
        /// Delete Actor Icon via UpdateArtifact. Artifact should be locked.
        /// </summary>
        /// <param name="user">User to perform operation.</param>
        /// <param name="actorArtifact">Actor artifact to delete icon.</param>
        /// <returns>Actor details</returns>
        private Actor DeleteActorIconAndValidate(IUser user, ArtifactWrapper actorArtifact)
        {
            // Setup & Execute:
            var actorDetails = (Actor)Helper.ArtifactStore.GetArtifactDetails(user, actorArtifact.Id);
            actorDetails.ActorIcon = null;

            actorArtifact.Update(user, actorDetails);
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
                Assert.AreEqual(expectedIconAddress, iconAddress, "Icon address should have expected format.");
            }
            else
            {
                expectedIconAddress = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ACTORICON_id_ +
                "?versionId={1}&addDraft=true&lastSavedTimestamp=", actorDetails.Id, versionNumber);
                StringAssert.StartsWith(expectedIconAddress, iconAddress, "Icon address should have expected format.");
            }

            // TODO: add get size (resolution) support for image files. Currently server changes original image size to 90*90 and compress it.
            Assert.DoesNotThrow(() => Helper.ArtifactStore.GetActorIcon(user, actorDetails.Id, versionNumber),
                "Getting ActorIcon shouldn't throw an error.");
        }

        #endregion Private Functions

    }
}
