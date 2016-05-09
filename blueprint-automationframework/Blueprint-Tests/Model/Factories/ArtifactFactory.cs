﻿using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using System.Collections.Generic;
using TestConfig;
using Utilities;
using Utilities.Factories;

namespace Model.Factories
{
    public static class ArtifactFactory
    {
        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public static IOpenApiArtifact CreateOpenApiArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            IOpenApiArtifact artifact = new OpenApiArtifact(address);
            artifact.BaseArtifactType = artifactType;

            artifact.ProjectId = project.Id;
            artifact.ParentId = project.Id;

            var projectArtifactType = project.ArtifactTypes.Find(at => at.BaseArtifactType.Equals(artifactType));
            artifact.ArtifactTypeId = projectArtifactType.Id;
            artifact.ArtifactTypeName = projectArtifactType.Name;
            artifact.Name = "OpenApi_Artifact_" + artifact.ArtifactTypeName + "_" + RandomGenerator.RandomAlphaNumeric(5);

            //TODO: Move this to Save method and get CreatedBy from the result of the OpenAPI call
            artifact.CreatedBy = user;

            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <returns>new artifact object</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]   // Ignore this warning.
        public static IOpenApiArtifact CreateOpenApiArtifact(IProject project, IUser user, BaseArtifactType artifactType)
        {
            TestConfiguration testConfig = TestConfiguration.GetInstance();
            return CreateOpenApiArtifact(testConfig.BlueprintServerAddress, user, project, artifactType);
        }

        /// <summary>
        /// Create and Save Multiple Artifacts
        /// </summary>
        /// <param name="project">The project where the artifacts are to be added</param>
        /// <param name="user">The user credentials for the request to create the artifacts</param>
        /// <param name="artifactType">Artifact Type</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create</param>
        /// <returns>The list of the artifact objects</returns>
        public static List<IOpenApiArtifact> CreateAndSaveOpenApiArtifacts(IProject project, IUser user, BaseArtifactType artifactType, int numberOfArtifacts)
        {
            var artifacts = new List<IOpenApiArtifact>();

            for (int i = 0; i < numberOfArtifacts; i++)
            {
                var artifact = CreateOpenApiArtifact(project, user, artifactType);
                artifact.Save();
                artifacts.Add(artifact);
            }
            return artifacts;
        }
    }
}