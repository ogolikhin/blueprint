using System;
using System.Collections.Generic;

namespace Model
{
    public interface IFileStore
    {
        List<IFileMetadata> Files { get; }


        void AddFile(IFile file);

        void DeleteFile(Guid id);
        void DeleteFile(IFile file);

        IFile GetFile(Guid id);
        IFile GetFile(IFile file);

        IFileMetadata GetFileMetadata(Guid id);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")] // Ignore this warning.
        short GetStatus();
    }
}
