using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using Common;
using Model.ArtifactModel.Impl;
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

        #endregion RapidReview methods

        #region  Storyteller methods

        /// <seealso cref="ISvcComponents.GetArtifactInfo(int, IUser, List{HttpStatusCode})"/>
        public ArtifactInfo GetArtifactInfo(int artifactId, IUser user = null, 
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            Logger.WriteTrace("{0}.{1}", nameof(SvcComponents), nameof(GetArtifactInfo));

            var restApi = new RestApiFacade(Address, user?.Token?.AccessControlToken);
            var path = I18NHelper.FormatInvariant(RestPaths.Svc.Components.Storyteller.ARTIFACT_INFO_id_, artifactId);

            var returnedArtifactInfo = restApi.SendRequestAndDeserializeObject<ArtifactInfo>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return returnedArtifactInfo;
        }

        #endregion Storyteller methods

        #endregion Members inherited from ISvcComponents
    }
}
