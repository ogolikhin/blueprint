using Common;
using Model.ArtifactModel.Adaptors;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Model.OpenApiModel.Services;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace Model.ArtifactModel.Impl
{
    public class Artifact : ArtifactBase, IArtifact
    {
        #region Constructors

        /// <summary>
        /// Constructor needed to deserialize it as generic type.
        /// </summary>
        public Artifact()
        {
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base url of the API</param>
        public Artifact(string address) : base(address)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="id">The artifact id</param>
        /// <param name="projectId">The project containing the artifact</param>
        public Artifact(string address, int id, int projectId) : base(address, id, projectId)
        {
        }

        #endregion Constructors

        #region Methods

        /// <seealso cref="Artifact.Save(IUser, bool, List{HttpStatusCode})"/>
        public void Save(IUser user = null,
            bool shouldGetLockForUpdate = true,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            // If CreatedBy is null, then this save is adding the artifact.  User must not be null.
            if (CreatedBy == null)
            {
                Assert.NotNull(user, "No user is available to add the artifact.");
                CreatedBy = user;
            }

            // If user is null, attempt to save using the CreatedBy user.  CreatedBy must not be null.
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to save the artifact.");
                user = CreatedBy;
            }

            SaveArtifact(this, user, shouldGetLockForUpdate, expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.Discard(IUser, List{HttpStatusCode})"/>
        public List<DiscardArtifactResult> Discard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactToDiscard = new List<IArtifactBase> { this };

            var discardArtifactResults = DiscardArtifacts(
                artifactToDiscard, 
                Address, 
                user, 
                expectedStatusCodes);

            foreach (var discardArtifactResult in discardArtifactResults)
            {
                if (discardArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    IsSaved = false;
                }
            }

            return discardArtifactResults;
        }

        /// <seealso cref="IArtifact.NovaDiscard(IUser, List{HttpStatusCode})"/>
        public List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactsToDiscard = new List<IArtifactBase> { this };

            return NovaDiscardArtifacts(Address,
                user,
                artifactsToDiscard,
                expectedStatusCodes);
        }

        public int GetVersion(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetVersion.");
                user = CreatedBy;
            }

            int artifactVersion = GetVersion(this, user, expectedStatusCodes);

            return artifactVersion;
        }

        /// <seealso cref="IArtifact.Lock(IUser, LockResult, List{HttpStatusCode})"/>
        public LockResultInfo Lock(IUser user = null,
            LockResult expectedLockResult = LockResult.Success,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return Lock(this, Address, user, expectedLockResult, expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.GetArtifactInfo(IUser, List{HttpStatusCode})"/>
        public ArtifactInfo GetArtifactInfo(IUser user = null,
           List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetArtifactInfo.");
                user = CreatedBy;
            }

            var service = SvcComponentsFactory.CreateSvcComponents(Address);

            return service.GetArtifactInfo(Id, user, expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.GetRapidReviewDiagramContent(IUser, List{HttpStatusCode})"/>
        public RapidReviewDiagram GetRapidReviewDiagramContent(
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            user = user ?? CreatedBy;

            Assert.NotNull(user, "No user is available to perform GetDiagramContentForRapidReview.");

            var artifactInfo = GetArtifactInfo(user);

            // Make sure we're calling this with a diagram artifact type.
            switch (artifactInfo.BaseTypePredefined)
            {
                case ItemTypePredefined.BusinessProcess:
                case ItemTypePredefined.DomainDiagram:
                case ItemTypePredefined.GenericDiagram:
                case ItemTypePredefined.Storyboard:
                case ItemTypePredefined.UseCaseDiagram:
                case ItemTypePredefined.UIMockup:
                    break;
                default:
                    throw new ArgumentException("Method works for graphical artifacts only.");
            }

            var service = SvcComponentsFactory.CreateSvcComponents(Address);

            return service.GetRapidReviewDiagramContent(user, Id, expectedStatusCodes);
        }

        public RapidReviewUseCase GetUseCaseContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetUseCaseContentForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactInfo = GetArtifactInfo(user);

            if (artifactInfo.BaseTypePredefined == ItemTypePredefined.UseCase)
            {
                string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.USECASE_id_, Id);
                var restApi = new RestApiFacade(Address, tokenValue);

                var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewUseCase>(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: expectedStatusCodes);

                return returnedArtifactContent;
            }
            else
            {
                throw new ArgumentException("Method works for UseCase artifacts only.");
            }
        }

        public RapidReviewGlossary GetGlossaryContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetGlossaryContentForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactInfo = GetArtifactInfo(user);

            if (artifactInfo.BaseTypePredefined == ItemTypePredefined.Glossary)
            {
                string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.GLOSSARY_id_, Id);
                var restApi = new RestApiFacade(Address, tokenValue);

                var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewGlossary>(
                    path,
                    RestRequestMethod.GET,
                    expectedStatusCodes: expectedStatusCodes);

                return returnedArtifactContent;
            }
            else
            {
                throw new ArgumentException("Method works for Glossary artifacts only.");
            }
        }

        public RapidReviewProperties GetPropertiesForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetPropertiesForRapidReview.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = RestPaths.Svc.Components.RapidReview.Artifacts.PROPERTIES;
            var artifactIds = new List<int> { Id };
            var restApi = new RestApiFacade(Address, tokenValue);

            var returnedArtifactProperties = restApi.SendRequestAndDeserializeObject<List<RapidReviewProperties>, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return returnedArtifactProperties[0];
        }

        /// <seealso cref="IArtifact.StorytellerPublish(IUser, List{HttpStatusCode})"/>
        public NovaPublishArtifactResult StorytellerPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return StorytellerPublishArtifact(artifactToPublish: this, user: user, expectedStatusCodes: expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.PostRaptorDiscussion(string, IUser, List{HttpStatusCode})"/>
        public IRaptorDiscussion PostRaptorDiscussion(string comment,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            return PostRaptorDiscussion(Address, Id, comment, user, expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.UpdateRaptorDiscussion(RaptorComment, IUser, IRaptorDiscussion, List{HttpStatusCode})"/>
        public IRaptorDiscussion UpdateRaptorDiscussion(RaptorComment comment,
            IUser user, IRaptorDiscussion discussionToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(discussionToUpdate, nameof(discussionToUpdate));

            return UpdateRaptorDiscussion(Address, Id, discussionToUpdate, comment, user, expectedStatusCodes);
        }

        public string DeleteRaptorDiscussion(IUser user, IRaptorDiscussion discussionToDelete,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(discussionToDelete, nameof(discussionToDelete));

            return DeleteRaptorDiscussion(Address, Id, discussionToDelete, user, expectedStatusCodes);
        }

        public OpenApiAttachment AddArtifactAttachment(IFile file, IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            return AddArtifactAttachment(Address, ProjectId, Id, file, user, expectedStatusCodes);
        }

        public OpenApiAttachment AddSubArtifactAttachment(int subArtifactId,
            IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            return AddSubArtifactAttachment(Address, ProjectId, Id, subArtifactId, file,
                user, expectedStatusCodes);
        }

        #endregion Methods

        #region Static Methods

        /// <summary>
        /// Save a single artifact to Blueprint.
        /// </summary>
        /// <param name="artifactToSave">The artifact to save.</param>
        /// <param name="user">The user saving the artifact.</param>
        /// <param name="shouldGetLockForUpdate">(optional) Pass false if you don't want to get a lock before trying to update the artifact.  Default is true.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        public static void SaveArtifact(IArtifactBase artifactToSave,
            IUser user,
            bool shouldGetLockForUpdate = true,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToSave, nameof(artifactToSave));

            // Use POST only if this is creating the artifact, otherwise use PATCH
            var restRequestMethod = artifactToSave.Id == 0 ? RestRequestMethod.POST : RestRequestMethod.PATCH;

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { artifactToSave.Id == 0 ? HttpStatusCode.Created : HttpStatusCode.OK };
            }

            if (restRequestMethod == RestRequestMethod.POST)
            {
                OpenApiArtifact.SaveArtifact(artifactToSave, user, expectedStatusCodes);
            }
            else if (restRequestMethod == RestRequestMethod.PATCH)
            {
                if (shouldGetLockForUpdate)
                {
                    Lock(artifactToSave, artifactToSave.Address, user);
                }

                UpdateArtifact(artifactToSave, user, expectedStatusCodes: expectedStatusCodes);
            }
            else
            {
                throw new InvalidOperationException("Only POST or PATCH methods are supported for saving artifacts!");
            }
        }

        /// <summary>
        /// Update an Artifact with Property Changes.
        /// </summary>
        /// <param name="artifactToUpdate">The artifact to be updated.</param>
        /// <param name="user">The user updating the artifact.</param>
        /// <param name="artifactDetailsChanges">(optional) The changes to make to the artifact.  This should contain the bare minimum changes that you want to make.
        ///     By default if null is passed, this function will make a random change to the 'Description' property.</param>
        /// <param name="address">(optional) The address of the ArtifactStore service.  If null, the Address property of the artifactToUpdate is used.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The ArtifactDetails that was sent to ArtifactStore to be saved.</returns>
        public static NovaArtifactDetails UpdateArtifact(IArtifactBase artifactToUpdate,
            IUser user,
            NovaArtifactDetails artifactDetailsChanges = null,
            string address = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            // TODO: Simplify this function.  It's insanely complicated.

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToUpdate, nameof(artifactToUpdate));

            var artifactChanges = artifactDetailsChanges;

            if (artifactChanges == null)
            {
                artifactChanges = new NovaArtifactDetails
                {
                    Id = artifactToUpdate.Id,
                    ProjectId = artifactToUpdate.ProjectId,
                    Version = artifactToUpdate.Version,
                    Description = "NewDescription_" + RandomGenerator.RandomAlphaNumeric(5)
                };
            }

            // Hack: This is a required hack because the REST call will now return a 501 Unimplemented if you pass the ItemTypeId in the JSON body.
            int? itemTypeId = artifactChanges.ItemTypeId; //we need to restore ItemTypeId after server call
            artifactChanges.ItemTypeId = null;

            if (address == null)
            {
                address = artifactToUpdate.Address;
            }

            // Hack for TFS bug 3739:  If you send a non-null/empty Name of a UseCase SubArtifact to the Update REST call, it returns 500 Internal Server Error
            // Set Name=null for all SubArtifacts to prevent a 500 error.
            if (artifactToUpdate.BaseArtifactType != BaseArtifactType.Process)
            {
                artifactChanges.SubArtifacts?.ForEach(delegate(NovaSubArtifact subArtifact)
                {
                    subArtifact.Name = null;
                });
            }

            ArtifactStore.UpdateArtifact(address, user, artifactChanges, expectedStatusCodes);

            if ((expectedStatusCodes == null) || expectedStatusCodes.Contains(HttpStatusCode.OK))
            {
                artifactToUpdate.IsSaved = true;

                if (user?.Token?.OpenApiToken == null)
                {
                    // We need an OpenAPI token to make the GetProject call below.
                    Assert.NotNull(artifactToUpdate.Project, "Project is null and we don't have an OpenAPI token!");
                }

                var project = artifactToUpdate.Project ?? ProjectFactory.CreateProject().GetProject(address, artifactToUpdate.ProjectId, user);
                var artifactBaseToUpdate = artifactToUpdate as ArtifactBase;

                // Copy updated properties into original artifact.
                if (artifactDetailsChanges == null)
                {
                    artifactBaseToUpdate.Id = artifactToUpdate.Id;
                    artifactBaseToUpdate.ProjectId = artifactToUpdate.ProjectId;
                    artifactBaseToUpdate.Version = artifactToUpdate.Version;
                    artifactBaseToUpdate.AddOrReplaceTextOrChoiceValueProperty(nameof(NovaArtifactDetails.Description), artifactChanges.Description, project, user);
                }
                else
                {
                    artifactBaseToUpdate.ReplacePropertiesWithPropertiesFromSourceArtifactDetails(artifactChanges, project, user);
                }
            }

            artifactChanges.ItemTypeId = itemTypeId; //restore ItemTypeId
            return artifactChanges;
        }

        /// <summary>
        /// Discard changes to artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.DiscardArtifacts(
                artifactsToDiscard,
                address,
                user,
                expectedStatusCodes);
        }

        /// <summary>
        /// Discard changes to artifact(s) on Blueprint server using NOVA endpoint (not OpenAPI).
        /// (Runs:  /svc/shared/artifacts/discard)
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request.</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<NovaDiscardArtifactResult> NovaDiscardArtifacts(string address,
            IUser user,
            List<IArtifactBase> artifactsToDiscard,

            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var discardResults = SvcShared.DiscardArtifacts(address, user, artifactsToDiscard, expectedStatusCodes);

            UpdateStatusOfArtifactsThatWereDiscarded(discardResults, artifactsToDiscard);

            return discardResults;
        }

        /// <summary>
        /// Updates the IsSaved and IsMarkedForDeletion flags to false for all artifacts that were successfully discarded.
        /// </summary>
        /// <param name="discardResults">The results returned from the Discard REST call.</param>
        /// <param name="artifactsToDiscard">The list of artifacts that you attempted to discard.</param>
        public static void UpdateStatusOfArtifactsThatWereDiscarded(List<NovaDiscardArtifactResult> discardResults, List<IArtifactBase> artifactsToDiscard)
        {
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));
            ThrowIf.ArgumentNull(discardResults, nameof(discardResults));

            // When each artifact is successfully discarded, set IsSaved & IsMarkedForDeletion flags to false.
            foreach (var discardedResult in discardResults)
            {
                var discardedArtifact = artifactsToDiscard.Find(a => (a.Id == discardedResult.ArtifactId) &&
                                                                     (discardedResult.Result == NovaDiscardArtifactResult.ResultCode.Success));

                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, (int)discardedResult.Result);

                if (discardedArtifact != null)
                {
                    discardedArtifact.IsSaved = false;
                    discardedArtifact.IsMarkedForDeletion = false;
                }
            }

            Assert.That(discardResults.Count.Equals(artifactsToDiscard.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactsToDiscard.Count, discardResults.Count);
        }

        /// <summary>
        /// Gets the Version property of an Artifact via API call
        /// </summary>
        /// <param name="artifact">The artifact</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The historical version of the artifact.</returns>
        public static int GetVersion(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            return OpenApi.GetArtifactVersion(artifact.Address, artifact, user, expectedStatusCodes);
        }

        /// <summary>
        /// Get discussions for the specified artifact/subartifact
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="includeDraft">false gets discussions for the last published version, true works with draft</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>RaptorDiscussion for artifact/subartifact</returns>
        public static IRaptorDiscussionsInfo GetRaptorDiscussions(string address,
            int itemId,
            bool includeDraft,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetRaptorDiscussions(
                address,
                itemId,
                includeDraft,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        // TODO Move this to a common area, not in artifact/openapiartifact class

        /// <summary>
        /// Search artifact by a substring in its name on Blueprint server.  Among published artifacts only.
        /// (Runs:  'GET svc/shared/artifacts/search')
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring (case insensitive) to search.</param>
        /// <param name="project">The project to search, if project is null search within all available projects.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring.</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            IProject project = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.SearchArtifactsByName(
                address,
                user,
                searchSubstring,
                project,
                expectedStatusCodes);
        }

        /// <summary>
        /// Lock an Artifact.
        /// (Runs:  POST /svc/shared/artifacts/lock  with artifact ID in the request body)
        /// </summary>
        /// <param name="artifact">The artifact to lock.</param>
        /// <param name="address">The base url of the API.</param>
        /// <param name="user">(optional) The user locking the artifact.  If null, it will use the user that created the artifact.</param>
        /// <param name="expectedLockResult">(optional) The expected LockResult returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The artifact lock result information</returns>
        public static LockResultInfo Lock(IArtifactBase artifact,
            string address,
            IUser user = null,
            LockResult expectedLockResult = LockResult.Success,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            if (user == null)
            {
                Assert.NotNull(artifact?.CreatedBy, "No user is available to lock the artifact.");
                user = artifact?.CreatedBy;
            }

            var artifactToLock = new List<IArtifactBase> { artifact };

            var artifactLockResults = LockArtifacts(
                artifactToLock,
                address,
                user,
                new List<LockResult> { expectedLockResult },
                expectedStatusCodes);

            Assert.AreEqual(1, artifactLockResults.Count, "Multiple lock artifact results were returned when 1 was expected.");

            var artifactLockResult = artifactLockResults.First();

            return artifactLockResult;
        }

        /// <summary>
        /// Lock Artifact(s).
        /// (Runs:  'POST /svc/shared/artifacts/lock'  with artifact IDs in the request body)
        /// </summary>
        /// <param name="address">The base URL of the Blueprint server.</param>
        /// <param name="user">The user locking the artifact.</param>
        /// <param name="artifactsToLock">The list of artifacts to lock.</param>
        /// <param name="expectedLockResults">(optional) A list of expected LockResults returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>List of LockResultInfo for the locked artifacts.</returns>
        public static List<LockResultInfo> LockArtifacts(List<IArtifactBase> artifactsToLock,
            string address,
            IUser user,
            List<LockResult> expectedLockResults = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            var lockResults = SvcShared.LockArtifacts(address, user, artifactsToLock, expectedStatusCodes);

            UpdateStatusOfArtifactsThatWereLocked(lockResults, user, artifactsToLock, expectedLockResults);

            return lockResults;
        }

        /// <summary>
        /// Updates the IsLocked and LockOwner flags to false for all artifacts that were successfully locked.
        /// </summary>
        /// <param name="lockResults">The results returned from the Lock REST call.</param>
        /// <param name="user">The user that performed the Lock operation.</param>
        /// <param name="artifactsToLock">The list of artifacts that you attempted to lock.</param>
        /// <param name="expectedLockResults">(optional) The expected LockResults returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        public static void UpdateStatusOfArtifactsThatWereLocked(List<LockResultInfo> lockResults,
            IUser user,
            List<IArtifactBase> artifactsToLock,
            List<LockResult> expectedLockResults = null)
        {
            ThrowIf.ArgumentNull(artifactsToLock, nameof(artifactsToLock));
            ThrowIf.ArgumentNull(lockResults, nameof(lockResults));

            if (expectedLockResults == null)
            {
                expectedLockResults = new List<LockResult> { LockResult.Success };
            }

            // Update artifacts with lock info.
            foreach (var artifact in artifactsToLock)
            {
                var lockResultInfo = lockResults.Find(x => x.Info.ArtifactId == artifact.Id);

                Assert.NotNull(lockResultInfo, "No LockResultInfo was returned for artifact ID {0} after trying to lock it!", artifact.Id);

                if (lockResultInfo.Result == LockResult.Success)
                {
                    artifact.LockOwner = user;
                    artifact.Status.IsLocked = true;
                }

                Assert.That(expectedLockResults.Contains(lockResultInfo.Result),
                    "We expected the lock Result to be one of: [{0}], but it was: {1}!",
                    String.Join(", ", expectedLockResults), lockResultInfo.Result);
            }
        }

        /// <summary>
        /// Publish a single artifact on Blueprint server.  This is only used in Storyteller.
        /// (Runs: 'POST /svc/shared/artifacts/publish')
        /// </summary>
        /// <param name="artifactToPublish">The artifact to publish.</param>
        /// <param name="user">The user saving the artifact.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.  If null, only '200 OK' is expected.</param>
        /// <returns>Result of Publish operation.</returns>
        public static NovaPublishArtifactResult StorytellerPublishArtifact(IArtifactBase artifactToPublish,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToPublish, nameof(artifactToPublish));
            
            var publishResults = SvcShared.PublishArtifacts(artifactToPublish.Address,
                user,
                new List<int> { artifactToPublish.Id },
                expectedStatusCodes);

            if (publishResults[0].StatusCode == NovaPublishArtifactResult.Result.Success)
            {
                artifactToPublish.IsPublished = true;
                artifactToPublish.IsSaved = false;
            }

            return publishResults[0];
        }

        /// <summary>
        /// Creates new discussion for the specified artifact/subartifact using Raptor REST API.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/discussions)
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="itemId">id of artifact/subartifact</param>
        /// <param name="comment">The comment for the new discussion.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorDiscussion for artifact/subartifact.</returns>
        public static IRaptorDiscussion PostRaptorDiscussion(string address,
            int itemId,
            string comment,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussion(address, itemId, comment,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Updates the specified discussion using Raptor REST API.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{artifactId}/discussions/{discussionToUpdateId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="discussionToUpdate">Discussion to update.</param>
        /// <param name="comment">The new comment with status for the discussion.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        public static IRaptorDiscussion UpdateRaptorDiscussion(string address,
            int itemId, IDiscussionAdaptor discussionToUpdate,
            RaptorComment comment,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.UpdateRaptorDiscussion(address, itemId, discussionToUpdate, comment,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Deletes the specified discussion using Raptor REST API.
        /// (Runs: DELETE /svc/components/RapidReview/artifacts/{artifactId}/deletethread/{commentToDeleteId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="discussionToDelete">The discussion to delete.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>message</returns>
        public static string DeleteRaptorDiscussion(string address,
            int itemId, IDiscussionAdaptor discussionToDelete,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.DeleteRaptorDiscussion(address, itemId, discussionToDelete, user, expectedStatusCodes);
        }

        /// <summary>
        /// Creates new reply for the specified Discussion using Raptor REST API.
        /// </summary>
        /// <param name="address">The base url of the Blueprint</param>
        /// <param name="discussion">Discussion to which we want reply.</param>
        /// <param name="comment">Comment for the new reply.</param>
        /// <param name="user">The user credentials to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created Reply for artifact/subartifact discussion.</returns>
        public static IReplyAdapter PostRaptorDiscussionReply(string address,
            IDiscussionAdaptor discussion,
            string comment,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussionReply(address, discussion, comment,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Updates the specified reply.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId}/reply/{replyId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="discussion">comment containing reply to update</param>
        /// <param name="replyToUpdate">reply to update</param>
        /// <param name="comment">The new comment for reply.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The updated Reply.</returns>
        public static IReplyAdapter UpdateRaptorDiscussionReply(string address,
            int itemId, IDiscussionAdaptor discussion, IReplyAdapter replyToUpdate,
            string comment,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.UpdateRaptorDiscussionReply(address, itemId, discussion, replyToUpdate,
                comment, user, expectedStatusCodes);
        }

        /// <summary>
        /// Deletes the specified reply using Raptor REST API.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/deletecomment/{replyToDeleteId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="replyToDelete">The reply to delete.</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>A success or failure message.</returns>
        public static string DeleteRaptorReply(string address,
            int itemId, IReplyAdapter replyToDelete,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.DeleteRaptorReply(address, itemId, replyToDelete, user, expectedStatusCodes);
        }
        
        /// <summary>
        /// add attachment to the specified artifact
        /// </summary>
        /// <param name="address">The base url of the Blueprint</param>
        /// <param name="projectId">Id of project containing artifact to add attachment</param>
        /// <param name="artifactId">Id of artifact to add attachment</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        public static OpenApiAttachment AddArtifactAttachment(string address,
            int projectId, int artifactId, IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.AddArtifactAttachment(address, projectId, artifactId,
                file, user, expectedStatusCodes);
        }

        /// <summary>
        /// add attachment to the specified artifact
        /// </summary>
        /// <param name="address">The base url of the Blueprint</param>
        /// <param name="projectId">Id of project containing artifact to add attachment</param>
        /// <param name="artifactId">Id of artifact to add attachment</param>
        /// <param name="subArtifactId">Id of subartifact to attach file</param>
        /// <param name="file">File to attach</param>
        /// <param name="user">The user to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only '201' is expected.</param>
        /// <returns>OpenApiAttachment object</returns>
        public static OpenApiAttachment AddSubArtifactAttachment(string address,
            int projectId, int artifactId, int subArtifactId, IFile file, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.AddSubArtifactAttachment(address, projectId,
                artifactId, subArtifactId, file, user, expectedStatusCodes);
        }
        
        #endregion Static Methods
    }

    public class ArtifactForUpdate
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }

        public List<PropertyForUpdate> CustomPropertyValues { get; set; }
        public List<PropertyForUpdate> SpecificPropertyValues { get; set; }
    }

    public class PropertyForUpdate
    {
        public int PropertyTypeId { get; set; }
        public int PropertyTypeVersionId { get; set; }
        public int PropertyTypePredefined { get; set; }
        public object Value { get; set; }
    }
}
