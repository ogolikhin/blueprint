using Common;
using Model.JobModel.Enums;
using System;
using System.Collections.Generic;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    public class AlmTarget : IAlmTarget
    {
        #region properties

        public int Id { get; set; }
        public string Name { get; set; }
        public int BlueprintProjectId { get; set; }
        public AlmType AlmType { get; set; }
        public Uri Url { get; set; }
        public string Domain { get; set; }
        public string Project { get; set; }

        #endregion properties

        #region static methods

        /// <summary>
        /// Get available ALM targets available from the project using the specified user.
        /// </summary>
        /// <param name="address">The address of the Blueprint server.</param>
        /// <param name="user">The user to authenticate with.</param>
        /// <param name="project">The project where the ALM targets exist.</param>
        /// <param name="expectedStatusCodes">(optional) The expected HTTP status codes for this call.  By default only 200 OK is expected.</param>
        /// <returns>The ALM targets available for the project authenticated to the specified user.</returns>
        public static List<IAlmTarget> GetAlmTargets(string address,
            IUser user,
            IProject project,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            var restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ALM.TARGETS, project.Id);

            var almTargets = restApi.SendRequestAndDeserializeObject<List<AlmTarget>>(
                path, 
                RestRequestMethod.GET, 
                expectedStatusCodes: expectedStatusCodes,
                shouldControlJsonChanges: false);

            return almTargets.ConvertAll(o => (IAlmTarget)o);
        }

        #endregion static methods
    }
}
