using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using ServiceLibrary.Repositories;

namespace ArtifactStore.Repositories
{
    [TestClass]
    public class CollectionsRepositoryTests
    {
        private Mock<ICollectionsRepository> _collectionsRepositoryMock;
        private SqlConnectionWrapperMock cxn;
        private CollectionsRepository repository;
        private Session session;
        private const int userId = 1;

        [TestInitialize]
        public void Initialize()
        {
            _collectionsRepositoryMock = new Mock<ICollectionsRepository>();
            cxn = new SqlConnectionWrapperMock();
            repository = new CollectionsRepository(cxn.Object);
            session = new Session { UserId = userId };
        }
    }
}
