﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Common;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using Model.Impl;
using Model.StorytellerModel;
using NUnit.Framework;
using Utilities;
using Model.SearchServiceModel;

namespace Helper
{
    public class TestHelper : IDisposable, IArtifactObserver
    {
        public enum ProjectRole
        {
            None,
            Viewer,
            AuthorFullAccess,
            Author
        }

        private bool _isDisposed = false;

        // Nova services:
        public IAccessControl AccessControl { get; } = AccessControlFactory.GetAccessControlFromTestConfig();
        public IAdminStore AdminStore { get; } = AdminStoreFactory.GetAdminStoreFromTestConfig();
        public IArtifactStore ArtifactStore { get; } = ArtifactStoreFactory.GetArtifactStoreFromTestConfig();
        public IBlueprintServer BlueprintServer { get; } = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        public IConfigControl ConfigControl { get; } = ConfigControlFactory.GetConfigControlFromTestConfig();
        public IFileStore FileStore { get; } = FileStoreFactory.GetFileStoreFromTestConfig();
        public ISearchService SearchService { get; } = SearchServiceFactory.GetSearchServiceFromTestConfig();
        public IStoryteller Storyteller { get; } = StorytellerFactory.GetStorytellerFromTestConfig();
        public ISvcShared SvcShared { get; } = SvcSharedFactory.GetSvcSharedFromTestConfig();

        // Lists of objects created by this class to be disposed:
        public List<IArtifactBase> Artifacts { get; } = new List<IArtifactBase>();
        public List<IProject> Projects { get; } = new List<IProject>();
        public List<IUser> Users { get; } = new List<IUser>();
        public List<IGroup> Groups { get; } = new List<IGroup>();
        public List<IProjectRole> ProjectRoles { get; } = new List<IProjectRole>();

        #region IArtifactObserver methods

        /// <seealso cref="IArtifactObserver.NotifyArtifactDeletion(IEnumerable{int})" />
        public void NotifyArtifactDeletion(IEnumerable<int> deletedArtifactIds)
        {
            ThrowIf.ArgumentNull(deletedArtifactIds, nameof(deletedArtifactIds));
            var artifactIds = deletedArtifactIds as IList<int> ?? deletedArtifactIds.ToList();
            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactDeletion), String.Join(", ", artifactIds));

            foreach (var deletedArtifactId in artifactIds)
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

        /// <seealso cref="IArtifactObserver.NotifyArtifactPublish(IEnumerable{int})" />
        public void NotifyArtifactPublish(IEnumerable<int> publishedArtifactIds)
        {
            ThrowIf.ArgumentNull(publishedArtifactIds, nameof(publishedArtifactIds));
            var artifactIds = publishedArtifactIds as IList<int> ?? publishedArtifactIds.ToList();
            Logger.WriteTrace("*** {0}.{1}({2}) was called.",
                nameof(TestHelper), nameof(TestHelper.NotifyArtifactPublish), String.Join(", ", artifactIds));

            foreach (var publishedArtifactId in artifactIds)
            {
                Artifacts.ForEach(a =>
                {
                    if (a.Id == publishedArtifactId)
                    {
                        if (a.IsMarkedForDeletion)
                        {
                            a.IsDeleted = true;
                            a.IsPublished = false;
                        }
                        else
                        {
                            a.IsPublished = true;
                        }

                        a.IsSaved = false;
                    }
                });
                Artifacts.RemoveAll(a => a.Id == publishedArtifactId);
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
        /// Create and save an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parentArtifact">(optional) The parent artifact.  By default artifact will be created in root of the project.</param>
        /// <returns>The new artifact object.</returns>
        public IOpenApiArtifact CreateAndSaveOpenApiArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            IArtifactBase parentArtifact = null)
        {
            IOpenApiArtifact artifact = ArtifactFactory.CreateOpenApiArtifact(project, user, artifactType, parentArtifact);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            artifact.Save();
            return artifact;
        }

        /// <summary>
        /// Creates a new OpenApi artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="numberOfVersions">(optional) The number of times to save and publish the artifact (to create multiple historical versions).</param>
        /// <returns>The OpenApi artifact.</returns>
        public IOpenApiArtifact CreateAndPublishOpenApiArtifact(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int numberOfVersions = 1)
        {
            IOpenApiArtifact artifact = CreateOpenApiArtifact(project, user, artifactType);

            for (int i = 0; i < numberOfVersions; ++i)
            {
                artifact.Save();
                artifact.Publish();
            }

            return artifact;
        }

        /// <summary>
        /// Creates a list of new published OpenApi artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactType">The type of artifacts to create.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <returns>The list of OpenApi artifacts.</returns>
        public List<IArtifactBase> CreateAndPublishMultipleOpenApiArtifacts(IProject project, IUser user, BaseArtifactType artifactType, int numberOfArtifacts)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                IOpenApiArtifact artifact = CreateAndPublishOpenApiArtifact(project, user, artifactType);
                artifactList.Add(artifact);
            }

