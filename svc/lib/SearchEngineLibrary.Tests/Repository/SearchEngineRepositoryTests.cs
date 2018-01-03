using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SearchEngineLibrary.Repository;
using ServiceLibrary.Repositories;

namespace SearchEngineLibrary.Tests.Repository
{
    [TestClass]
    public class SearchEngineRepositoryTests
    {
        private ISearchEngineRepository _searchEngineRepository;
        private SqlConnectionWrapperMock _sqlConnectionWrapperMock;

        [TestInitialize]
        public void Init()
        {            
            _sqlConnectionWrapperMock = new SqlConnectionWrapperMock();

            _searchEngineRepository = new SearchEngineRepository(_sqlConnectionWrapperMock.Object);
        }
    }
}
