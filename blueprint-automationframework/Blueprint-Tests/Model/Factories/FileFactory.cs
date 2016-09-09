using System;
using Model.Impl;
using Model.NovaModel.Impl;
using Model.NovaModel;

namespace Model.Factories
{
    public static class FileFactory
    {
        /// <summary>
        /// Creates a new IFile.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="lastModifiedDate">The last modified date.</param>
        /// <param name="fileContents">The contents of the file.</param>
        /// <returns>An IFile.</returns>
        public static IFile CreateFile(string fileName, string fileType, DateTime lastModifiedDate, byte[] fileContents)
        {
            IFile file = new File
            {
                Content = fileContents,
                FileName = fileName,
                FileType = fileType,
                LastModifiedDate = lastModifiedDate
            };

            return file;
        }

        /// <summary>
        /// Creates a new INovaFile.
        /// </summary>
        /// <param name="fileName">The filename.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="lastModifiedDate">The last modified date.</param>
        /// <param name="fileContents">The contents of the file.</param>
        /// <returns>An INovaFile.</returns>
        public static INovaFile CreateNovaFile(string fileName, string fileType, DateTime lastModifiedDate, byte[] fileContents)
        {
            INovaFile file = new NovaFile
            {
                Content = fileContents,
                FileName = fileName,
                FileType = fileType,
                LastModifiedDate = lastModifiedDate
            };

            return file;
        }
    }
}
