using Common;
using Model.Impl;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Model.OpenApiModel.Services;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ProjectFactory
    {
        public static string Address { get; } = GetOpenApiUrl();

        private static string GetOpenApiUrl()
        {
            var testConfig = TestConfiguration.GetInstance();
            return testConfig.BlueprintServerAddress;
        }

        public static IArtifactStore ArtifactStore { get; } = GetArtifactStore();

        private static IArtifactStore GetArtifactStore()
        {
            return ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
        }



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

            var project = new Project { Name = name, Description = description, Location = location, Id = id };
            return project;
        }

        /// <summary>
        /// Gets all projects on the Blueprint server.
        /// </summary>
        /// <param name="user">The user making the REST request.</param>
        /// <param name="shouldRetrievePropertyTypes">(optional) Pass true if you also want to get the property types for each project.</param>
        /// <param name="shouldRetrieveNovaArtifactTypes">(optional) Pass true if you also want to get the Nova artifact types for each project.</param>
        /// <returns>A list of projects that were found.</returns>
        public static List<IProject> GetAllProjects(IUser user, bool shouldRetrieveArtifactTypes = false, 
            bool shouldRetriveNovaArtifactTypes = false)
        {
            ThrowIf.ArgumentNull(user, nameof(user));

            var projects = OpenApi.GetProjects(Address, user);

            foreach (var project in projects)
            {
                project.ArtifactStore = ArtifactStore;

                if (shouldRetrieveArtifactTypes)
                {
                    project.GetAllOpenApiArtifactTypes(Address, user);
                }

                if (shouldRetriveNovaArtifactTypes)
                {
                    project.NovaArtifactTypes.AddRange(project.GetAllNovaArtifactTypes(ArtifactStore, user));
                }
            }

            return projects;
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
            var projects = GetAllProjects(user);

            if (projects.Count == 0)
            {
                Logger.WriteError("No project available on the test server {0}", Address);
                throw new DataException("No project available on the test server");
            }

            // Get project from blueprint
            var prj = projectName == null ? projects.First() : projects.First(t => (t.Name == projectName));

            // Create a project object in memeory using the constructor
            var project = new Project { Name = prj.Name, Description = prj.Description, Id = prj.Id, ArtifactStore = ArtifactStore};  // TODO: Do we need to make a copy of it?

            if (shouldRetrieveArtifactTypes)
            {
                project.GetAllOpenApiArtifactTypes(address: Address, user: user,
                    shouldRetrievePropertyTypes: shouldRetrievePropertyTypes);
            }

            return project;
        }

        /// <summary>
        /// Gets the first x number of projects from BP database.
        /// </summary>
        /// <param name="user">The user making the REST request.</param>
        /// <param name="numberOfProjects">The number of projects to get.</param>
        /// <param name="shouldRetrieveArtifactTypes">(optional) Define if ArtifactType list needs to be retrieved.
        ///     By default, set to true</param>
        /// <param name="shouldRetrievePropertyTypes">(optional) Define if Property values also need to be retrieved
        ///     as part of ArtifactType list.  By default, set to false</param>
        /// <param name="shouldRetrieveNovaArtifactTypes">(optional) Define if NovaArtifactType list needs to be retrieved.
        ///     By default, set to true</param>
        /// <returns>The first x number of projects retrieved from Blueprint server.</returns>
        /// <exception cref="DataException">If not enough projects exist on the server.</exception>
        public static List<IProject> GetProjects(
            IUser user,
            int numberOfProjects,
            bool shouldRetrieveArtifactTypes = true,
            bool shouldRetrievePropertyTypes = false,
            bool shouldRetrieveNovaArtifactTypes = true)
        {
            var allProjects = GetAllProjects(user);

            if (allProjects.Count < numberOfProjects)
            {
                string errorMsg = I18NHelper.FormatInvariant("Not enough projects available on the test server '{0}'.  Need {1}, but only {2} exist.",
                    Address, numberOfProjects, allProjects.Count);

                Logger.WriteError(errorMsg);
                throw new DataException(errorMsg);
            }

            // Get project(s) from blueprint.
            var projects = allProjects.GetRange(0, numberOfProjects);

            if (shouldRetrieveArtifactTypes)
            {
                foreach (var project in projects)
                {
                    project.GetAllOpenApiArtifactTypes(address: Address, user: user,
                        shouldRetrievePropertyTypes: shouldRetrievePropertyTypes);
                }
            }

            if (shouldRetrieveNovaArtifactTypes)
            {
                var artifactStore = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();

                foreach (var project in projects)
                {
                    project.GetAllNovaArtifactTypes(artifactStore, user);
                }
            }

            return projects;
        }
    }
}

