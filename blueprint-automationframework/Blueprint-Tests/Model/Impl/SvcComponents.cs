using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class SvcComponents : NovaServiceBase, ISvcComponents
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="address">The base URI of the svc/components service.</param>
        public SvcComponents(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #region Members inherited from ISvcComponents

        #region FileStore methods

        /// <seealso cref="ISvcComponents.UploadFile(IUser, IFile, DateTime?, List{HttpStatusCode})"/>
        public string UploadFile(
            IUser user,
            IFile file,
            DateTime? expireDate = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(UploadFile));

            ThrowIf.ArgumentNull(file, nameof(file));

            if (expectedStatusCodes == null)
            {
                expectedStatusCodes = new List<HttpStatusCode> { HttpStatusCode.Created };
            }

            var additionalHeaders = new Dictionary<string, string>();
            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.FileStore.FILES_filename_, file.FileName);

            if (expireDate != null)
            {
                DateTime time = (DateTime)expireDate;
                path = I18NHelper.FormatInvariant("{0}/?expired={1}",
                    path, time.ToUniversalTime().ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff'Z'", CultureInfo.InvariantCulture));
            }

            byte[] bytes = file.Content.ToArray();
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Uploading a file named: {1}, size: {2}", nameof(SvcComponents), file.FileName, bytes.Length);

            var artifactResult = restApi.SendRequestAndGetResponse(
                path,
                RestRequestMethod.POST,
                fileName: file.FileName,
                fileContent: bytes,
                contentType: "application/json;charset=utf8",
                additionalHeaders: additionalHeaders,
                expectedStatusCodes: expectedStatusCodes);

            return artifactResult.Content;
        }

        #endregion FileStore methods

        #region RapidReview methods

        /// <seealso cref="ISvcComponents.GetRapidReviewDiagramContent(IUser, int, List{HttpStatusCode})"/>
        public RapidReviewDiagram GetRapidReviewDiagramContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetRapidReviewDiagramContent));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.DIAGRAM_id_, artifactId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            var diagramContent = restApi.SendRequestAndDeserializeObject<RapidReviewDiagram>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return diagramContent;
        }

        /// <seealso cref="ISvcComponents.GetRapidReviewGlossaryContent(IUser, int, List{HttpStatusCode})"/>
        public RapidReviewGlossary GetRapidReviewGlossaryContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string tokenValue = user?.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.GLOSSARY_id_, artifactId);
            var restApi = new RestApiFacade(Address, tokenValue);

            var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewGlossary>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactContent;
        }

        /// <seealso cref="ISvcComponents.GetRapidReviewUseCaseContent(IUser, int, List{HttpStatusCode})"/>
        public RapidReviewUseCase GetRapidReviewUseCaseContent(
            IUser user,
            int artifactId,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string tokenValue = user?.Token?.AccessControlToken;

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.RapidReview.USECASE_id_, artifactId);
            var restApi = new RestApiFacade(Address, tokenValue);

            var returnedArtifactContent = restApi.SendRequestAndDeserializeObject<RapidReviewUseCase>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactContent;
        }

        /// <seealso cref="ISvcComponents.GetRapidReviewArtifactsProperties(IUser, List{int}, List{HttpStatusCode})"/>
        public RapidReviewProperties GetRapidReviewArtifactsProperties(
            IUser user,
            List<int> artifactIds,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            string tokenValue = user?.Token?.AccessControlToken;
            string path = RestPaths.Svc.Components.RapidReview.Artifacts.PROPERTIES;

            var restApi = new RestApiFacade(Address, tokenValue);

            var returnedArtifactProperties = restApi.SendRequestAndDeserializeObject<List<RapidReviewProperties>, List<int>>(
                path,
                RestRequestMethod.POST,
                artifactIds,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return returnedArtifactProperties[0];
        }

        #endregion RapidReview methods

        #region  Storyteller methods

        /// <seealso cref="ISvcComponents.GetArtifactInfo(int, IUser, List{HttpStatusCode})"/>
        public ArtifactInfo GetArtifactInfo(
            int artifactId,
            IUser user = null,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetArtifactInfo));

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.ARTIFACT_INFO_id_, artifactId);

            var returnedArtifactInfo = restApi.SendRequestAndDeserializeObject<ArtifactInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes);

            return returnedArtifactInfo;
        }

        /// <seealso cref="ISvcComponents.GetProcess(int, IUser, int?, List{HttpStatusCode})"/>
        public IProcess GetProcess(
            int artifactId,
            IUser user = null,
            int? versionIndex = null, 
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetProcess));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.PROCESSES_id_, artifactId);

            var queryParameters = new Dictionary<string, string>();

            if (versionIndex.HasValue)
            {
                queryParameters.Add("versionId", versionIndex.ToString());
            }

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Getting the Process with artifact ID: {1}", nameof(SvcComponents), artifactId);

            var response = restApi.SendRequestAndDeserializeObject<Process>(
                path,
                RestRequestMethod.GET,
                queryParameters: queryParameters,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return response;
        }

        /// <seealso cref="ISvcComponents.GetProcesses(int, IUser, List{HttpStatusCode})"/>
        public IList<IProcess> GetProcesses(int projectId, IUser user = null, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetProcesses));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.Projects_id_.PROCESSES, projectId);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Getting all Processes for project ID: {1}", nameof(SvcComponents), projectId);

            var response = restApi.SendRequestAndDeserializeObject<List<Process>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return response.ConvertAll(o => (IProcess)o);
        }

        /// <seealso cref="ISvcComponents.GetUserStoryArtifactType(int, IUser, List{HttpStatusCode})"/>
        public OpenApiArtifactType GetUserStoryArtifactType(int projectId, IUser user, List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetUserStoryArtifactType));

            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.Projects_id_.ArtifactTypes.USER_STORY, projectId);
            var restApi = new RestApiFacade(Address, user.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Getting the User Story Artifact Type for project ID: {1}", nameof(SvcComponents), projectId);

            return restApi.SendRequestAndDeserializeObject<OpenApiArtifactType>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);
        }

        /// <seealso cref="ISvcComponents.UpdateProcess(IProcess, IUser, List{HttpStatusCode})"/>
        public ProcessUpdateResult UpdateProcess(
            IProcess process,
            IUser user = null, 
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(UpdateProcess));

            ThrowIf.ArgumentNull(process, nameof(process));

            string path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.PROCESSES_id_, process.Id);
            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);

            Logger.WriteInfo("{0} Updating Process ID: {1}, Name: {2}", nameof(SvcComponents), process.Id, process.Name);

            var processBodyObject = (Process)process;

            return restApi.SendRequestAndDeserializeObject<ProcessUpdateResult, Process>(
                path,
                RestRequestMethod.PATCH,
                jsonObject: processBodyObject,
                expectedStatusCodes: expectedStatusCodes);
        }

        #endregion Storyteller methods

        #endregion Members inherited from ISvcComponents
    }
}
