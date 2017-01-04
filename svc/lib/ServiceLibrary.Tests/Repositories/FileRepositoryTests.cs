using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class FileRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NoUri_ThrowsArgumentNullException()
        {
            // Arrange
            Uri uri = null;

            // Act
            var repository = new FileRepository(uri);
            repository = null;
        }
    }
}
