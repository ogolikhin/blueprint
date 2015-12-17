using Model;
using Helper.Factories;
using TestConfig;
using NUnit.Framework;
using CustomAttributes;

namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenAPI)]
    public class ArtifactTests
    {
        private static TestConfiguration _testConfig = TestConfiguration.GetInstance();
        private IUser _user = null;
        private IArtifactStore _artifactStore = ArtifactStoreFactory.CreateArtifactStore("http://bpakvmsys08:8080/");

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        [Test]
        public void ArtifactCreation()
        {
            ArtifactCreation(1, 90);
        }

        /// <summary>
        /// Create an artifact under the existing project .
        /// </summary>
        /// <param name="projectId">(Optional) The ID of the existing project Id.</param>
        /// <param name="artifactTypeId">(Optional) The ID of artifactTypeId</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        private void ArtifactCreation(int projectId = 0, int artifactTypeId = 0)
        {
            IArtifact artifact = ArtifactFactory.CreateArtifact(projectId, artifactTypeId);
            _artifactStore.AddArtifact(artifact,_user);
        }
    }
}
