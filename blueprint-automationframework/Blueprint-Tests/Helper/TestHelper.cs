﻿using System;
using System.Collections.Generic;
using System.Linq;
using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.StorytellerModel;
using NUnit.Framework;
using Utilities;

namespace Helper
{
    public class TestHelper : IDisposable, IArtifactObserver
    {
        private bool _isDisposed = false;

        // Nova services:
        public IAccessControl AccessControl { get; } = AccessControlFactory.GetAccessControlFromTestConfig();
        public IAdminStore AdminStore { get; } = AdminStoreFactory.GetAdminStoreFromTestConfig();
        public IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
        public IBlueprintServer BlueprintServer { get; } = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        public IConfigControl ConfigControl { get; } = ConfigControlFactory.GetConfigControlFromTestConfig();
        public IFileStore FileStore { get; } = FileStoreFactory.GetFileStoreFromTestConfig();
        public IStoryteller Storyteller { get; } = StorytellerFactory.GetStorytellerFromTestConfig();

        // Lists of objects created by this class to be disposed:
        public List<IArtifactBase> Artifacts { get; } = new List<IArtifactBase>();
        public List<IProject> Projects { get; } = new List<IProject>();
        public List<IUser> Users { get; } = new List<IUser>();

        #region IArtifactObserver methods

