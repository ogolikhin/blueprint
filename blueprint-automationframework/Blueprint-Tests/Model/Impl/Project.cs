﻿using System;
using System.Runtime.Serialization;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using Common;
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

        /// <summary>
        /// Gets a list of all projects on the Blueprint server.
        /// </summary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="user">(optional) The user to authenticate to the server with.  Defaults to no authentication.</param>
        /// <returns>A list of all projects on the Blueprint server.</returns>
        public List<IProject> GetProjects(string address, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Token?.OpenApiToken);
            const string path = RestPaths.OpenApi.PROJECTS;

            List<Project> projects = restApi.SendRequestAndDeserializeObject<List<Project>>(path, RestRequestMethod.GET);

            // VS Can't automatically convert List<Project> to List<IProject>, so we need to do it manually.
            return projects.ConvertAll(o => (IProject)o);
        }

        /// <sumary>
        /// Get a project based on the project ID on the Blueprint server.
        /// </sumary>
        /// <param name="address">The base Uri address of the Blueprint server.</param>
        /// <param name="projectId">The ID of the project need to be retrieved.</param>
        /// <param name="user">(optional) The user to authenticate to the server with. Default to use no authentication. </param>
        /// <returns>a project associated with the projectId provided with the request.</returns>
        public IProject GetProject(string address, int projectId, IUser user = null)
        {
            RestApiFacade restApi = new RestApiFacade(address, user?.Token.OpenApiToken);
            string path = I18NHelper.FormatInvariant(RestPaths.OpenApi.PROJECT, projectId);
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

            var path = I18NHelper.FormatInvariant(RestPaths.OpenApi.Projects.MetaData.ARTIFACT_TYPES, Id);
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

        #endregion Public Methods

    }
}
