using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ServiceLibrary.Models;
using System.Data;
using ServiceLibrary.Repositories.ApplicationSettings;
using System.Threading.Tasks;

namespace ServiceLibrary.Repositories
{
    [TestClass]
    public class SqlFeaturesRepositoryTests
    {
        private Mock<ISqlConnectionWrapper> _sqlConnectionWrapperMock;
        private SqlFeaturesRepository _sqlFeaturesRepository;

        [TestInitialize]
        public void TestInitialize()
        {
            _sqlConnectionWrapperMock = new Mock<ISqlConnectionWrapper>(MockBehavior.Strict);
            _sqlFeaturesRepository = new SqlFeaturesRepository(_sqlConnectionWrapperMock.Object);
        }

        [TestMethod]
        public async Task SqlFeaturesRepository_ReturnsFeaturesFromDb()
        {
            // Arrange
            var features = new[] { new Feature { Name = "Feature", Enabled = true } };
            _sqlConnectionWrapperMock
                .Setup(cw =>
                    cw.QueryAsync<Feature>(
                        It.IsAny<string>(),
                        It.IsAny<object>(),
                        It.IsAny<IDbTransaction>(),
                        It.IsAny<int?>(),
                        It.IsAny<CommandType?>()))
                .ReturnsAsync(features);

            // Act
            var actualFeatures = await _sqlFeaturesRepository.GetFeaturesAsync();

            // Assert
            Assert.AreSame(features, actualFeatures);
        }
    }
}
