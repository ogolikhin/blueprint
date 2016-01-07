using System.Net;
using System.Net.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.LocalLog;

namespace ServiceLibrary.Repositories.ConfigControl
{
    [TestClass]
    public class StatusRepositoryTests
    {
        [TestMethod]
        public async Task Status()
        {
            // Arrange

            var httpClientProvider = new TestHttpClientProvider(request =>
            {
                if (request.Method == HttpMethod.Get)
                {
                    return new HttpResponseMessage(HttpStatusCode.OK);
                }
                return new HttpResponseMessage(HttpStatusCode.NotFound);
            });
            var servicelog = new StatusRepository(httpClientProvider, new LocalFileLog());

            // Act
            var status = await servicelog.GetStatus();

            // Assert
            Assert.IsTrue(status);
        }

    }
}
