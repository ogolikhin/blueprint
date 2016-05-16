using System;
using System.Collections.Generic;
using Model;
using Model.ArtifactModel;
using Model.Factories;
using Model.StorytellerModel;

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
        public IFileStore Filestore { get; } = FileStoreFactory.GetFileStoreFromTestConfig();
        public IStoryteller Storyteller { get; } = StorytellerFactory.GetStorytellerFromTestConfig();

        // Lists of objects created by this class to be disposed:
        public IList<IArtifactBase> Artifacts { get; } = new List<IArtifactBase>();
        public IList<IProject> Projects { get; } = new List<IProject>();
        public IList<IUser> Users { get; } = new List<IUser>();

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
                Filestore?.Dispose();
                BlueprintServer?.Dispose();
                ArtifactStore?.Dispose();
                AdminStore?.Dispose();
                AccessControl?.Dispose();

                /*
                foreach (var artifact in Artifacts)
                {
                    artifact.Delete();
                    artifact.Publish();
                }
                */

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
