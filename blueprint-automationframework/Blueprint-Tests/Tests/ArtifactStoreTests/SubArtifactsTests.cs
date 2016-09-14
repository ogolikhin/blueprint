using System.Collections.Generic;
using Common;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Utilities;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class SubArtifactsTests : TestBase
    {
        private IUser _user = null;

        private int useCaseId = 11;

        [SetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.BothAccessControlAndOpenApiTokens);
        }

        [TearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }

        #region Custom data tests

        [Category(Categories.CustomData)]
        [Test]
        [TestRail(165855)]
        [Description("GetSubartifacts for Use Case from Custom Data project. Check that results have expected content.")]
        public void GetSubArtifacts_CustomProjectUseCase_ReturnsCorrectSubArtifactsList()
        {
            List<INovaSubArtifact> subArtifacts = null;
            Assert.DoesNotThrow(() =>
            {
                subArtifacts = Helper.ArtifactStore.GetSubartifacts(_user, useCaseId);
            }, "GetSubartifacts shouldn't throw an error.");

            Assert.AreEqual(4, subArtifacts.Count, ".");
        }

        #endregion Custom Data
    }
}
