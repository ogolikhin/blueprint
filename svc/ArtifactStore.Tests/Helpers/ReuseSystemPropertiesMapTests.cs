using System;
using System.Linq;
using ArtifactStore.Models;
using ArtifactStore.Models.Reuse;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLibrary.Models;

namespace ArtifactStore.Helpers
{
    [TestClass]
    public class ReuseSystemPropertiesMapTests
    {
        [TestMethod]
        public void GetPropertyTypePredefined_MapDoesNotContainProperty_EmptyListShouldBeReturned()
        {
            //Arrange


            //Assert
            var ptf = ReuseSystemPropertiesMap.Instance.GetPropertyTypePredefined(ReconcileProperty.Name,
                ItemTypePredefined.Actor);

            //Act
            Assert.IsFalse(!ptf.Any(), "No property type info should have been returned");
        }

        [TestMethod]
        public void GetPropertyTypePredefined_MapContainsProperty_PropertyListShouldBeReturned()
        {
            //Arrange


            //Assert
            var ptf = ReuseSystemPropertiesMap.Instance.GetPropertyTypePredefined(ReconcileProperty.ActorImageName,
                ItemTypePredefined.Actor).ToList();

            //Act
            Assert.IsNotNull(ptf, "property type info should have been returned");
            Assert.IsTrue(ptf.Count == 2, "2 property type infos should have been returned");
            Assert.IsTrue(ptf[0] == PropertyTypePredefined.RawData, "first property type info should have been RawData");
            Assert.IsTrue(ptf[1] == PropertyTypePredefined.Image, "second property type info should have been Image");
        }
    }
}
