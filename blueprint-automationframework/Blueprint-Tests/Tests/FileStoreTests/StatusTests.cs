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
        public void StatusUpcheck_OK()
        {
            Assert.DoesNotThrow(() =>
            {
                _filestore.GetStatusUpcheck();
            });
        }
    }
}
