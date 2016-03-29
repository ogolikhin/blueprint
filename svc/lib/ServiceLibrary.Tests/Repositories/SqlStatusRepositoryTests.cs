﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlStatusRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_ConnectionString_CreatesConnectionWithString()
        {
            // Arrange
            string cxn = "data source=(local)";
            string cmd = "command";

            // Act
            var repository = new SqlStatusRepository(cxn, cmd, "test");

            // Assert
            Assert.AreEqual(cxn, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetStatus

        [TestMethod]
        public async Task GetStatus_QueryReturnsVersion_ReturnsCorrectVersionString()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var cmd = "Test";
            var repository = new SqlStatusRepository(cxn.Object, cmd, "Test");
            IEnumerable<string> result = new[] { "7.0.0.0" };
            cxn.SetupQueryAsync(cmd, null, result);

            // Act
            string status = await repository.GetStatus();

            // Assert
            cxn.Verify();
            Assert.AreEqual(status, "7.0.0.0");
        }

        #endregion GetStatus
    }
}
