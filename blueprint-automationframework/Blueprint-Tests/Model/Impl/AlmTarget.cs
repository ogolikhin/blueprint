using Common;
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
        public string AlmType { get; set; }
        public Uri Url { get; set; }
        public string Domain { get; set; }
        public string Project { get; set; }
        
        #endregion projerties

        #region static methods

        public static List<IAlmTarget> GetAlmTargets (string address,
            IUser user,
            IProject project,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(project, nameof(project));

            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.ALMTARGETS, project.Id);

            List<AlmTarget> almTargets = restApi.SendRequestAndDeserializeObject<List<AlmTarget>>(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            return almTargets.ConvertAll(o => (IAlmTarget)o);
        }

        #endregion static methods
    }
}
