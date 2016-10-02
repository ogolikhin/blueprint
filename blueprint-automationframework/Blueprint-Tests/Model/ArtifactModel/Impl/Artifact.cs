using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Utilities;
using NUnit.Framework;
using Utilities.Facades;
using Common;
using Model.ArtifactModel.Enums;
using Model.Factories;
using Model.Impl;
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

        public void Save(IUser user = null,
            bool shouldGetLockForUpdate = true,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
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

            SaveArtifact(this, user, shouldGetLockForUpdate, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        public List<DiscardArtifactResult> Discard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
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
                expectedStatusCodes, 
                sendAuthorizationAsCookie);

            foreach (var discardArtifactResult in discardArtifactResults)
            {
                if (discardArtifactResult.ResultCode == HttpStatusCode.OK)
                {
                    IsSaved = false;
                }
            }

            return discardArtifactResults;
        }

        public List<NovaDiscardArtifactResult> NovaDiscard(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform Discard.");
                user = CreatedBy;
            }

            var artifactsToDiscard = new List<IArtifactBase> { this };

            var discardArtifactResults = NovaDiscardArtifacts(
                artifactsToDiscard,
                Address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);

            foreach (var discardArtifactResult in discardArtifactResults)
            {
                if (discardArtifactResult.Result == NovaDiscardArtifactResult.ResultCode.Success)
                {
                    IsSaved = false;
                    IsMarkedForDeletion = false;
                }
            }

            return discardArtifactResults;
        }

        public int GetVersion(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetVersion.");
                user = CreatedBy;
            }

            int artifactVersion = GetVersion(this, user, expectedStatusCodes, sendAuthorizationAsCookie);

            return artifactVersion;
        }

        /// <seealso cref="IArtifact.Lock(IUser, LockResult, List{HttpStatusCode}, bool)"/>
        public LockResultInfo Lock(IUser user = null,
            LockResult expectedLockResult = LockResult.Success,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return Lock(this, Address, user, expectedLockResult, expectedStatusCodes, sendAuthorizationAsCookie);
        }

        public ArtifactInfo GetArtifactInfo(IUser user = null,
           List<HttpStatusCode> expectedStatusCodes = null,
           bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetArtifactInfo.");
                user = CreatedBy;
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.ARTIFACT_INFO_id_, Id);

            var returnedArtifactInfo = restApi.SendRequestAndDeserializeObject<ArtifactInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactInfo;
        }

        public RapidReviewDiagram GetDiagramContentForRapidReview(IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            if (user == null)
            {
                Assert.NotNull(CreatedBy, "No user is available to perform GetDiagramContentForRapidReview.");
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
            string path;
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

            switch (artifactInfo.BaseTypePredefined)
            {
                case ItemTypePredefined.BusinessProcess:
                case ItemTypePredefined.DomainDiagram:
                case ItemTypePredefined.GenericDiagram:
                case ItemTypePredefined.Storyboard:
                case ItemTypePredefined.UseCaseDiagram:
                case ItemTypePredefined.UIMockup:
                    path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.DIAGRAM_id_, Id);
                    break;
                default:
                    throw new ArgumentException("Method works for graphical artifacts only.");
            }

            var diagramContent = restApi.SendRequestAndDeserializeObject<RapidReviewDiagram>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return diagramContent;
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
                RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

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
                RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

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
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

            var returnedArtifactProperties = restApi.SendRequestAndDeserializeObject<List<RapidReviewProperties>, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactProperties[0];
        }

        public NovaPublishArtifactResult NovaPublish(IUser user = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            return PublishArtifact(artifactToPublish: this, user: user, expectedStatusCodes: expectedStatusCodes,
                sendAuthorizationAsCookie: sendAuthorizationAsCookie);
        }

        public IRaptorComment PostRaptorDiscussions(string discussionsText,
            IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            return PostRaptorDiscussions(Address, Id, discussionsText, user, expectedStatusCodes);
        }

        /// <seealso cref="IArtifact.UpdateRaptorDiscussions(string, IUser, IRaptorComment, List{HttpStatusCode})"/>
        public IRaptorComment UpdateRaptorDiscussions(string discussionText,
            IUser user, IRaptorComment commentToUpdate,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(commentToUpdate, nameof(commentToUpdate));
            return UpdateRaptorDiscussion(Address, Id, commentToUpdate, discussionText, user, expectedStatusCodes);
        }

        public string DeleteRaptorDiscussion(IUser user, IRaptorComment commentToDelete,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(commentToDelete, nameof(commentToDelete));
            return DeleteRaptorDiscussion(Address, Id, commentToDelete, user, expectedStatusCodes);
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
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false).</param>
        public static void SaveArtifact(IArtifactBase artifactToSave,
            IUser user,
            bool shouldGetLockForUpdate = true,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
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
                OpenApiArtifact.SaveArtifact(artifactToSave, user, expectedStatusCodes, sendAuthorizationAsCookie);
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
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToUpdate, nameof(artifactToUpdate));

            string tokenValue = user.Token?.AccessControlToken;
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.ArtifactStore.ARTIFACTS_id_, artifactToUpdate.Id);
            NovaArtifactDetails artifactChanges = artifactDetailsChanges;

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
            artifactChanges.ItemTypeId = null;

            if (address == null)
            {
                address = artifactToUpdate.Address;
            }

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);
            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                bodyObject: artifactChanges,
                expectedStatusCodes: expectedStatusCodes);

            if ((expectedStatusCodes == null) || expectedStatusCodes.Contains(HttpStatusCode.OK))
            {
                artifactToUpdate.IsSaved = true;

                if (user?.Token?.OpenApiToken == null)
                {
                    // We need an OpenAPI token to make the GetProject call below.
                    Assert.NotNull(artifactToUpdate.Project, "Project is null and we don't have an OpenAPI token!");
                }

                IProject project = artifactToUpdate.Project ?? ProjectFactory.CreateProject().GetProject(address, artifactToUpdate.ProjectId, user);
                ArtifactBase artifactBaseToUpdate = artifactToUpdate as ArtifactBase;

                // Copy updated properties into original artifact.
                if (artifactDetailsChanges == null)
                {
                    artifactBaseToUpdate.Id = artifactToUpdate.Id;
                    artifactBaseToUpdate.ProjectId = artifactToUpdate.ProjectId;
                    artifactBaseToUpdate.Version = artifactToUpdate.Version;
                    artifactBaseToUpdate.AddOrReplaceTextOrChoiceValueProperty("Description", artifactChanges.Description, project, user);
                }
                else
                {
                    artifactBaseToUpdate.ReplacePropertiesWithPropertiesFromSourceArtifactDetails(artifactChanges, project, user);
                }
            }

            return artifactChanges;
        }

        /// <summary>
        /// Discard changes to artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.DiscardArtifacts(
                artifactsToDiscard,
                address,
                user,
                expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Discard changes to artifact(s) on Blueprint server using NOVA endpoint (not OpenAPI).
        /// (Runs:  /svc/shared/artifacts/discard)
        /// </summary>
        /// <param name="artifactsToDiscard">The artifact(s) having changes to be discarded.</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<NovaDiscardArtifactResult> NovaDiscardArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToDiscard, nameof(artifactsToDiscard));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            var artifactsIds = artifactsToDiscard.Select(artifact => artifact.Id).ToList();
            var artifactResults = restApi.SendRequestAndDeserializeObject<NovaDiscardArtifactResults, List<int>>(
                RestPaths.Svc.Shared.Artifacts.DISCARD,
                RestRequestMethod.POST,
                artifactsIds,
                expectedStatusCodes: expectedStatusCodes);

            var discardedResultList = artifactResults.DiscardResults;

            // When each artifact is successfully discarded, set IsSaved & IsMarkedForDeletion flags to false.
            foreach (var discardedResult in discardedResultList)
            {
                var discardedArtifact = artifactsToDiscard.Find(a => a.Id.Equals(discardedResult.ArtifactId) &&
                    discardedResult.Result == NovaDiscardArtifactResult.ResultCode.Success);

                Logger.WriteDebug("Result Code for the Discarded Artifact {0}: {1}", discardedResult.ArtifactId, (int)discardedResult.Result);

                if (discardedArtifact != null)
                {
                    discardedArtifact.IsSaved = false;
                    discardedArtifact.IsMarkedForDeletion = false;
                }
            }

            Assert.That(discardedResultList.Count.Equals(artifactsToDiscard.Count),
                "The number of artifacts passed for Discard was {0} but the number of artifacts returned was {1}",
                artifactsToDiscard.Count, discardedResultList.Count);

            return discardedResultList;
        }

        /// <summary>
        /// Gets the Version property of an Artifact via API call
        /// </summary>
        /// <param name="artifact">The artifact</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The historical version of the artifact.</returns>
        public static int GetVersion(IArtifactBase artifact,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            return OpenApiArtifact.GetVersion(artifact, user, expectedStatusCodes, sendAuthorizationAsCookie);
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
        public static IRaptorDiscussion GetRaptorDiscussions(string address,
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
        /// Search artifact by a substring in its name on Blueprint server. Among published artifacts only.
        /// </summary>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="searchSubstring">The substring(case insensitive) to search.</param>
        /// <param name="project">The project to search, if project is null search within all available projects.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Send session token as cookie instead of header</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes.</param>
        /// <returns>List of first 10 artifacts with name containing searchSubstring</returns>
        public static IList<IArtifactBase> SearchArtifactsByName(string address,
            IUser user,
            string searchSubstring,
            IProject project = null,
            bool sendAuthorizationAsCookie = false,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.SearchArtifactsByName(
                address,
                user,
                searchSubstring,
                project,
                sendAuthorizationAsCookie,
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
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false).</param>
        /// <returns>The artifact lock result information</returns>
        public static LockResultInfo Lock(IArtifactBase artifact,
            string address,
            IUser user = null,
            LockResult expectedLockResult = LockResult.Success,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
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
                expectedStatusCodes,
                sendAuthorizationAsCookie);

            Assert.That(artifactLockResults.Count == 1, "Multiple lock artifact results were returned when 1 was expected.");

            var artifactLockResult = artifactLockResults.First();

            return artifactLockResult;
        }

        /// <summary>
        /// Lock Artifact(s).
        /// (Runs:  POST /svc/shared/artifacts/lock  with artifact IDs in the request body)
        /// </summary>
        /// <param name="artifactsToLock">The list of artifacts to lock</param>
        /// <param name="address">The base url of the API</param>
        /// <param name="user">The user locking the artifact</param>
        /// <param name="expectedLockResults">(optional) A list of expected LockResults returned in the JSON body.  This is only checked if StatusCode = 200.
        ///     If null, only Success is expected.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>List of LockResultInfo for the locked artifacts</returns>
        public static List<LockResultInfo> LockArtifacts(List<IArtifactBase> artifactsToLock,
            string address,
            IUser user,
            List<LockResult> expectedLockResults = null,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactsToLock, nameof(artifactsToLock));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var artifactIds = (
                from IArtifactBase artifact in artifactsToLock
                select artifact.Id).ToList();

            var restApi = new RestApiFacade(address, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<LockResultInfo>, List<int>>(
                RestPaths.Svc.Shared.Artifacts.LOCK,
                RestRequestMethod.POST,
                jsonObject: artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            if (expectedLockResults == null)
            {
                expectedLockResults = new List<LockResult> { LockResult.Success };
            }

            // Update artifacts with lock info.
            foreach (var artifact in artifactsToLock)
            {
                var lockResultInfo = response.Find(x => x.Info.ArtifactId == artifact.Id);

                Assert.NotNull(lockResultInfo, "No LockResultInfo was returned for artifact ID {0} after trying to lock it!", artifact.Id);

                if (lockResultInfo.Result == LockResult.Success)
                {
                    artifact.LockOwner = user;
                }

                if (restApi.StatusCode == HttpStatusCode.OK)
                {
                    Assert.That(expectedLockResults.Contains(lockResultInfo.Result),
                        "We expected the lock Result to be one of: [{0}], but it was: {1}!",
                        string.Join(", ", expectedLockResults), lockResultInfo.Result);
                }
            }

            return response;
        }

        /// <summary>
        /// Publish a single artifact on Blueprint server.
        /// (Runs: /svc/shared/artifacts/publish)
        /// </summary>
        /// <param name="artifactToPublish">The artifact to publish</param>
        /// <param name="user">The user saving the artifact</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>Resut of Publish operation</returns>
        ///TODO: override the Publish() function in the Artifact class to call this PublishArtifact() function
        public static NovaPublishArtifactResult PublishArtifact(IArtifactBase artifactToPublish,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifactToPublish, nameof(artifactToPublish));
            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            const string path = RestPaths.Svc.Shared.Artifacts.PUBLISH;
            RestApiFacade restApi = new RestApiFacade(artifactToPublish.Address, tokenValue);

            var publishResults = restApi.SendRequestAndDeserializeObject<List<NovaPublishArtifactResult>, List<int>>(path, RestRequestMethod.POST,
                new List<int> { artifactToPublish.Id },
                expectedStatusCodes: expectedStatusCodes);

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
        /// <param name="discussionsText">text for the new discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorComment for artifact/subartifact</returns>
        public static IRaptorComment PostRaptorDiscussions(string address,
            int itemId,
            string discussionsText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussion(address, itemId, discussionsText,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Updates the specified comment using Raptor REST API.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{artifactId}/discussions/{commentToUpdateId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="commentToUpdate">comment to update</param>
        /// <param name="discussionText">new text for discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        public static IRaptorComment UpdateRaptorDiscussion(string address,
            int itemId, IRaptorComment commentToUpdate,
            string discussionText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.UpdateRaptorDiscussion(address, itemId, commentToUpdate, discussionText,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Deletes the specified comment using Raptor REST API.
        /// (Runs: DELETE /svc/components/RapidReview/artifacts/{artifactId}/deletethread/{commentToDeleteId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="commentToDelete">comment to delete</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>message</returns>
        public static string DeleteRaptorDiscussion(string address,
            int itemId, IRaptorComment commentToDelete,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.DeleteRaptorDiscussion(address, itemId, commentToDelete, user, expectedStatusCodes);
        }

        /// <summary>
        /// Creates new reply for the specified Comment using Raptor REST API
        /// </summary>
        /// <param name="address">The base url of the Blueprint</param>
        /// <param name="comment">Comment to which we want reply</param>
        /// <param name="replyText">text for the new reply</param>
        /// <param name="user">The user credentials to authenticate with</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>Newly created RaptorReply for artifact/subartifact comment</returns>
        public static IRaptorReply PostRaptorDiscussionReply(string address,
            IRaptorComment comment,
            string replyText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.PostRaptorDiscussionReply(address, comment, replyText,
                user, expectedStatusCodes);
        }

        /// <summary>
        /// Updates the specified reply.
        /// (Runs: PATCH /svc/components/RapidReview/artifacts/{itemId}/discussions/{discussionId}/reply/{replyId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="comment">comment containing reply to update</param>
        /// <param name="replyToUpdate">reply to update</param>
        /// <param name="newDiscussionText">new text for discussion</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>updated RaptorDiscussion</returns>
        public static IRaptorReply UpdateRaptorDiscussionReply(string address,
            int itemId, IRaptorComment comment, IRaptorReply replyToUpdate,
            string newDiscussionText,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            return OpenApiArtifact.UpdateRaptorDiscussionReply(address, itemId, comment, replyToUpdate,
                newDiscussionText, user, expectedStatusCodes);
        }

        /// <summary>
        /// Deletes the specified reply using Raptor REST API.
        /// (Runs: /svc/components/RapidReview/artifacts/{artifactId}/deletecomment/{replyToDeleteId})
        /// </summary>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="itemId">id of artifact</param>
        /// <param name="replyToDelete">comment to delete</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>message</returns>
        public static string DeleteRaptorReply(string address,
            int itemId, IRaptorReply replyToDelete,
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

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public List<PropertyForUpdate> CustomPropertyValues { get; set; }
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
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
