using System;
using System.Collections.Generic;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.StorytellerModel;
using NUnit.Framework;

namespace Helper
{
    public class TestHelper : IDisposable
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
        public IList<IArtifactBase> Artifacts { get; } = new List<IArtifactBase>();
        public IList<IProject> Projects { get; } = new List<IProject>();
        public IList<IUser> Users { get; } = new List<IUser>();

        #region User management

        /// <summary>
        /// Creates a new user object with random values and adds it to the Blueprint database.
        /// </summary>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new unique user object that was added to the database.</returns>
        public IUser CreateUserAndAddToDatabase(UserSource source = UserSource.Database)
        {
            IUser user = UserFactory.CreateUserAndAddToDatabase(source);
            Users.Add(user);
            return user;
        }

        /// <summary>
        /// Creates a new user object with random values, but with the username & password specified
        /// and adds it to the Blueprint database.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user object.</returns>
        public IUser CreateUserAndAddToDatabase(string username, string password,
            UserSource source = UserSource.Database)
        {
            IUser user = UserFactory.CreateUserAndAddToDatabase(username, password, source);
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
        /// <param name="source">(optional) Where the user exists.</param>
        /// <returns>A new user that has the requested access tokens.</returns>
        public IUser CreateUserAndAuthenticate(AuthenticationTokenTypes targets, UserSource source = UserSource.Database)
        {
            IUser user = CreateUserAndAddToDatabase(source);

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

        #region Members inherited from IDisposable

        /// <summary>
        /// Disposes this object and all disposable objects owned by this object.
        /// </summary>
        /// <param name="disposing">Pass true if explicitly called, or false if called from the destructor.</param>
        protected virtual void Dispose(bool disposing)
        {
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

                foreach (var artifact in Artifacts)
                {
                    artifact.Delete();
                    artifact.Publish();
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
