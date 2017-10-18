using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using ServiceLibrary.Helpers;

namespace ServiceLibrary.Models
{
    [TestClass]
    public class LicenseTransactionTests
    {
        [TestMethod]
        public void LicenseTransaction_ActiveLicenses_Deserialization_Test()
        {
            // Arrange
            var data = new LicenseTransaction
            {
                Details = "0:1;1:2;2:1;"
            };

            var json = JsonConvert.SerializeObject(data);

            // Act
            var deserializedData = JsonConvert.DeserializeObject<LicenseTransaction>(json);

            // Assert
            Assert.IsNotNull(deserializedData.ActiveLicenses);
            AssertDictionariesEqual(data.ActiveLicenses, deserializedData.ActiveLicenses);
        }

        private void AssertDictionariesEqual<K, V>(IDictionary<K, V> expected, IDictionary<K, V> actual)
        {
            if (ReferenceEquals(expected, actual))
                return;

            Assert.IsNotNull(expected, "Expected dictionary is null");
            Assert.IsNotNull(actual, "Actual dictionary is null");

            Assert.AreEqual(expected.Count, actual.Count, "Different sizes");

            foreach (var pair in expected)
            {
                V actualValue;
                Assert.IsTrue(actual.TryGetValue(pair.Key, out actualValue), I18NHelper.FormatInvariant("Missing key: {0}", pair.Key));
                Assert.AreEqual(pair.Value, actualValue, I18NHelper.FormatInvariant("For key: {0}", pair.Key));
            }
        }
    }
}
