using System;
using System.Net;
using System.Text;
using CustomAttributes;
using Helper.Factories;
using Model;
using Model.Factories;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using TestConfig;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class BasicTests
    {
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();

        private readonly IBlueprintServer _server = BlueprintServerFactory.GetBlueprintServerFromTestConfig();
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

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            var randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            var fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, useMultiPartMime:true);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            var deleteFile = _filestore.GetFile(storedFile.Id, _user, HttpStatusCode.NotFound);
            Assert.IsNull(deleteFile);

        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithoutMultiMimeParts_OK(uint fileSize, string fakeFileName, string fileType)
        {
            // Setup: create a fake file with a random byte array.
            var randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            var fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            var deleteFile = _filestore.GetFile(storedFile.Id, _user, HttpStatusCode.NotFound);
            Assert.IsNull(deleteFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithExpireTime_OK(uint fileSize, string fakeFileName, string fileType, string deleteExpireDateTime)
        {
            // Setup: create a fake file with a random byte array.
            var randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            var fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, Convert.ToDateTime(deleteExpireDateTime));

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user);

            var deleteFile = _filestore.GetFile(storedFile.Id, _user, HttpStatusCode.NotFound);
            Assert.IsNull(deleteFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void PostFileWithDeleteExpireTime_OK(uint fileSize, string fakeFileName, string fileType, string deleteExpireDateTime)
        {
            // Setup: create a fake file with a random byte array.
            var randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            var fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);
            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual(fileType, returnedFile.FileType,
                "The file type returned does not math the type sent!");

            _filestore.DeleteFile(storedFile.Id, _user, Convert.ToDateTime(deleteExpireDateTime));

            var deleteFile = _filestore.GetFile(storedFile.Id, _user, HttpStatusCode.NotFound);
            Assert.IsNull(deleteFile);
        }

        [TestCase((uint)1024, "1KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)2048, "2KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)4096, "4KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        [TestCase((uint)8192, "8KB_File.txt", "text/plain", "2016-12-13", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void GetHeadOnly_OK(uint fileSize, string fakeFileName, string fileType, string deleteExpireDateTime)
        {
            // Setup: create a fake file with a random byte array.
            var randomChunk = RandomGenerator.RandomAlphaNumericUpperAndLowerCase(fileSize);
            var fileContents = Encoding.ASCII.GetBytes(randomChunk);
            var file = FileFactory.CreateFile(fakeFileName, fileType, DateTime.Now, fileContents);

            // Add the file to Filestore.
            var storedFile = _filestore.AddFile(file, _user, Convert.ToDateTime(deleteExpireDateTime), true);

            // Verify that the file was stored properly by getting it back and comparing it with original.
            var returnedFile = _filestore.GetFile(storedFile.Id, _user);

            Assert.AreEqual(file.Content, returnedFile.Content,
                "The file bytes returned from FileStore do not match the bytes we added!");
            Assert.AreEqual(fakeFileName, returnedFile.FileName,
                "The file returned filename does not match the filename of the sent file!");
            Assert.AreEqual("text/plain", returnedFile.FileType,
                "The file type returned does not math the type sent!");
        }

        [TestCase((uint)1024, 500, "1KB_File.txt", "text/plain", Explicit = true, Reason = IgnoreReasons.UnderDevelopment)]
        public void Status_OK(uint fileSize, int chunkSize, string fakeFileName, string fileType)
        { 
            // Add the file to Filestore.
            var response = _filestore.GetStatus();

            Assert.That(response == HttpStatusCode.OK, "Status is not OK!");
        }
    }
}
