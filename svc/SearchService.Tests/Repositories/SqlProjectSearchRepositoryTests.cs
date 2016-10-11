using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchService.Models;
using ServiceLibrary.Repositories;

namespace SearchService.Repositories
{
    [TestClass]
    public class SqlProjectSearchRepositoryTests
    {
        private IProjectSearchRepository _projectSearchRepository;
        private SqlConnectionWrapperMock _cxn;

        [TestInitialize]
        public void Initialize()
        {
            _cxn = new SqlConnectionWrapperMock();
            _projectSearchRepository = new SqlProjectSearchRepository(_cxn.Object);
        }

        [TestMethod]
        public async Task GetProjects_EmptyListReturned()
        {
            // Arrange
            const int userId = 1;
            const int resultCount = 1;
            const string searchText = "test";
            const string separatorChar = "/";
            _cxn.SetupQueryAsync("GetProjectsByName",
                new Dictionary<string, object> { { "userId", userId }, { "projectName", searchText }, { "resultCount", resultCount }, { "separatorChar", separatorChar } },
                new List<ProjectSearchResult>());
            // Act
            var result = (await _projectSearchRepository.GetProjectsByName(userId, searchText, resultCount, separatorChar)).ToList();
            Assert.AreEqual(0, result.Count);
        }
    }
}
