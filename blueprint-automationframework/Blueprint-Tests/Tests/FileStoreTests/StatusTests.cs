using System.Net;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;

namespace FileStoreTests
{
    [TestFixture]
    [Category(Categories.Filestore)]
    public class StatusTests
    {
        private IFileStore _filestore;

        [SetUp]
        public void SetUp()
        {
            _filestore = FileStoreFactory.GetFileStoreFromTestConfig();
        }

        [Test]
        public void Status_OK()
        {
            // Add the file to Filestore.
            var response = _filestore.GetStatus();

            Assert.That(response == HttpStatusCode.OK, "File store service status is not OK!");
        }
    }
}
