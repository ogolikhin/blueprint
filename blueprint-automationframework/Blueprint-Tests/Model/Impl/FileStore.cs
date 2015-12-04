using System;
using System.Collections.Generic;

namespace Model.Impl
{
    public class FileStore : IFileStore
    {
        private const string SVC_PATH = "/svc/filestore";

        private List<IFileMetadata> _Files = new List<IFileMetadata>();

        #region Inherited from IFileStore

        public List<IFileMetadata> Files { get { return _Files; } }


        public void AddFile(IFile file)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(Guid id)
        {
            throw new NotImplementedException();
        }

        public void DeleteFile(IFile file)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(Guid id)
        {
            throw new NotImplementedException();
        }

        public IFile GetFile(IFile file)
        {
            throw new NotImplementedException();
        }

        public IFileMetadata GetFileMetadata(Guid id)
        {
            throw new NotImplementedException();
        }

        public short GetStatus()
        {
            throw new NotImplementedException();
        }

        #endregion Inherited from IFileStore
    }
}
