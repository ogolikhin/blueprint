using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class FileRepositoryTests
    {
        [SuppressMessage("Microsoft.Usage", "CA1806:DoNotIgnoreMethodResults", MessageId = "ServiceLibrary.Repositories.FileRepository")]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_NoHttpWebClient_ThrowsArgumentNullException()
        {
            // Arrange
            IHttpWebClient httpWebClient = null;

            // Act
            new FileRepository(httpWebClient);
        }
    }
}
