using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using Utilities;
using Utilities.Facades;

namespace Model.NavigationModel.Impl
{
    public class Navigation : INavigation
    {
        #region Constants

        private const string SVC_PATH = "svc/shared/navigation";
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #endregion Constants

        #region Properties

        public string Address { get; }

        public List<IArtifact> Artifacts { get; } = new List<IArtifact>();

        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<ArtifactReference>>))]
        public List<ArtifactReference> ArtifactReferenceList { get; set; }

        #endregion Properties

        #region Constructors

        public Navigation(string address)
        {
            ThrowIf.ArgumentNull(address, nameof(address));

            Address = address;
        }

        #endregion Constructors

        #region Methods

        public List<IArtifactReference> GetNavigation(IUser user, List<IArtifact> artifacts, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));
            ThrowIf.ArgumentNull(artifacts, nameof(artifacts));

            string tokenValue = user.Token?.AccessControlToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = string.Empty;
            }

            //Get list of artifacts which were created.
            List<int> artifactIds = artifacts.Select(artifact => artifact.Id).ToList();

            string path = SVC_PATH;

            foreach (var id in artifactIds)
            {
                path = I18NHelper.FormatInvariant("{0}/{1}", path, id);
            }

            var restApi = new RestApiFacade(Address, user.Username, user.Password, tokenValue);

            var response = restApi.SendRequestAndDeserializeObject<List<ArtifactReference>>(
                path,
                RestRequestMethod.GET,
                expectedStatusCodes: expectedStatusCodes,
                cookies: cookies);

            return response.ConvertAll(o => (IArtifactReference)o) ;
        }

        public List<DeleteArtifactResult> DeleteNavigationArtifact(IArtifact artifact, List<HttpStatusCode> expectedStatusCodes = null, bool sendAuthorizationAsCookie = false, bool deleteChildren = false)
        {
            ThrowIf.ArgumentNull(artifact, nameof(artifact));

            Artifacts.Remove(Artifacts.First(i => i.Id.Equals(artifact.Id)));
            return artifact.Delete(artifact.CreatedBy, expectedStatusCodes, sendAuthorizationAsCookie: sendAuthorizationAsCookie, deleteChildren: deleteChildren);
        }

        #endregion Methods
    }

    public class ArtifactReference : IArtifactReference
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; }
        public string TypePrefix { get; set; }
        public ItemTypePredefined BaseItemTypePredefined { get; set; }
        public string Link { get; set; }
    }
}