            return artifactList;
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
        /// Create an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parent">(optional)The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <returns>The new artifact object.</returns>
        public IArtifact CreateArtifact(IProject project, IUser user, BaseArtifactType artifactType, IArtifactBase parent = null)
        {
            IArtifact artifact = ArtifactFactory.CreateArtifact(project, user, artifactType, parent: parent);
            Artifacts.Add(artifact);
            artifact.RegisterObserver(this);
            return artifact;
        }

        /// <summary>
        /// Create and save an artifact object using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <returns>The new artifact object.</returns>
        public IArtifact CreateAndSaveArtifact(IProject project, IUser user, BaseArtifactType artifactType, IArtifactBase parent = null)
        {
            IArtifact artifact = CreateArtifact(project, user, artifactType, parent);
            artifact.Save();
            return artifact;
        }

        /// <summary>
        /// Create and save multiple artifacts using the Blueprint application server address from the TestConfiguration file.
        /// </summary>
        /// <param name="project">The target project.</param>
        /// <param name="user">User for authentication.</param>
        /// <param name="artifactType">ArtifactType.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <returns>The list of artifacts created.</returns>
        public List<IArtifactBase> CreateAndSaveMultipleArtifacts(IProject project,
            IUser user,
            BaseArtifactType artifactType,
            int numberOfArtifacts,
            IArtifactBase parent = null)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                IArtifact artifact = CreateAndSaveArtifact(project, user, artifactType, parent);
                artifactList.Add(artifact);
            }

            return artifactList;
        }

        /// <summary>
        /// Creates a new artifact, then saves and publishes it the specified number of times.
        /// </summary>
        /// <param name="project">The project where the artifact is to be created.</param>
        /// <param name="user">The user who will create the artifact.</param>
        /// <param name="artifactType">The type of artifact to create.</param>
        /// <param name="parent">(optional) The parent artifact. By default artifact will be created in the root of the project.</param>
        /// <param name="numberOfVersions">(optional) The number of times to save and publish the artifact (to create multiple historical versions).</param>
        /// <returns>The artifact.</returns>
        public IArtifact CreateAndPublishArtifact(IProject project, IUser user, BaseArtifactType artifactType, IArtifactBase parent = null, int numberOfVersions = 1)
        {
            IArtifact artifact = CreateArtifact(project, user, artifactType, parent);

            for (int i = 0; i < numberOfVersions; ++i)
            {
                artifact.Save();
                artifact.Publish();
            }

            return artifact;
        }

        /// <summary>
        /// Creates a list of new published artifacts.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactType">The type of artifacts to create.</param>
        /// <param name="numberOfArtifacts">The number of artifacts to create.</param>
        /// <returns>The list of artifacts.</returns>
        public List<IArtifactBase> CreateAndPublishMultipleArtifacts(IProject project, IUser user, BaseArtifactType artifactType, int numberOfArtifacts)
        {
            var artifactList = new List<IArtifactBase>();

            for (int i = 0; i < numberOfArtifacts; ++i)
            {
                IArtifact artifact = CreateAndPublishArtifact(project, user, artifactType);
                artifactList.Add(artifact);
            }

            return artifactList;
        }

        /// <summary>
        /// Creates a chain of saved parent/child artifacts of the given artifact types.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactTypeChain">The artifact types of each artifact in the chain starting at the top parent.</param>
        /// <returns>The list of artifacts in the chain starting at the top parent.</returns>
        public List<IArtifact> CreateSavedArtifactChain(IProject project, IUser user, BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            var artifactChain = new List<IArtifact>();
            IArtifact bottomArtifact = null;

            // Create artifact chain.
            foreach (BaseArtifactType artifactType in artifactTypeChain)
            {
                bottomArtifact = CreateAndSaveArtifact(project, user, artifactType, parent: bottomArtifact);
                artifactChain.Add(bottomArtifact);
            }

            return artifactChain;
        }

        /// <summary>
        /// Creates a chain of published parent/child artifacts of the given artifact types.
        /// </summary>
        /// <param name="project">The project where the artifacts are to be created.</param>
        /// <param name="user">The user who will create the artifacts.</param>
        /// <param name="artifactTypeChain">The artifact types of each artifact in the chain starting at the top parent.</param>
        /// <returns>The list of artifacts in the chain starting at the top parent.</returns>
        public List<IArtifact> CreatePublishedArtifactChain(IProject project, IUser user, BaseArtifactType[] artifactTypeChain)
        {
            ThrowIf.ArgumentNull(artifactTypeChain, nameof(artifactTypeChain));

            var artifactChain = new List<IArtifact>();
            IArtifact bottomArtifact = null;

            // Create artifact chain.
            foreach (BaseArtifactType artifactType in artifactTypeChain)
            {
                bottomArtifact = CreateAndPublishArtifact(project, user, artifactType, parent: bottomArtifact);
                artifactChain.Add(bottomArtifact);
            }

            return artifactChain;
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

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database,
        /// then assigns random fake AdminStore and/or OpenAPI tokens.
        /// </summary>
        /// <param name="targets">The authentication targets.</param>
        /// <param name="instanceAdminRole">(optional) The Instance Admin Role to assign to the user.  Pass null if you don't want any role assigned.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user that has the requested access tokens.</returns>
        public IUser CreateUserWithInvalidToken(AuthenticationTokenTypes targets,
            InstanceAdminRole? instanceAdminRole = InstanceAdminRole.DefaultInstanceAdministrator,
            UserSource source = UserSource.Database)
        {
            IUser user = CreateUserAndAddToDatabase(instanceAdminRole, source);
            string fakeTokenValue = Guid.NewGuid().ToString("N");   // 'N' creates a 32-char string with no hyphens.

            if ((targets & AuthenticationTokenTypes.AccessControlToken) != 0)
            {
                user.SetToken(fakeTokenValue);
            }

            if ((targets & AuthenticationTokenTypes.OpenApiToken) != 0)
            {
                user.SetToken(I18NHelper.FormatInvariant("{0} {1}",
                    BlueprintToken.OPENAPI_START_OF_TOKEN, fakeTokenValue));
            }

            return user;
        }

        /// <summary>
        /// Creates a user with project role permissions for one or more projects.
        /// </summary>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="projects">The list of projects that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide</param>
        /// <returns>Created authenticated user with required premissions</returns>
        public static IUser CreateUserWithProjectRolePermissions(
            TestHelper testHelper, 
            ProjectRole role, 
            List<IProject> projects, 
            IArtifactBase artifact = null)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));
            ThrowIf.ArgumentNull(projects, nameof(projects));

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            var newUser = testHelper.CreateUserAndAddToDatabase(instanceAdminRole: null);

            testHelper.AdminStore.AddSession(newUser);

            foreach (var project in projects)
            {
                AssignProjectRolePermissionsToUser(newUser, testHelper, role, project, artifact);
            }

            Logger.WriteInfo("User {0} created.", newUser.Username);

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(CreateUserWithProjectRolePermissions));

            return newUser;
        }

        /// <summary>
        /// Creates a user with project role permissions for the specified project. Optionally, creates role permissions for a single artifact within
        /// a project.
        /// </summary>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide</param>
        /// <returns>Newly created, authenticated user with required premissions</returns>
        public static IUser CreateUserWithProjectRolePermissions(
            TestHelper testHelper, 
            ProjectRole role,
            IProject project, 
            IArtifactBase artifact = null)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));
            ThrowIf.ArgumentNull(project, nameof(project));

            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            return CreateUserWithProjectRolePermissions(testHelper, role, new List<IProject> { project }, artifact);
        }

        /// <summary>
        /// Assigns project role permissions to the specified user and gets updated Session-Token.
        /// Optionally, creates role permissions for a single artifact within a project.
        /// </summary>
        /// <param name="user">User to assign role</param>
        /// <param name="testHelper">An instance of TestHelper</param>
        /// <param name="role">Author, Viewer or No permission role</param>
        /// <param name="project">The project that the role is created for</param>
        /// <param name="artifact">(optional) Specific artifact to apply permissions to instead of project-wide</param>
        /// after adding a new permissions role</param>
        public static void AssignProjectRolePermissionsToUser(IUser user, TestHelper testHelper, ProjectRole role, IProject project, IArtifactBase artifact = null)
        {
            ThrowIf.ArgumentNull(testHelper, nameof(testHelper));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));
            if (artifact != null)
            {
                Assert.IsTrue(artifact.ProjectId == project.Id, "Artifact should belong to the project");
            }

            Logger.WriteTrace("{0}.{1} called.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));

            IProjectRole projectRole = null;

            if (role == ProjectRole.Viewer)
            {
                projectRole = ProjectRoleFactory.CreateProjectRole(
                        project, RolePermissions.Read,
                        role.ToString());
            }
            else if (role == ProjectRole.AuthorFullAccess)
            {
                projectRole = ProjectRoleFactory.CreateProjectRole(
                        project,
                        RolePermissions.Delete |
                        RolePermissions.Edit |
                        RolePermissions.CanReport |
                        RolePermissions.Comment |
                        RolePermissions.DeleteAnyComment |
                        RolePermissions.CreateRapidReview |
                        RolePermissions.ExcelUpdate |
                        RolePermissions.Read |
                        RolePermissions.Reuse |
                        RolePermissions.Share |
                        RolePermissions.Trace,
                        role.ToString());
            }
            else if (role == ProjectRole.None)
            {
                projectRole = ProjectRoleFactory.CreateProjectRole(
                        project, RolePermissions.None,
                        role.ToString());
            }
            else if (role == ProjectRole.Author)
            {
                projectRole = ProjectRoleFactory.CreateProjectRole(
                        project,
                        RolePermissions.Edit |
                        RolePermissions.CanReport |
                        RolePermissions.Comment |
                        RolePermissions.CreateRapidReview |
                        RolePermissions.ExcelUpdate |
                        RolePermissions.Read |
                        RolePermissions.Reuse |
                        RolePermissions.Share |
                        RolePermissions.Trace,
                        role.ToString());
            }

            if (projectRole != null)
            {
                testHelper.ProjectRoles.Add(projectRole);
            }

            var permissionsGroup = testHelper.CreateGroupAndAddToDatabase();
            permissionsGroup.AddUser(user);
            permissionsGroup.AssignRoleToProjectOrArtifact(project, role: projectRole, artifact: artifact);

            Logger.WriteInfo("User {0} created.", user.Username);

            Logger.WriteTrace("{0}.{1} finished.", nameof(TestHelper), nameof(AssignProjectRolePermissionsToUser));
        }

        #endregion User management

        #region Group management
        /// <summary>
        /// Creates a new group object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="licenseType">(optional) The license level to assign to the group. By default it is Author.</param>
        /// <returns>A new unique group object that was added to the database.</returns>
        public IGroup CreateGroupAndAddToDatabase(GroupLicenseType licenseType = GroupLicenseType.Author)
        {
            var group = GroupFactory.CreateGroup(licenseType);
            group.AddGroupToDatabase();
            Groups.Add(group);
            return group;
        }
        #endregion Group management

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

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDate">The first date to compare.</param>
        /// <param name="secondDate">The second date to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(DateTime firstDate, DateTime secondDate, string message)
        {
            Assert.AreEqual(firstDate.ToUniversalTime(), secondDate.ToUniversalTime(), message);
        }

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDate">The first date to compare.</param>
        /// <param name="secondDateStr">The second date (as a string) to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(DateTime firstDate, string secondDateStr, string message)
        {
            DateTime secondDate = DateTime.Parse(secondDateStr, CultureInfo.InvariantCulture);
            AssertUtcDatesAreEqual(firstDate, secondDate, message);
        }

        /// <summary>
        /// Asserts that the two dates are equal.  Before comparing, this function will convert the dates to UTC time.
        /// </summary>
        /// <param name="firstDateStr">The first date (as a string) to compare.</param>
        /// <param name="secondDateStr">The second date (as a string) to compare.</param>
        /// <param name="message">The assert message to display if the dates are different.</param>
        public static void AssertUtcDatesAreEqual(string firstDateStr, string secondDateStr, string message)
        {
            DateTime firstDate = DateTime.Parse(firstDateStr, CultureInfo.InvariantCulture);
            DateTime secondDate = DateTime.Parse(secondDateStr, CultureInfo.InvariantCulture);
            AssertUtcDatesAreEqual(firstDate, secondDate, message);
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

                if (Artifacts != null)
                {
                    Logger.WriteDebug("Deleting/Discarding all artifacts created by this TestHelper instance...");
                    ArtifactBase.DisposeArtifacts(Artifacts, this);
                }

                if (Groups != null)
                {
                    Logger.WriteDebug("Deleting all groups created by this TestHelper instance...");

                    foreach (var group in Groups)
                    {
                        group.DeleteGroup();
                    }
                }

                if (ProjectRoles != null)
                {
                    Logger.WriteDebug("Deleting all project roles created by this TestHelper instance...");

                    foreach (var role in ProjectRoles)
                    {
                        role.DeleteRole();
                    }
                }

                if (Projects != null)
                {
                    Logger.WriteDebug("Deleting all projects created by this TestHelper instance...");

                    foreach (var project in Projects)
                    {
                        project.DeleteProject();
                    }
                }

                if (Users != null)
                {
                    Logger.WriteDebug("Deleting all users created by this TestHelper instance...");

                    foreach (var user in Users)
                    {
                        user.DeleteUser();
                    }
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
