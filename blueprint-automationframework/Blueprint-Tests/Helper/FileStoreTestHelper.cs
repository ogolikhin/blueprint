﻿using System;
using System.Text;
using Common;
using Model;
using Model.Factories;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;
using Model.NovaModel;

namespace Helper
{
    public static class FileStoreTestHelper
    {
        /// <summary>
        /// Asserts that the two files are identical.
        /// </summary>
        /// <param name="expectedFile">The expected file to compare.</param>
        /// <param name="actualFile">The actual file to compare.</param>
        /// <param name="compareIds">(optional) Pass false if you don't want to include the File IDs in the comparisons.</param>
        /// <param name="compareContent">(optional) Pass false if you don't want to compare file content (i.e. if you are only comparing meta-data).</param>
        /// <param name="compareFileNames">(optional) Pass false if you don't want to compare the FileName's.</param>
        /// <exception cref="AssertionException">If file1 is not identical to file2.</exception>
        public static void AssertFilesAreIdentical(IFile expectedFile, IFile actualFile,
            bool compareIds = true, bool compareContent = true, bool compareFileNames = true)
        {
            ThrowIf.ArgumentNull(expectedFile, nameof(expectedFile));
            ThrowIf.ArgumentNull(actualFile, nameof(actualFile));

            if (compareFileNames)
            {
                Assert.AreEqual(expectedFile.FileName, actualFile.FileName,
                "The file name of the files don't match!");
            }

            Assert.AreEqual(expectedFile.FileType, actualFile.FileType,
                "The file type of the files don't match!");

            if (compareFileNames)
            {
                Assert.AreEqual(expectedFile.FileName, actualFile.FileName,
                    "The file name of the files don't match!");
            }

            if (compareContent)
            {
                Assert.AreEqual(expectedFile.Content, actualFile.Content,
                    "The file content of the files don't match!");
            }

            if (compareIds)
            {
                Assert.AreEqual(expectedFile.Guid, actualFile.Guid,
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
                Assert.AreEqual(file1.Guid, file2.Guid,
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
            var file = CreateFileWithRandomByteArray(fileSize, fileName, fileType);

            // Add the file to Filestore.
            var addedFile = filestore.AddFile(file, user, useMultiPartMime: true);

            AssertFilesAreIdentical(file, addedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = filestore.GetFile(addedFile.Guid, user);

            AssertFilesAreIdentical(addedFile, returnedFile);

            return addedFile;
        }

        /// <summary>
        /// Create a file consisting of a random byte array.
        /// </summary>
        /// <param name="fileSize">The size of the file being created.</param>
        /// <param name="fakeFileName">The filename of the file being created.</param>
        /// <param name="fileType">The mime filetype of the file being created.</param>
        /// <returns>The created file.</returns>
        public static IFile CreateFileWithRandomByteArray(uint fileSize, string fakeFileName, string fileType)
        {
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);
            return file;
        }

        /// <summary>
        /// Create a file consisting of a random name, size & contents.
        /// </summary>
        /// <param name="fileType">(optional) The file type.</param>
        /// <returns>The created file.</returns>
        public static IFile CreateFileWithRandomByteArray(string fileType = "text/plain")
        {
            uint fileSize = (uint)RandomGenerator.RandomNumber(4096);
            string fileName = I18NHelper.FormatInvariant("{0}.txt", RandomGenerator.RandomAlphaNumeric(10));
            return CreateFileWithRandomByteArray(fileSize, fileName, fileType);
        }

        /// <summary>
        /// Create a file with the specified contents.
        /// </summary>
        /// <param name="fakeFileName">The filename of the file being created.</param>
        /// <param name="fileType">The mime filetype of the file being created.</param>
        /// <param name="fileContents">The contents of the file.</param>
        /// <param name="encoding">(optional) The encoding of the file contents.  Default is Unicode.</param>
        /// <returns>The created file.</returns>
        public static IFile CreateFileWithStringContents(string fakeFileName, string fileType, string fileContents, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.Unicode;
            byte[] fileBytes = encoding.GetBytes(fileContents);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileBytes);
            return file;
        }


        /// <summary>
        /// Asserts that the two Nova files are identical.
        /// </summary>
        /// <param name="file1">First file to compare.</param>
        /// <param name="file2">Second file to compare.</param>
        /// <param name="compareIds">(optional) Pass false if you don't want to include the File IDs in the comparisons.</param>
        /// <param name="compareContentLength">(optional) Pass false if you don't want to compare file content Length</param>
        /// <exception cref="AssertionException">If file1 is not identical to file2.</exception>
        public static void AssertNovaFilesAreIdentical(INovaFile file1, INovaFile file2, bool compareIds = true, bool compareContentLength = true)
        {
            ThrowIf.ArgumentNull(file1, nameof(file1));
            ThrowIf.ArgumentNull(file2, nameof(file2));

            Assert.AreEqual(file1.FileName, file2.FileName, "The file name of the files don't match!");

            if (compareContentLength)
            {
                Assert.AreEqual(file1.ContentLength, file2.ContentLength, "The file content length of the files don't match!");
            }

            if (compareIds)
            {
                Assert.AreEqual(file1.Guid, file2.Guid, "The file Id of the files don't match!");
            }
        }


        /// <summary>
        /// Create a file consisting of a random byte array.
        /// </summary>
        /// <param name="fileSize">(optional) The size of the file being created.  Defaults to a random size.</param>
        /// <param name="fakeFileName">(optional) The filename of the file being created.  Defaults to a random string.</param>
        /// <param name="fileType">(optional) The mime filetype of the file being created.</param>
        /// <returns>The created file.</returns>
        public static INovaFile CreateNovaFileWithRandomByteArray(uint? fileSize = null, string fakeFileName = null, string fileType = "text/plain")
        {
            if (fileSize == null)
            {
                fileSize = (uint)RandomGenerator.RandomNumber(4096);
            }

            if (fakeFileName == null)
            {
                fakeFileName = I18NHelper.FormatInvariant("{0}.txt", RandomGenerator.RandomAlphaNumeric(10));
            }

            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize.Value);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);

            var file = FileFactory.CreateNovaFile(fakeFileName, fileType, DateTime.Now, fileContents);
            return file;
        }

        /// <summary>
        /// Create a file consisting of a random byte array.
        /// </summary>
        /// <param name="user">User to perform an operation.</param>
        /// <param name="fileName">The filename of the file being uploaded.</param>
        /// <param name="fileType">The mime filetype of the file being uploaded.</param>
        /// <param name="expireTime">Expire time of the file being uploaded.</param>
        /// <param name="fileStore">The FileStore to add the file into.</param>
        /// <returns>The uploaded file with valid Guid from FileStore DB.</returns>
        public static INovaFile UploadNovaFileToFileStore(IUser user, string fileName, string fileType,
            DateTime expireTime, IFileStore fileStore)
        {
            ThrowIf.ArgumentNull(fileStore, nameof(fileStore));
            var fileToUpload = CreateNovaFileWithRandomByteArray((uint)2048, fileName, fileType);
            var uploadedFile = fileStore.AddFile(fileToUpload, user, expireTime: expireTime);
            Assert.IsNotNull(fileStore.GetSQLExpiredTime(uploadedFile.Guid), "Uploaded file shouldn't have null ExpiredTime");
            return uploadedFile;
        }
    }
}