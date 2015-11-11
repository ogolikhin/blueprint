using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdminStore.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;

namespace AdminStore.Repositories
{
    [TestClass]
    public class SqlConfigRepositoryTests
    {
        #region Constuctor

        [TestMethod]
        public void Constructor_CreatesConnectionToAdminStorage()
        {
            // Arrange

            // Act
            var repository = new SqlConfigRepository();

            // Assert
            Assert.AreEqual(WebApiConfig.AdminStorage, repository._connectionWrapper.CreateConnection().ConnectionString);
        }

        #endregion Constructor

        #region GetLabels

        [TestMethod]
        public async Task GetLabels_QueryReturnsLabels_ReturnsLabels()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlConfigRepository(cxn.Object);
            var locale = "en-CA";
            ApplicationLabel[] result =
            {
                new ApplicationLabel {Key = "key", Locale = locale, Text = "text"},
                new ApplicationLabel {Key = "key2", Locale = locale, Text = "text2"}
            };
            cxn.SetupQueryAsync("GetApplicationLabels", new Dictionary<string, object> { { "Locale", locale } }, result);

            // Act
            IEnumerable<ApplicationLabel> labels = await repository.GetLabels(locale);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEqual(result, labels.ToList());
        }

        #endregion GetLabels
    }
}
