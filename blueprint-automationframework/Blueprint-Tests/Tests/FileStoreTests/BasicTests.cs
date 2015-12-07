using System;
using System.Text;
using CustomAttributes;
using Helper.Factories;
using Model;
using Model.Factories;
using NUnit.Framework;
using TestConfig;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class BasicTests
    {
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();

        private IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
        private IFileStore _filestore = null;
        private IUser _user = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
            _filestore = FileStoreFactory.CreateFileStore(_server.Address);
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [TestCase((uint)1024, "1KB_File.txt", "Text", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "Text", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "Text", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "Text", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void AddFile_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            string randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            byte[] fileContents = Encoding.ASCII.GetBytes(randomChunk);
            IFile file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            IFile storedFile = _filestore.AddFile(file, _user);

            try
            {
                // Verify that the file was stored properly by getting it back and comparing it with original.
                IFile returnedFile = _filestore.GetFile(storedFile.Id);
                Assert.AreEqual(file.Content, returnedFile.Content,
                    "The file bytes returned from FileStore do not match the bytes we added!");
            }
            finally
            {
                // Cleanup.
                _filestore.DeleteFile(storedFile);
            }
        }
    }
}
