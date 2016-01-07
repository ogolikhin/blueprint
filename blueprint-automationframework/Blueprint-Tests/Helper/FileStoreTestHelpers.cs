using System;
using System.Text;
using Model;
using Model.Factories;
using Utilities.Factories;

namespace FileStoreTests
{
    public static class FileStoreTestHelpers
    {
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