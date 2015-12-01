using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FileStore.Repositories
{
    [TestClass]
    public class FileMapperRepositoryTests
    {
        private FileMapperRepository _fileMapperRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _fileMapperRepository = new FileMapperRepository();
        }

        [TestMethod]
        public void GetMappedOutputContentType_NullValue()
        {
            //Arrange
            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(null);

            //Assert
            Assert.AreEqual(FileMapperRepository.DefaultMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_EmptyValue()
        {
            //Arrange
            string input = "";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.DefaultMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_StringSpace()
        {
            //Arrange
            string input = "   ";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.DefaultMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_JunkValue()
        {
            //Arrange
            string input = "  asdasdasdsdas ";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.DefaultMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_TxtValue()
        {
            //Arrange
            string input = ".Txt";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.TextMediaType, result);
        }

        

        

        

        

        

        

        [TestMethod]
        public void GetMappedOutputContentType_bmpValue()
        {
            //Arrange
            string input = ".bmp";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.BmpMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_iefValue()
        {
            //Arrange
            string input = ".ief";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.IefMediaType, result);
        }

        

        [TestMethod]
        public void GetMappedOutputContentType_tifValue()
        {
            //Arrange
            string input = ".tif";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.TiffMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_tiffValue()
        {
            //Arrange
            string input = ".tiff";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.TiffMediaType, result);
        }

        

        [TestMethod]
        public void GetMappedOutputContentType_HtmValue()
        {
            //Arrange
            string input = ".htm";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.HtmlMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_HtmlValue()
        {
            //Arrange
            string input = ".html";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.HtmlMediaType, result);
        }

        [TestMethod]
        public void GetMappedOutputContentType_CssValue()
        {
            //Arrange
            string input = ".css";

            //Act
            var result = _fileMapperRepository.GetMappedOutputContentType(input);

            //Assert
            Assert.AreEqual(FileMapperRepository.CssMediaType, result);
        }

    }
}
