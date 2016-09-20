using System;
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
                Assert.AreEqual(file1.Guid, file2.Guid,
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
            IFile file = FileStoreTestHelper.CreateFileWithRandomByteArray(fileSize, fileName, fileType);

            // Add the file to Filestore.
            var addedFile = filestore.AddFile(file, user, useMultiPartMime: true);

            FileStoreTestHelper.AssertFilesAreIdentical(file, addedFile, compareIds: false);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = filestore.GetFile(addedFile.Guid, user);

            FileStoreTestHelper.AssertFilesAreIdentical(addedFile, returnedFile);

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
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);
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
            string fileName = I18NHelper.FormatInvariant("{0}.{1}", RandomGenerator.RandomAlphaNumeric(10), "txt");
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
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileBytes);
            return file;
        }


        /// <summary>
        /// Asserts that the two Nova files are identical.
        /// </summary>
        /// <param name="file1">First file to compare.</param>
        /// <param name="file2">Second file to compare.</param>
        /// <param name="compareIds">(optional) Pass false if you don't want to include the File IDs in the comparisons.</param>
        /// <param name="compareContentLenth">(optional) Pass false if you don't want to compare file content Length</param>
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
        /// <param name="fileSize">The size of the file being created.</param>
        /// <param name="fakeFileName">The filename of the file being created.</param>
        /// <param name="fileType">The mime filetype of the file being created.</param>
        /// <returns>The created file.</returns>
        public static INovaFile CreateNovaFileWithRandomByteArray(uint fileSize, string fakeFileName, string fileType)
        {
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            INovaFile file = FileFactory.CreateNovaFile(fakeFileName, fileType, DateTime.Now, fileContents);
            return file;
        }
    }
}