using System.Collections.Generic;
using System.Linq;
using CustomAttributes;
using Helper;
using Model;
using Model.ArtifactModel.Impl;
using Model.Factories;
using NUnit.Framework;
using TestCommon;
using Model.ArtifactModel.Impl.PredefinedProperties;

namespace ArtifactStoreTests
{
    [TestFixture]
    [Category(Categories.ArtifactStore)]
    public class ActorPropertiesTests : TestBase
    {
        private IUser _user = null;

        private int actorInheritedFromOtherActorId = 8;
        private int parentActorId = 9;

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

        [Test]
        [TestRail(165800)]
        [Description("Gets ArtifactDetails for the actor with non-empty Inherited From field. Verify the inherited from object has expected information.")]
        public void GetActorInheritance_CustomProject_ReturnsActorInheritance()
        {
            NovaArtifactDetails artifactDetails = Helper.ArtifactStore.GetArtifactDetails(_user, actorInheritedFromOtherActorId); ;
            ActorInheritanceValue actorInheritance = null;

            actorInheritance = artifactDetails.GetActorInheritance();
            Assert.AreEqual(parentActorId, actorInheritance.ActorId, "Inherited From artifact should have id {0}, but it has id {1}", parentActorId, actorInheritance.ActorId);
        }
    }
}