        /// <seealso cref="IArtifactObserver.NotifyArtifactDeletion(IEnumerable{int})" />
        public void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds)
        {
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));
            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactDeletion), String.Join(", ", deletedArtifactIds));

            foreach (var deletedArtifactId in deletedArtifactIds)
            {
                Artifacts.ForEach(a =>
                {
                    if (a.Id == deletedArtifactId)
                    {
                        a.IsDeleted = true;
                        a.IsPublished = false;
                        a.IsSaved = false;
                    }
                });
                Artifacts.RemoveAll(a => a.Id == deletedArtifactId);
            }
        }

        #endregion IArtifactObserver methods

        #region Artifact Management

        /// <summary>
        /// Create an Open API artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public IOpenApiArtifact CreateOpenApiArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            IOpenApiArtifact artifact = ArtifactFactory.CreateOpenApiArtifact(address, user, project, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create an Open API artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="user">user for authentication</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object</returns>
        public IOpenApiArtifact CreateOpenApiArtifact(IProject project, IUser user, BaseArtifactType artifactType)
        {
            IOpenApiArtifact artifact = ArtifactFactory.CreateOpenApiArtifact(project, user, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create an artifact object and populate required attribute values with ArtifactTypeId, ArtifactTypeName, and ProjectId based the target project
        /// </summary>
        /// <param name="address">address for Blueprint application server</param>
        /// <param name="user">user for authentication</param>
        /// <param name="project">The target project</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object for the target project with selected artifactType</returns>
        public IArtifact CreateArtifact(string address, IUser user, IProject project, BaseArtifactType artifactType)
        {
            IArtifact artifact = ArtifactFactory.CreateArtifact(address, user, project, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file
        /// </summary>
        /// <param name="project">The target project</param>
        /// <param name="user">user for authentication</param>
        /// <param name="artifactType">artifactType</param>
        /// <returns>new artifact object</returns>
        public IArtifact CreateArtifact(IProject project, IUser user, BaseArtifactType artifactType)
        {
            IArtifact artifact = ArtifactFactory.CreateArtifact(project, user, artifactType);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Creates a new artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created in.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="numberOfVersions">(optional) The number of times to save and publish the artifact (to create multiple historical versions).</param>
        /// <returns>The artifact.</returns>
        public IArtifact CreateAndPublishArtifact(IProject project, IUser user, BaseArtifactType artifactType, int numberOfVersions = 1)
        {
            IArtifact artifact = CreateArtifact(project, user, artifactType);

            for (int i = 0; i < numberOfVersions; ++i)
            {
                artifact.Save();
                artifact.Publish();
            }

            return artifact;
        }

        #endregion Artifact Management

        #region Project Management

        /// <summary>
        /// Creates a new project object with the values specified, or with random values for any unspecified parameters.
        /// </summary>
        /// <param name="name">The name of the project.</param>
        /// <param name="description">(optional) The description of the project.</param>
        /// <param name="location">(optional) The location of the project.</param>
        /// <param name="id">(optional) Internal database identifier.  Only set this if you read the project from the database.</param>
        /// <returns>The new project object.</returns>
        public IProject CreateProject(string name = null, string description = null, string location = null, int id = 0)
        {
            IProject project = ProjectFactory.CreateProject(name, description, location, id);
            Projects.Add(project);
            return project;
        }

        #endregion Project Management

        #region User management

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new unique user object that was added to the database.</returns>
        public IUser CreateUserAndAddToDatabase(InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            IUser user = UserFactory.CreateUserAndAddToDatabase(instanceAdminRole, source);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username & password specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public IUser CreateUserAndAddToDatabase(string username, string password,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            IUser user = UserFactory.CreateUserAndAddToDatabase(username, password, instanceAdminRole, source);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Used to specify which type of session tokens to get for the user.
        /// </summary>
        [Flags]
        public enum AuthenticationTokenTypes
        {
            None = 0,
            AccessControlToken = 1,
            OpenApiToken = 2,
            BothAccessControlAndOpenApiTokens = 3
        }

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database,
        /// then authenticates to AdminStore and/or OpenApi to get session tokens.
        /// </summary>
        /// <param name="targets">The authentication targets.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user that has the requested access tokens.</returns>
        public IUser CreateUserAndAuthenticate(AuthenticationTokenTypes targets,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            IUser user = CreateUserAndAddToDatabase(instanceAdminRole, source);

            if ((targets & AuthenticationTokenTypes.AccessControlToken) != 0)
            {
                AdminStore.AddSession(user);
                Assert.NotNull(user.Token?.AccessControlToken, "User '{0}' didn't get an AccessControl token!", user.Username);
            }

            if ((targets & AuthenticationTokenTypes.OpenApiToken) != 0)
            {
                BlueprintServer.LoginUsingBasicAuthorization(user);
                Assert.NotNull(user.Token?.OpenApiToken, "User '{0}' didn't get an OpenAPI token!", user.Username);
            }

            return user;
        }

        #endregion User management

        #region Custom Asserts

        /// <summary>
        /// Asserts that both artifacts are identical.
        /// </summary>
        /// <param name="firstArtifact">The first artifact to compare.</param>
        /// <param name="secondArtifact">The second artifact to compare.</param>
        /// <param name="compareBlueprintUrls">(optional) Pass true if you want to also compare the BlueprintUrl properties of the artifacts.</param>
        /// <param name="compareVersions">(optional) Pass true if you want to also compare the Version properties of the artifacts.</param>
        public static void AssertArtifactsAreEqual(IArtifactBase firstArtifact,
            IArtifactBase secondArtifact,
            bool compareBlueprintUrls = false,
            bool compareVersions = false)
        {
            ThrowIf.ArgumentNull(firstArtifact, nameof(firstArtifact));
            ThrowIf.ArgumentNull(secondArtifact, nameof(secondArtifact));

            Assert.AreEqual(firstArtifact.AreAttachmentsReadOnly, secondArtifact.AreAttachmentsReadOnly,
                "First artifact AreAttachmentsReadOnly: '{0}' doesn't match second artifact AreAttachmentsReadOnly: '{1}'",
                firstArtifact.AreAttachmentsReadOnly, secondArtifact.AreAttachmentsReadOnly);
            Assert.AreEqual(firstArtifact.AreDocumentReferencesReadOnly, secondArtifact.AreDocumentReferencesReadOnly,
                "First artifact AreDocumentReferencesReadOnly: '{0}' doesn't match second artifact AreDocumentReferencesReadOnly: '{1}'",
                firstArtifact.AreDocumentReferencesReadOnly, secondArtifact.AreDocumentReferencesReadOnly);
            Assert.AreEqual(firstArtifact.AreTracesReadOnly, secondArtifact.AreTracesReadOnly,
                "First artifact AreTracesReadOnly: '{0}' doesn't match second artifact AreTracesReadOnly: '{1}'",
                firstArtifact.AreTracesReadOnly, secondArtifact.AreTracesReadOnly);
            Assert.AreEqual(firstArtifact.ArtifactTypeId, secondArtifact.ArtifactTypeId,
                "First artifact ArtifactTypeId: '{0}' doesn't match second artifact ArtifactTypeId: '{1}'",
                firstArtifact.ArtifactTypeId, secondArtifact.ArtifactTypeId);
            Assert.AreEqual(firstArtifact.ArtifactTypeName, secondArtifact.ArtifactTypeName,
                "First artifact ArtifactTypeName: '{0}' doesn't match second artifact ArtifactTypeName: '{1}'",
                firstArtifact.ArtifactTypeName, secondArtifact.ArtifactTypeName);
            Assert.AreEqual(firstArtifact.BaseArtifactType, secondArtifact.BaseArtifactType,
                "First artifact BaseArtifactType: '{0}' doesn't match second artifact BaseArtifactType: '{1}'",
                firstArtifact.BaseArtifactType, secondArtifact.BaseArtifactType);

            if (compareBlueprintUrls)
            {
                Assert.AreEqual(firstArtifact.BlueprintUrl, secondArtifact.BlueprintUrl,
                    "First artifact BlueprintUrl: '{0}' doesn't match second artifact BlueprintUrl: '{1}'",
                    firstArtifact.BlueprintUrl, secondArtifact.BlueprintUrl);
            }

            Assert.AreEqual(firstArtifact.Id, secondArtifact.Id,
                "First artifact ID: '{0}' doesn't match second artifact ID: '{1}'",
                firstArtifact.Id, secondArtifact.Id);
            Assert.AreEqual(firstArtifact.Name, secondArtifact.Name,
                "First artifact Name: '{0}' doesn't match second artifact Name: '{1}'",
                firstArtifact.Name, secondArtifact.Name);
            Assert.AreEqual(firstArtifact.ProjectId, secondArtifact.ProjectId,
                "First artifact ProjectId: '{0}' doesn't match second artifact ProjectId: '{1}'",
                firstArtifact.ProjectId, secondArtifact.ProjectId);

            if (compareVersions)
            {
                Assert.AreEqual(firstArtifact.Version, secondArtifact.Version,
                    "First artifact Version: '{0}' doesn't match second artifact Version: '{1}'",
                    firstArtifact.Version, secondArtifact.Version);
            }
        }

        #endregion Custom Asserts

        #region Members inherited from IDisposable

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly called, or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(TestHelper.Dispose));

            if (_isDisposed)
            {
                return;
            }

            if (disposing)
            {
                Storyteller?.Dispose();
                FileStore?.Dispose();
                ConfigControl?.Dispose();
                BlueprintServer?.Dispose();
                ArtifactStore?.Dispose();
                AdminStore?.Dispose();
                AccessControl?.Dispose();

                var savedArtifactsDictionary = new Dictionary<IUser, List<IArtifactBase>>();

                // Separate the published from the unpublished artifacts.  Delete the published ones, and discard the saved ones.
                foreach (var artifact in Artifacts.ToArray())
                {
                    if (artifact.IsPublished)
                    {
                        if (artifact.IsMarkedForDeletion)
                        {
                            artifact.Publish();
                        }
                        else
                        {
                            artifact.Delete();
                            artifact.Publish();
                        }
                    }
                    else if (artifact.IsSaved)
                    {
                        if (savedArtifactsDictionary.ContainsKey(artifact.CreatedBy))
                        {
                            savedArtifactsDictionary[artifact.CreatedBy].Add(artifact);
                        }
                        else
                        {
                            savedArtifactsDictionary.Add(artifact.CreatedBy, new List<IArtifactBase> { artifact });
                        }
                    }

                    artifact.UnregisterObserver(this);
                }

                // For each user that created artifacts, discard the list of artifacts they created.
                foreach (IUser user in savedArtifactsDictionary.Keys)
                {
                    Logger.WriteDebug("*** Discarding all unpublished artifacts created by user: '{0}'.", user.Username);
                    Artifact.DiscardArtifacts(savedArtifactsDictionary[user], savedArtifactsDictionary[user].First().Address, user);
                }

                foreach (var project in Projects)
                {
                    project.DeleteProject();
                }

                foreach (var user in Users)
                {
                    user.DeleteUser();
                }
            }

            _isDisposed = true;

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(TestHelper.Dispose));
        }

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion Members inherited from IDisposable
    }
}
