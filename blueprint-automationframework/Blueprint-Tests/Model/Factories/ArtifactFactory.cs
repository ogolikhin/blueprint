using Model.ArtifactModel;
using Model.ArtifactModel.Enums;
using Model.ArtifactModel.Impl;
using Newtonsoft.Json;
using NUnit.Framework;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ArtifactFactory
    {
        /// <summary>
        /// Create an Open API artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <param name="parent">(optional) The parent artifact.  By default artifact will be created in root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public static IOpenApiArtifact CreateOpenApiArtifact(string address,
            IUser user,
            IProject project,
            BaseArtifactType artifactType,
            IArtifactBase parent = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            var artifact = new OpenApiArtifact(address);
            artifact.BaseArtifactType = artifactType;

            artifact.ProjectId = project.Id;
            artifact.ParentId = parent?.Id ?? project.Id;

            var projectArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.Equals(artifactType));
            artifact.ArtifactTypeId = projectArtifactType.Id;
            artifact.ArtifactTypeName = projectArtifactType.Name;
            artifact.Name = name ?? "OpenApi_Artifact_" + artifact.ArtifactTypeName + "_" + RandomGenerator.RandomAlphaNumeric(5);

            //TODO: Move this to Save method and get CreatedBy from the result of the OpenAPI call
            artifact.CreatedBy = user;

            return artifact;
        }

        /// <summary>
        /// Create an Open API artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="user">user for authentication</param>
        /// <param name="artifactType">artifactType</param>
        /// <param name="artifactId">(optional) You can specify a custom artifact ID here (for testing non-existent artifacts for example).</param>
        /// <param name="parent">(optional) The parent artifact.  By default artifact will be created in root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>new artifact object</returns>
        public static IOpenApiArtifact CreateOpenApiArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int? artifactId = null,
            IArtifactBase parent = null,
            string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));

            var testConfig = TestConfiguration.GetInstance();
            var artifact = CreateOpenApiArtifact(testConfig.BlueprintServerAddress, user, project, artifactType, parent, name);

            if (artifactId != null)
            {
                artifact.Id = artifactId.Value;
            }

            return artifact;
        }

        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project.
        /// </summary>
        /// <param name="address">Address for Blueprint application server.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="project">The target project.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>New artifact object for the target project with selected artifactType.</returns>
        public static IArtifact CreateArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType,
            IArtifactBase parent = null, string name = null)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            var artifact = new Artifact(address);
            artifact.BaseArtifactType = artifactType;

            artifact.Project = project;
            artifact.ProjectId = project.Id;
            artifact.ParentId = parent?.Id ?? project.Id;
            
            var projectArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.Equals(artifactType));
            Assert.NotNull(projectArtifactType, "No custom artifact type was found in project '{0}' for BaseArtifactType: {1}!",
                project.Name, artifactType);
            artifact.ArtifactTypeId = projectArtifactType.Id;
            artifact.ArtifactTypeName = projectArtifactType.Name;
            artifact.Name = name ?? "Artifact_" + artifact.ArtifactTypeName + "_" + RandomGenerator.RandomAlphaNumeric(5);

            //TODO: Move this to Save method and get CreatedBy from the result of the OpenAPI call
            artifact.CreatedBy = user;

            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="artifactId">(optional) You can specify a custom artifact ID here (for testing non-existent artifacts for example).</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <param name="name">(optional) Artifact's name.</param>
        /// <returns>New artifact object.</returns>
        public static IArtifact CreateArtifact(IProject project, IUser user, BaseArtifactType artifactType,
            int? artifactId = null, IArtifactBase parent = null, string name = null)
        {
            var testConfig = TestConfiguration.GetInstance();
            var artifact = CreateArtifact(testConfig.BlueprintServerAddress, user, project, artifactType, parent, name);

            if (artifactId != null)
            {
                artifact.Id = artifactId.Value;
            }

            return artifact;
        }

        /// <summary>
        /// Tries to convert NovaArtifactDetails to specific artifact type (now it works for Actor).
        /// </summary>
        /// <param name="artifactDetails">artifactDetails object.</param>
        /// <param name="artifactDetailsJSON">JSON representation of artifactDetails.</param>
        /// <returns>Object of specific artifact type.</returns>
        public static NovaArtifactDetails ConvertToSpecificArtifact(NovaArtifactDetails artifactDetails, string artifactDetailsJSON)
        {
            ThrowIf.ArgumentNull(artifactDetails, nameof(artifactDetails));
            switch (artifactDetails.PredefinedType.Value)
            {
                case (int)ItemTypePredefined.Actor:
                    var actorDetails = JsonConvert.DeserializeObject<Actor>(artifactDetailsJSON);
                    return actorDetails;
                case (int)ItemTypePredefined.ArtifactReviewPackage:
                    var reviewDetails = JsonConvert.DeserializeObject<Review>(artifactDetailsJSON);
                    return reviewDetails;
                default:
                    return artifactDetails;
            }
        }
    }
}
