using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ArtifactStore.Services.Workflow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;
using ServiceLibrary.Repositories.ConfigControl;

namespace ArtifactStore.Controllers
{
    [TestClass]
    public class CollectionsControllerTests
    {
        private Mock<ICollectionsService> _collectionsServiceMock;

        [TestInitialize]
        public void Initialize()
        {
            _collectionsServiceMock = new Mock<ICollectionsService>();
        }
    }
}