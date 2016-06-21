using Common;
using Model.Impl;
using System.Data;
using System.Net;
using Utilities.Factories;
using System.Linq;
using System.Collections.Generic;
using Utilities;
using Utilities.Facades;
using TestConfig;

namespace Model.Factories
{
    public static class ProjectFactory
    {
        private static string _address = getOpenApiUrl();

        private static string getOpenApiUrl()
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return testConfig.BlueprintServerAddress;
        }

        private const string SVC_PROJECTS_PATH = "/api/v1/projects";

        /// <summary>
        /// Creates a new project object with the values specified, or with random values for any unspecified parameters.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="description">(optional) The description of the project.</param>
        /// <param name="location">(optional) The location of the project.</param>
        /// <param name="id">(optional) Internal database identifier.  Only set this if you read the project from the database.</param>
        /// <returns>The new project object.</returns>
        public static IProject CreateProject(string name = null, string description = null, string location = null, int id = 0)
        {
            if (name == null) { name = RandomGenerator.RandomAlphaNumeric(10); }
            if (description == null) { description = RandomGenerator.RandomAlphaNumeric(10); }
            if (location == null) { location = RandomGenerator.RandomAlphaNumeric(10); }

            IProject project = new Project { Name = name, Description = description, Location = location, Id = id };
            return project;
        }

        /// <summary>
        /// Get the project object with the name specified, or the first project from BP database.
        /// </summary>
        /// <param name="user">The user making the REST request.</param>
        /// <param name="projectName">(optional) The name of the project.</param>
        /// <param name="shouldRetrieveArtifactTypes">(optional) Define if ArtifactType list needs to be retrieved.
        ///  By default, set to true</param>
        /// <param name="shouldRetrievePropertyTypes">(optional) Define if Property values also need to be retrieved
        ///  as part of ArtifactType list. By default, set to false</param>
        /// <returns>The first valid project object that retrieved from Blueprint server or valid project object with the project name specified </returns>
        public static IProject GetProject(IUser user, string projectName = null, bool shouldRetrieveArtifactTypes = true, bool shouldRetrievePropertyTypes = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            string path = I18NHelper.FormatInvariant("{0}", SVC_PROJECTS_PATH);

            RestApiFacade restApi = new RestApiFacade(_address, user.Token?.OpenApiToken);
            List<HttpStatusCode> expectedStatusCodes = new List<HttpStatusCode>() { HttpStatusCode.OK, HttpStatusCode.PartialContent };
            List<Project> projects = restApi.SendRequestAndDeserializeObject<List<Project>>(path, RestRequestMethod.GET, expectedStatusCodes: expectedStatusCodes);

            if (projects.Count == 0)
            {
                Logger.WriteError("No project available on the test server {0}", _address);
                throw new DataException("No project available on the test server");
            }

            // Get project from blueprint
            Project prj = projectName == null ? projects.First() : projects.First(t => (t.Name == projectName));

            // Create a project object in memeory using the constructor
            IProject project = new Project { Name = prj.Name, Description = prj.Description, Id = prj.Id};

            if (shouldRetrieveArtifactTypes)
            {
                project.GetAllArtifactTypes(address: _address, user: user,
                    shouldRetrievePropertyTypes: shouldRetrievePropertyTypes);
            }

            return project;
        }
    }
}

