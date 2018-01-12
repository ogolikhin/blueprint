using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class CollectionsRepositoryTests
    {
        private Mock<ICollectionsRepository> _collectionsRepositoryMock;
        private Mock<IArtifactRepository> _artifactRepositoryMock;
        private SqlConnectionWrapperMock cxn;
        private CollectionsRepository repository;
        private Session session;
        private const int userId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _collectionsRepositoryMock = new Mock<ICollectionsRepository>();
            _artifactRepositoryMock = new Mock<IArtifactRepository>();
            cxn = new SqlConnectionWrapperMock();
            repository = new CollectionsRepository(cxn.Object, new SqlArtifactRepository(), new SqlHelper());
            session = new Session { UserId = userId };
        }
    }
}
