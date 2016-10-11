using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.Factories;
using Model.Impl;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

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

        /// <seealso cref="IArtifactObserver.NotifyArtifactDeletion(IEnumerable{int})" />
        public void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds)
        {
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));
            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(Storyteller), nameof(Storyteller.NotifyArtifactDeletion), string.Join(", ", deletedArtifactIds));

            foreach (var deletedArtifactId in deletedArtifactIds)
            {
                Artifacts.ForEach(a =>
                {
                    if (a.Id == deletedArtifactId)
                    {
                        a.IsDeleted = true;
                        a.IsPublished = false;
                        a.IsSaved = false;
                    }
                });
                Artifacts.RemoveAll(a => a.Id == deletedArtifactId);
            }
        }

        /// <seealso cref="IArtifactObserver.NotifyArtifactPublish(IEnumerable{int})" />
        public void NotifyArtifactPublish(IEnumerable<int> publishedArtifactIds)
        {
            ThrowIf.ArgumentNull(publishedArtifactIds, nameof(publishedArtifactIds));
            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(Storyteller), nameof(Storyteller.NotifyArtifactPublish), String.Join(", ", publishedArtifactIds));

            foreach (var publishedArtifactId in publishedArtifactIds)
            {
                Artifacts.ForEach(a =>
                {
                    if (a.Id == publishedArtifactId)
                    {
                        if (a.IsMarkedForDeletion)
                        {
                            a.IsDeleted = true;
                            a.IsPublished = false;
                        }
                        else
                        {
                            a.IsPublished = true;
                        }

                        a.IsSaved = false;
                    }
                });
                Artifacts.RemoveAll(a => a.Id == publishedArtifactId);
            }
        }

        #endregion IArtifactObserver methods

        #region Implemented from IStoryteller

        public List<IArtifact> Artifacts { get; } = new List<IArtifact>();

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

        /// <seealso cref="IStoryteller.GenerateUserStories(IUser, IProcess, List{HttpStatusCode}, bool)"/>
        public List<IStorytellerUserStory> GenerateUserStories(IUser user,
            IProcess process,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
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
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Generating user stories for process ID: {1}, Name: {2}", nameof(Storyteller), process.Id, process.Name);
            List<StorytellerUserStory> userstoryResults = null;

            userstoryResults = restApi.SendRequestAndDeserializeObject<List<StorytellerUserStory>>(
                path,
                RestRequestMethod.POST,
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            // Since Storyteller created the user story artifacts, we aren't tracking them, so we need to tell Delete to also delete children.
            var artifact = Artifacts.Find(a => a.Id == process.Id);
            artifact.ShouldDeleteChildren = true;

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
                cookies: cookies);

            return response;
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
                cookies: cookies);

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
                cookies: cookies);

            return response;
        }

        public IProcess UpdateProcess(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UpdateProcess));

            var restResponse = UpdateProcessAndGetRestResponse(user, process, expectedStatusCodes, sendAuthorizationAsCookie);
            var updateProcessResult = JsonConvert.DeserializeObject<UpdateResult<Process>>(restResponse.Content);

            return updateProcessResult.Result;
        }

        public string UpdateProcessReturnResponseOnly(IUser user, IProcess process, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UpdateProcessReturnResponseOnly));

            var restResponse = UpdateProcessAndGetRestResponse(user, process, expectedStatusCodes, sendAuthorizationAsCookie);

            return restResponse.Content;
        }

        public string UploadFile(IUser user, IFile file, DateTime? expireDate = null, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(UploadFile));

            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(file, nameof(file));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var additionalHeaders = new Dictionary<string, string>();
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.FileStore.FILES_filename_, file.FileName);

            if (expireDate != null)
            {
                DateTime time = (DateTime)expireDate;
                path = I18NHelper.FormatInvariant("{0}/?expired={1}", path, time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture));
            }

            byte[] bytes = file.Content.ToArray();
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Uploading a file named: {1}, size: {2}", nameof(Storyteller), file.FileName, bytes.Length);

            var artifactResult = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                fileName: file.FileName,
                fileContent: bytes,
                contentType: "application/json;charset=utf8",
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return artifactResult.Content;
        }

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
            RestApiFacade restApi = new RestApiFacade(Address, tokenValue);

            Logger.WriteInfo("{0} Publishing Process ID: {1}, name: {2}", nameof(Storyteller), process.Id, process.Name);
            restApi.SendRequestAndDeserializeObject<List<PublishArtifactResult>, List<int>>(path, RestRequestMethod.POST, new List<int> { process.Id },
                expectedStatusCodes: expectedStatusCodes);

            // Mark artifact in artifact list as published
            MarkArtifactAsPublished(process.Id);

            return restApi.Content;
        }

        public List<DiscardArtifactResult> DiscardProcessArtifact(IArtifact artifact,
            List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DiscardProcessArtifact));

            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Artifacts.Remove(Artifacts.First(i => i.Id.Equals(artifact.Id)));
            return artifact.Discard(artifact.CreatedBy, expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie);
        }

        public List<DeleteArtifactResult> DeleteProcessArtifact(IArtifact artifact, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false, bool? deleteChildren = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DeleteProcessArtifact));

            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Artifacts.Remove(Artifacts.First(i => i.Id.Equals(artifact.Id)));
            return artifact.Delete(artifact.CreatedBy, expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie, deleteChildren: deleteChildren);
        }
        
        public int GetStorytellerShapeLimitFromDb
        {
            get
            {
                using (IDatabase database = DatabaseFactory.CreateDatabase())
                {
                    database.Open();
                    string query = I18NHelper.FormatInvariant("SELECT [Value] FROM {0} WHERE [Key] = '{1}'",
                        Storyteller.APPLICATION_SETTINGS_TABLE, Storyteller.STORYTELLER_LIMIT_KEY);

                    Logger.WriteDebug("Running: {0}", query);
                    using (SqlCommand cmd = database.CreateSqlCommand(query))
                    {
                        var result = cmd.ExecuteScalar();
                        return ParseStorytellerLimitFromDb(result);
                    }
                }
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
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of ArtifactResult objects created by the dicard artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<DiscardArtifactResult> DiscardProcessArtifacts(List<IArtifactBase> artifactsToDiscard,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(DiscardProcessArtifacts));

            return Artifact.DiscardArtifacts(artifactsToDiscard, address, user, expectedStatusCodes,
                sendAuthorizationAsCookie);
        }

        /// <summary>
        /// Publish Process Artifact(s) (Used when publishing a single process artifact OR a list of artifacts)
        /// </summary>
        /// <param name="artifactsToPublish">The list of process artifacts to publish</param>
        /// <param name="address">The base url of the Open API</param>
        /// <param name="user">The user credentials for the request</param>
        /// <param name="shouldKeepLock">(optional) Boolean parameter which defines whether or not to keep the lock after publishing the artfacts</param>
        /// <param name="expectedStatusCodes">(optional) A list of expected status codes. If null, only OK: '200' is expected.</param>
        /// <param name="sendAuthorizationAsCookie">(optional) Flag to send authorization as a cookie rather than an HTTP header (Default: false)</param>
        /// <returns>The list of PublishArtifactResult objects created by the publish artifacts request</returns>
        /// <exception cref="WebException">A WebException sub-class if request call triggers an unexpected HTTP status code.</exception>
        public static List<PublishArtifactResult> PublishProcessArtifacts(List<IArtifactBase> artifactsToPublish,
            string address,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool shouldKeepLock = false,
            bool sendAuthorizationAsCookie = false)
        {
            Logger.WriteTrace("{0}.{1}", nameof(Storyteller), nameof(PublishProcessArtifacts));

            return Artifact.PublishArtifacts(
                artifactsToPublish, 
                address, 
                user, 
                shouldKeepLock, 
                expectedStatusCodes,
                sendAuthorizationAsCookie);
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
        }

        /// <summary>
        /// Mark the Artifact as Saved (Indicates artifact has pending changes)
        /// </summary>
        /// <param name="artifactId">The id of the artifact to be saved</param>
        private void MarkArtifactAsSaved(int artifactId)
        {
            var publishedArtifact = Artifacts.Find(artifact => artifact.Id == artifactId);
            publishedArtifact.IsSaved = true;
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

            var restResponse = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.PATCH,
                bodyObject: (Process)process,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            // Mark artifact in artifact list as saved
            MarkArtifactAsSaved(process.Id);

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
