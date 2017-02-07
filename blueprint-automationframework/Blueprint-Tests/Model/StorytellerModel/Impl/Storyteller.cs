﻿using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Model.ArtifactModel.Enums;
using Model.ModelHelpers;
using Utilities;
using Utilities.Facades;
using Utilities.Factories;

namespace Model.StorytellerModel.Impl
{
    public class Storyteller : IStoryteller, IArtifactObserver
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        public const string APPLICATION_SETTINGS_TABLE = "[dbo].[ApplicationSettings]";
        public const string STORYTELLER_LIMIT_KEY = "StorytellerShapeLimit";

        public string Address { get; }

        #region Constructor

        public Storyteller(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructor

        #region IArtifactObserver methods

        /// <seealso cref="IArtifactObserver.NotifyArtifactDeleted(IEnumerable{int})" />
        public void NotifyArtifactDeleted(IEnumerable<int> deletedArtifactIds)
        {
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));
            var artifactIds = deletedArtifactIds as int[] ?? deletedArtifactIds.ToArray();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(Storyteller), nameof(NotifyArtifactDeleted), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactDeleted(Artifacts, deletedArtifactIds);
        }

        /// <seealso cref="IArtifactObserver.NotifyArtifactDiscarded(IEnumerable{int})" />
        public void NotifyArtifactDiscarded(IEnumerable<int> discardedArtifactIds)
        {
            ThrowIf.ArgumentNull(discardedArtifactIds, nameof(discardedArtifactIds));
            var artifactIds = discardedArtifactIds as int[] ?? discardedArtifactIds.ToArray();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(Storyteller), nameof(NotifyArtifactDiscarded), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactDiscarded(Artifacts, discardedArtifactIds);
        }

        /// <seealso cref="IArtifactObserver.NotifyArtifactPublished(IEnumerable{int})" />
        public void NotifyArtifactPublished(IEnumerable<int> publishedArtifactIds)
        {
            ThrowIf.ArgumentNull(publishedArtifactIds, nameof(publishedArtifactIds));
            var artifactIds = publishedArtifactIds as int[] ?? publishedArtifactIds.ToArray();

            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(Storyteller), nameof(NotifyArtifactPublished), string.Join(", ", artifactIds));

            ArtifactObserverHelper.NotifyArtifactPublished(Artifacts, publishedArtifactIds);
        }

        #endregion IArtifactObserver methods

        #region Implemented from IStoryteller

        public List<IArtifact> Artifacts { get; } = new List<IArtifact>();

        public List<NovaProcess> NovaProcesses { get; } = new List<NovaProcess>();

        public IArtifact CreateAndSaveProcessArtifact(IProject project, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndSaveProcessArtifact));

            //Create an artifact with ArtifactType and populate all required values without properties
            var artifact = ArtifactFactory.CreateArtifact(Address, user, project, BaseArtifactType.Process);

            //Set to add in root of the project
            artifact.ParentId = artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            artifact.Save(user);

            // Add artifact to artifacts list
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);

            return artifact;
        }

        public List<IArtifact> CreateAndSaveProcessArtifacts(IProject project, IUser user, int numberOfArtifacts)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndSaveProcessArtifacts));

            var artifacts = new List<IArtifact>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = CreateAndSaveProcessArtifact(project, user);
                artifacts.Add(artifact);
            }
            return artifacts;
        }

        public IArtifact CreateAndPublishProcessArtifact(IProject project, IUser user)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndPublishProcessArtifact));

            var publishedArtfiactList = CreateAndPublishProcessArtifacts(project, user, 1);

            Assert.That(publishedArtfiactList.Count().Equals(1),"The expected number of published artifact" +
                                                                " was 1 but response object contains {0} artifacts",
                                                                publishedArtfiactList.Count());
            return publishedArtfiactList[0];
        }

        public List<IArtifact> CreateAndPublishProcessArtifacts(IProject project, IUser user, int numberOfArtifacts)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndPublishProcessArtifacts));

            var artifacts = new List<IArtifact>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = CreateAndSaveProcessArtifact(project, user);
                MarkArtifactAsSaved(artifact.Id);
                artifact.Publish(user);
                MarkArtifactAsPublished(artifact.Id);
                artifacts.Add(artifact);
            }

            return artifacts;
        }

        public NovaProcess CreateAndSaveNovaProcessArtifact(IProject project, IUser user, int? parentId = null,
            double? orderIndex = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndSaveNovaProcessArtifact));

            ThrowIf.ArgumentNull(project, nameof(project));

            parentId = parentId ?? project.Id;

            string artifactName = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(10);
            var novaArtifact = ArtifactStore.CreateArtifact(Address, user, ItemTypePredefined.Process, artifactName,
                project, parentId, orderIndex, expectedStatusCodes);

            var novaProcess = GetNovaProcess(user, novaArtifact.Id);
            UpdateNovaProcess(user, novaProcess,expectedStatusCodes);
            NovaProcesses.Add(novaProcess);

            return novaProcess;
        }

        public List<NovaProcess> CreateAndSaveNovaProcessArtifacts(IProject project, IUser user, int numberOfArtifacts, int? parentId = null,
            double? orderIndex = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndSaveNovaProcessArtifacts));

            var novaProcesses = new List<NovaProcess>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var novaProcess = CreateAndSaveNovaProcessArtifact(project, user, parentId, orderIndex);
                novaProcesses.Add(novaProcess);
            }
            return novaProcesses;
        }

        public NovaProcess CreateAndPublishNovaProcessArtifact(IProject project, IUser user)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndPublishNovaProcessArtifact));

            var publishedNovaProcessList = CreateAndPublishNovaProcessArtifacts(project, user, 1);

            Assert.That(publishedNovaProcessList.Count().Equals(1), "The expected number of published artifact" +
                  " was 1 but response object contains {0} artifacts", publishedNovaProcessList.Count());
            return publishedNovaProcessList[0];
        }

        public List<NovaProcess> CreateAndPublishNovaProcessArtifacts(IProject project, IUser user, int numberOfArtifacts, int? parentId = null,
            double? orderIndex = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(CreateAndPublishNovaProcessArtifacts));

            ThrowIf.ArgumentNull(project, nameof(project));

            var novaProcesses = new List<NovaProcess>();
            var artifacts = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var novaProcess = CreateAndSaveNovaProcessArtifact(project, user, parentId, orderIndex);
                novaProcesses.Add(novaProcess);

                var artifact = new Artifact(Address, novaProcess.Id, project.Id);
                artifacts.Add(artifact);
            }

            ArtifactStore.PublishArtifacts(Address, artifacts, user);

            return novaProcesses;
        }

        /// <seealso cref="IStoryteller.GenerateUserStories(IUser, IProcess, List{HttpStatusCode}, bool, bool)"/>
        public List<IStorytellerUserStory> GenerateUserStories(IUser user,
            IProcess process,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false,
            bool shouldDeleteChildren = true)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(GenerateUserStories));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.Projects_id_.Processes_id_.USERSTORIES, process.ProjectId, process.Id);

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            var additionalHeaders = new Dictionary<string, string>();
            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Generating user stories for process ID: {1}, Name: {2}", nameof(Storyteller), process.Id, process.Name);

            var userstoryResults =  restApi.SendRequestAndDeserializeObject<List<StorytellerUserStory>>(
                path,
                RestRequestMethod.POST,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            // Since Storyteller created the user story artifacts, we aren't tracking them, so we need to tell Delete to also delete children.
            if (shouldDeleteChildren)
            {
                var artifact = Artifacts.Find(a => a.Id == process.Id);
                artifact.ShouldDeleteChildren = true;
            }

            return userstoryResults.ConvertAll(o => (IStorytellerUserStory)o);
        }

        public IProcess GetProcess(IUser user, int artifactId, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(GetProcess));

            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.PROCESSES_id_, artifactId);

            var queryParameters = new Dictionary<string, string>();

            if (versionIndex.HasValue)
            {
                queryParameters.Add("versionId", versionIndex.ToString());
            }

            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Getting the Process with artifact ID: {1}", nameof(Storyteller), artifactId);

            var response = restApi.SendRequestAndDeserializeObject<Process>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies,
                shouldControlJsonChanges: false);

            return response;
        }

        /// <seealso cref="IStoryteller.GetNovaProcess(IUser, int, int?, List{HttpStatusCode})"/>
        public NovaProcess GetNovaProcess(IUser user, int artifactId, int? versionIndex = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return ArtifactStore.GetNovaProcess(Address, user, artifactId, versionIndex, expectedStatusCodes);
        }

        public IList<IProcess> GetProcesses(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(GetProcesses));

            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.Projects_id_.PROCESSES, projectId);
            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Getting all Processes for project ID: {1}", nameof(Storyteller), projectId);

            var response = restApi.SendRequestAndDeserializeObject<List<Process>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies,
                shouldControlJsonChanges: false);

            return response.ConvertAll(o => (IProcess)o);
        }

        /// <seealso cref="Storyteller.GetUserStoryArtifactType(IUser, int, List{HttpStatusCode}, bool)"/>
        public OpenApiArtifactType GetUserStoryArtifactType(IUser user, int projectId, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(GetUserStoryArtifactType));

            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.Projects_id_.ArtifactTypes.USER_STORY, projectId);
            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Getting the User Story Artifact Type for project ID: {1}", nameof(Storyteller), projectId);

            var response = restApi.SendRequestAndDeserializeObject<OpenApiArtifactType>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies,
                shouldControlJsonChanges: false);

            return response;
        }

        public IProcess UpdateProcess(IUser user, IProcess process, bool lockArtifactBeforeUpdate = true, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UpdateProcess));

            ThrowIf.ArgumentNull(process, nameof(process));

            if (lockArtifactBeforeUpdate)
            {
                var artifactToLock = Artifacts.Find(a => a.Id == process.Id);

                if (!artifactToLock.Status.IsLocked)
                {
                    // Lock process artifact before update
                    artifactToLock.Lock(user);
                }
            }

            var restResponse = UpdateProcessAndGetRestResponse(user, process, expectedStatusCodes, sendAuthorizationAsCookie);
            var updatedProcess = JsonConvert.DeserializeObject<Process>(restResponse.Content);

            return updatedProcess;
        }

        /// <seealso cref="IStoryteller.UpdateNovaProcess(IUser, NovaProcess, List{HttpStatusCode})"/>
        public NovaProcess UpdateNovaProcess(IUser user, NovaProcess novaProcess, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return ArtifactStore.UpdateNovaProcess(Address, user, novaProcess, expectedStatusCodes);
        }

        /// <seealso cref="IStoryteller.UpdateProcessReturnResponseOnly(IUser, IProcess, bool, List{HttpStatusCode})"/>
        public string UpdateProcessReturnResponseOnly(IUser user, IProcess process, bool lockArtifactBeforeUpdate = true, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UpdateProcessReturnResponseOnly));

            ThrowIf.ArgumentNull(process, nameof(process));

            if (lockArtifactBeforeUpdate)
            {
                var artifactToLock = Artifacts.Find(a => a.Id == process.Id);

                if (!artifactToLock.Status.IsLocked)
                {
                    // Lock process artifact before update
                    artifactToLock.Lock(user);
                }
            }

            var restResponse = UpdateProcessAndGetRestResponse(user, process, expectedStatusCodes);

            return restResponse.Content;
        }

        /// <seealso cref="IStoryteller.UploadFile(IUser, IFile, DateTime?, List{HttpStatusCode})"/>
        public string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return SvcComponents.UploadFile(Address, user, file, expireDate, expectedStatusCodes);
        }

        /// <seealso cref="IStoryteller.PublishProcess(IUser, IProcess, List{HttpStatusCode}, bool)"/>
        public string PublishProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(PublishProcess));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.OK };
            }

            const string path = RestPaths.Svc.Shared.Artifacts.PUBLISH;
            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Publishing Process ID: {1}, name: {2}", nameof(Storyteller), process.Id, process.Name);
            var publishResults = restApi.SendRequestAndDeserializeObject<List<NovaPublishArtifactResult>, List<int>>(
                path, 
                RestRequestMethod.POST, 
                new List<int> { process.Id },
                expectedStatusCodes: expectedStatusCodes);

            if (publishResults?[0].StatusCode == NovaPublishArtifactResult.Result.Success)
            {
                // Mark artifact in artifact list as published.
                MarkArtifactAsPublished(process.Id);
            }

            return restApi.Content;
        }

        /// <seealso cref="IStoryteller.DiscardProcessArtifact(IArtifact, List{HttpStatusCode})"/>
        public List<DiscardArtifactResult> DiscardProcessArtifact(IArtifact artifact,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DiscardProcessArtifact));

            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var discardedArtifacts = artifact.Discard(artifact.CreatedBy, expectedStatusCodes);
            var discardedArtifactIds = discardedArtifacts.Select(a => a.ArtifactId);

            NotifyArtifactDiscarded(discardedArtifactIds);

            return discardedArtifacts;
        }

        /// <seealso cref="IStoryteller.DeleteProcessArtifact(IArtifact, bool?, List{HttpStatusCode})"/>
        public List<OpenApiDeleteArtifactResult> DeleteProcessArtifact(IArtifact artifact, bool? deleteChildren = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DeleteProcessArtifact));

            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            var deletedArtifacts = artifact.Delete(artifact.CreatedBy, deleteChildren: deleteChildren, expectedStatusCodes: expectedStatusCodes);
            var deletedArtifactIds = deletedArtifacts.Select(a => a.ArtifactId);

            NotifyArtifactDeleted(deletedArtifactIds);

            return deletedArtifacts;
        }

        /// <seealso cref="IStoryteller.DeleteNovaProcessArtifact(IUser, NovaProcess, List{HttpStatusCode})"/>
        public List<NovaArtifact> DeleteNovaProcessArtifact(IUser user, NovaProcess novaProcess, List<HttpStatusCode> expectedStatusCodes = null)
        {
            return ArtifactStore.DeleteNovaProcessArtifact(Address, user, novaProcess, expectedStatusCodes);
        }
        
        public int GetStorytellerShapeLimitFromDb
        {
            get
            {
                string query = I18NHelper.FormatInvariant("SELECT [Value] FROM {0} WHERE [Key] = '{1}'",
                        Storyteller.APPLICATION_SETTINGS_TABLE, Storyteller.STORYTELLER_LIMIT_KEY);
                var result = DatabaseHelper.ExecuteSingleValueSqlQuery<int>(query, "Value");
                return ParseStorytellerLimitFromDb(result);
            }
        }

        #endregion Implemented from IStoryteller

        #region Members inherited from IDisposable

        private bool _isDisposed = false;

        /// <summary>
        /// Disposes this object by deleting all artifacts that were created.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly disposing or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(Storyteller), nameof(Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                // Delete all the artifacts that were created.
                if (Artifacts != null)
                {
                    Logger.WriteDebug("Deleting/Discarding all artifacts created by this Storyteller instance...");
                    ArtifactBase.DisposeArtifacts(Artifacts.ConvertAll(o => (IArtifactBase)o), this);
                }
            }

            _isDisposed = true;

            Logger.WriteTrace("{0}.{1} finished.", nameof(Storyteller), nameof(Dispose));
        }

        /// <summary>
        /// Disposes this object by deleting all sessions that were created.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable

        #region Static Methods

        /// <summary>
        /// Discard the added process artifact(s) from Blueprint
        /// </summary>
        /// <param name="artifactsToDiscard">The process artifact(s) to be discarded.</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user to authenticate to Blueprint.</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardProcessArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DiscardProcessArtifacts));

            return Artifact.DiscardArtifacts(artifactsToDiscard, address, user, expectedStatusCodes);
        }

        /// <summary>
        /// Publish Process Artifact(s) (Used when publishing a single process artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of process artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<OpenApiPublishArtifactResult> PublishProcessArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool shouldKeepLock = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(PublishProcessArtifacts));

            return ArtifactBase.PublishArtifacts(
                artifactsToPublish, 
                address, 
                user, 
                shouldKeepLock, 
                expectedStatusCodes);
        }

        #endregion Static Methods

        #region Private Methods

        /// <summary>
        /// Mark the Artifact as Published (Indicates artifact has no pending changes)
        /// </summary>
        /// <param name="artifactId">The id of the artifact to be published</param>
        private void MarkArtifactAsPublished(int artifactId)
        {
            var publishedArtifact = Artifacts.Find(artifact => artifact.Id == artifactId);
            publishedArtifact.IsSaved = false;
            publishedArtifact.IsPublished = true;
            publishedArtifact.Status.IsLocked = false;
        }

        /// <summary>
        /// Mark the Artifact as Saved (Indicates artifact has pending changes)
        /// </summary>
        /// <param name="artifactId">The id of the artifact to be saved</param>
        private void MarkArtifactAsSaved(int artifactId)
        {
            var publishedArtifact = Artifacts.Find(artifact => artifact.Id == artifactId);
            publishedArtifact.IsSaved = true;
            publishedArtifact.Status.IsLocked = true;
        }

        /// <summary>
        /// Update a Process but only return the RestResponse object.
        /// </summary>
        /// <param name="user">The user credentials for the request to update a process</param>
        /// <param name="process">The process to update</param>
        /// <param name="expectedStatusCodes">(optional) Expected status codes for the request</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The RestResponse object returned by the update process request</returns>
        private RestResponse UpdateProcessAndGetRestResponse(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UpdateProcess));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(process, nameof(process));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.PROCESSES_id_, process.Id);
            var restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Updating Process ID: {1}, Name: {2}", nameof(Storyteller), process.Id, process.Name);

            var processBodyObject = (Process)process;

            restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                bodyObject: processBodyObject,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            // Mark artifact in artifact list as saved
            MarkArtifactAsSaved(process.Id);

            // Get restResponse using get process
            var restResponse = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.GET,
                bodyObject: processBodyObject,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return restResponse;
        }
        
        /// <summary>
        /// Parses the result from the database to an int value for Storyteller shape limit
        /// </summary>
        /// <param name="result">The result from the database</param>
        /// <returns>Number value of the shape limit</returns>
        /// <exception cref="ArgumentNullException">If key value does not exist in database, will throw an ArgumentNullException.</exception>
        private static int ParseStorytellerLimitFromDb(object result)
        {
            int returnVal;
            if (result != null && Int32.TryParse(result.ToString(), out returnVal))
            {
                return returnVal;
            }
            var errorMessage =
                string.Format(CultureInfo.InvariantCulture,
                    "Could not find {0} value from the {1} table. Please check that the migration.sql ran propertly.",
                    STORYTELLER_LIMIT_KEY, APPLICATION_SETTINGS_TABLE);
            throw new ArgumentNullException(errorMessage);
        }
        
        #endregion Private Methods

    }
}
