﻿using System;
using System.Text;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class FileStoreTestHelper
    {
        /// <summary>
        /// Asserts that the two files are identical.
        /// </summary>
        /// <param name="file1">First file to compare.</param>
        /// <param name="file2">Second file to compare.</param>
        /// <param name="compareIds">(optional) Pass false if you don't want to include the File IDs in the comparisons.</param>
        /// <param name="compareContent">(optional) Pass false if you don't want to compare file content (i.e. if you are only comparing meta-data).</param>
        /// <exception cref="AssertionException">If file1 is not identical to file2.</exception>
        public static void AssertFilesAreIdentical(IFile file1, IFile file2, bool compareIds = true, bool compareContent = true)
        {
            ThrowIf.ArgumentNull(file1, nameof(file1));
            ThrowIf.ArgumentNull(file2, nameof(file2));

            Assert.AreEqual(file1.FileName, file2.FileName,
                "The file name of the files don't match!");
            Assert.AreEqual(file1.FileType, file2.FileType,
                "The file type of the files don't match!");

            if (compareContent)
            {
                Assert.AreEqual(file1.Content, file2.Content,
                    "The file content of the files don't match!");
            }

            if (compareIds)
            {
                Assert.AreEqual(file1.Id, file2.Id,
                    "The file Id of the files don't match!");
            }
        }

        /// <summary>
        /// Asserts that the meta-data of the two files are identical.
        /// </summary>
        /// <param name="file1">First file's meta-data to compare.</param>
        /// <param name="file2">Second file's meta-data to compare.</param>
        /// <param name="compareIds">(optional) Pass false if you don't want to include the File IDs in the comparisons.</param>
        /// <exception cref="AssertionException">If file1 is not identical to file2.</exception>
        public static void AssertFilesMetadataAreIdentical(IFileMetadata file1, IFileMetadata file2, bool compareIds = true)
        {
            ThrowIf.ArgumentNull(file1, nameof(file1));
            ThrowIf.ArgumentNull(file2, nameof(file2));

            Assert.AreEqual(file1.FileName, file2.FileName,
                "The file name of the files don't match!");
            Assert.AreEqual(file1.FileType, file2.FileType,
                "The file type of the files don't match!");

            if (compareIds)
            {
                Assert.AreEqual(file1.Id, file2.Id,
                    "The file Id of the files don't match!");
            }
        }

        /// <summary>
        /// Creates a random file and adds it to FileStore.
        /// </summary>
        /// <param name="fileSize">The size of the file to create.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileType">The file type.</param>
        /// <param name="filestore">The FileStore to add the file into.</param>
        /// <param name="user">A user with a valid token to authenticate to FileStore.</param>
        /// <returns>The file that was added to FileStore.</returns>
        /// <exception cref="AssertionException">If the file was not added to FileStore.</exception>
        public static IFile CreateAndAddFile(uint fileSize, string fileName, string fileType, IFileStore filestore, IUser user)
        {
            ThrowIf.ArgumentNull(filestore, nameof(filestore));

            // Create a fake file with a random byte array.
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fileName, fileType);

            // Add the file to Filestore.
            var addedFile = filestore.AddFile(file, user, useMultiPartMime: true);

            FileStoreTestHelper.AssertFilesAreIdentical(file, addedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = filestore.GetFile(addedFile.Id, user);

            FileStoreTestHelper.AssertFilesAreIdentical(addedFile, returnedFile);

            return addedFile;
        }

        /// <summary>
        /// Create a file consisting of a random byte array
        /// </summary>
        /// <param name="fileSize">The size of the file being created</param>
        /// <param name="fakeFileName">The filename of the file being created</param>
        /// <param name="fileType">The mime filetype of the file being created</param>
        /// <returns>The created file</returns>
        public static IFile CreateFileWithRandomByteArray(uint fileSize, string fakeFileName, string fileType)
        {
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);
            return file;
        }
    }
}