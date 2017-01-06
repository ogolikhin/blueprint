using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class FileRepositoryTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NoHttpWebClient_ThrowsArgumentNullException()
        {
            // Arrange
            IHttpWebClient httpWebClient = null;

            // Act
            var repository = new FileRepository(httpWebClient);
        }
    }
}
