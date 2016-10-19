using System;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Common;
using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Facades;

namespace Model.Impl
{
    [DataContract(Name = "Project", Namespace = "Model")]
    public class Project : IProject
    {
        private const string SessionTokenCookieName = "BLUEPRINT_SESSION_TOKEN";

        #region Properties

        /// <summary>
        /// Id of the project
        /// </summary>
        [JsonProperty("Id")]
        public int Id { get; set; }

        /// <summary>
        /// Name of the project
        /// </summary>
        [JsonProperty("Name")]
        public string Name { get; set; }

        /// <summary>
        /// Description of the project
        /// </summary>
        [JsonProperty("Description")]
        public string Description { get; set; }

        /// <summary>
        /// Full path for the project. e.g. /Blueprint/Project
        /// </summary>
        public string Location { get; set; }

        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<OpenApiArtifactType>>))]
        public List<OpenApiArtifactType> ArtifactTypes { get; } = new List<OpenApiArtifactType>();

        [JsonIgnore]
        public List<NovaArtifactType> NovaArtifactTypes { get; } = new List<NovaArtifactType>();

        [JsonIgnore]
        public List<NovaPropertyType> NovaPropertyTypes { get; } = new List<NovaPropertyType>();

        [JsonIgnore]
        public List<NovaArtifactType> NovaSubArtifactTypes { get; } = new List<NovaArtifactType>();

        #endregion Properties

        #region Public Methods

        /// <summary>
        /// Creates a new project on the Blueprint server.
        /// </summary>
        public void CreateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Deletes a project on the Blueprint server.
        /// </summary>
        public void DeleteProject()
        {
            throw new NotImplementedException();
        }

        /// <seealso cref="IProject.GetDefaultCollectionFolder(string, IUser)"/>
        public INovaArtifact GetDefaultCollectionFolder(string address, IUser user)
        {
            var novaArtifacts = ArtifactStore.GetProjectChildrenByProjectId(address, Id, user);
            return novaArtifacts.Find(a => a.PredefinedType == (int)BaselineAndCollectionTypePredefined.CollectionFolder);
        }

        /// <seealso cref="IProject.GetItemTypeIdForPredefinedType(ItemTypePredefined)"/>
        public int GetItemTypeIdForPredefinedType(ItemTypePredefined predefinedType)
        {
            NovaArtifactType itemType = NovaArtifactTypes.Find(at => at.PredefinedType == predefinedType);
            Assert.NotNull(itemType, "No Nova artifact type was found in project {0} for predefined type: '{1}'", Id, predefinedType);
            return itemType.Id;
        }

        /// <seealso cref="IProject.GetProjects(string, IUser)"/>
        public List<IProject> GetProjects(string address, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string path = RestPaths.OpenApi.PROJECTS;

            List<Project> projects = restApi.SendRequestAndDeserializeObject<List<Project>>(path, RestRequestMethod.GET);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }

        /// <seealso cref="IProject.GetProject(string, int, IUser)"/>
        public IProject GetProject(string address, int projectId, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.PROJECTS_id_, projectId);
            Project project = restApi.SendRequestAndDeserializeObject<Project>(path, RestRequestMethod.GET);

            return project;
        }

        /// <summary>
        /// Updates a project on the Blueprint server with the changes that were made to this object.
        /// </summary>
        public void UpdateProject()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a string representation of this object.
        /// </summary>
        /// <returns>A string representation of this object.</returns>
        public override string ToString()
        {
            return I18NHelper.FormatInvariant("[Project]: Id={0}, Name={1}, Description={2}, Location={3}", Id, Name, Description, Location);
        }

        /// <seealso cref="IProject.GetAllArtifactTypes(string, IUser, bool, List{HttpStatusCode}, bool)"/>
        public List<OpenApiArtifactType> GetAllArtifactTypes(
            string address,
            IUser user,
            bool shouldRetrievePropertyTypes = false,
            List<HttpStatusCode> expectedStatusCodes = null,
            bool sendAuthorizationAsCookie = false
            )
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string tokenValue = user.Token?.OpenApiToken;
            var cookies = new Dictionary<string, string>();

            if (sendAuthorizationAsCookie)
            {
                cookies.Add(SessionTokenCookieName, tokenValue);
                tokenValue = BlueprintToken.NO_TOKEN;
            }

            RestApiFacade restApi = new RestApiFacade(address, tokenValue);

            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects_id_.MetaData.ARTIFACT_TYPES, Id);
            var queryParameters = new Dictionary<string, string>();

            if (shouldRetrievePropertyTypes)
            {
                queryParameters.Add("PropertyTypes", "true");
            }

            // Retrieve the artifact type list for the project 
            var artifactTypes = restApi.SendRequestAndDeserializeObject<List<OpenApiArtifactType>>(path, RestRequestMethod.GET,
                queryParameters: queryParameters, expectedStatusCodes: expectedStatusCodes, cookies: cookies);

            // Clean and repopulate ArtifactTypes if there is any element exist for ArtifactTypes
            if (ArtifactTypes.Any())
            {
                ArtifactTypes.Clear();
            }

            foreach (var artifactType in artifactTypes)
            {
                ArtifactTypes.Add(artifactType);
            }

            return artifactTypes;
        }

        /// <seealso cref="IProject.GetAllNovaArtifactTypes(IArtifactStore, IUser, List{HttpStatusCode})"/>
        public List<NovaArtifactType> GetAllNovaArtifactTypes(
            IArtifactStore artifactStore,
            IUser user,
            List<HttpStatusCode> expectedStatusCodes = null)
        {
            ThrowIf.ArgumentNull(artifactStore, nameof(artifactStore));

            var artifactTypesResult = artifactStore.GetCustomArtifactTypes(this, user, expectedStatusCodes);
            var artifactTypes = artifactTypesResult.ArtifactTypes;

            // Clean and repopulate NovaArtifactTypes if there is any element exist for NovaArtifactTypes.
            if (NovaArtifactTypes.Any())
            {
                NovaArtifactTypes.Clear();
            }

            NovaArtifactTypes.AddRange(artifactTypes);

            if (NovaPropertyTypes.Any())
            {
                NovaPropertyTypes.Clear();
            }

            NovaPropertyTypes.AddRange(artifactTypesResult.PropertyTypes);

            if (NovaSubArtifactTypes.Any())
            {
                NovaSubArtifactTypes.Clear();
            }

            NovaSubArtifactTypes.AddRange(artifactTypesResult.SubArtifactTypes);

            return artifactTypes;
        }

        #endregion Public Methods

    }
}
