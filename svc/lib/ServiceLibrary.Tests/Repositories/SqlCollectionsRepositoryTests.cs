using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlCollectionsRepositoryTests
    {
        private const int UserId = 1;
        private Session _session;

        private Mock<ICollectionsRepository> _collectionsRepositoryMock;
        private SqlConnectionWrapperMock _cxn;
        private SqlCollectionsRepository _repository;

        [TestInitialize]
        public void Initialize()
        {
            _collectionsRepositoryMock = new Mock<ICollectionsRepository>();
            _cxn = new SqlConnectionWrapperMock();
            _repository = new SqlCollectionsRepository(_cxn.Object);
            _session = new Session { UserId = UserId };
        }
    }
}
