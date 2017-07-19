using ConfigControl.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Repositories;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConfigControl.Repositories
{
    [TestClass]
    public class SqlConfigRepositoryTests
    {
        #region GetSettings

        [TestMethod]
        public async Task GetSettings_QueryReturnsResults_ReturnsResults()
        {
            // Arrange
            var cxn = new SqlConnectionWrapperMock();
            var repository = new SqlConfigRepository(cxn.Object);
            ConfigSetting[] result =
            {
                new ConfigSetting { Group = "group", IsRestricted = false, Key = "key", Value = "value" },
                new ConfigSetting { Group = "group", IsRestricted = true, Key = "key2", Value = "value2" }
            };
            cxn.SetupQueryAsync(
                "GetConfigSettings",
                new Dictionary<string, object> { { "AllowRestricted", true } },
                result);

            // Act
            IEnumerable<ConfigSetting> settings = await repository.GetSettings(true);

            // Assert
            cxn.Verify();
            CollectionAssert.AreEquivalent(result, settings.ToList());
        }

        #endregion GetSettings
    }
}
