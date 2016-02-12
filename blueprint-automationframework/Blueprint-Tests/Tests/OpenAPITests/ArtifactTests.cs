using Model;
using NUnit.Framework;
using CustomAttributes;
using Model.Factories;

namespace OpenAPITests
{

    [TestFixture]
    [Category(Categories.OpenApi)]
    public class ArtifactTests
    {
        private IUser _user = null;
        private IProject _project = null;
        private IOpenApiArtifact _artifact = null;

        [SetUp]
        public void SetUp()
        {
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject();
            _artifact = ArtifactFactory.CreateArtifactFromTestConfig();
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
        [Explicit(IgnoreReasons.UnderDevelopment)]
        /// <summary>
        /// Create an "Actor" type artifact under the existing project .
        /// </summary>
        /// <param name="_project">the existing project</param>
        /// <param name="_artifact">artifact object that contain artifactType information belong to property</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        public void AddArtifact_Actor()
        {
            //Set artifact Type with an actor type
            _artifact.ArtifactTypeName = "Actor";
            //update the artifact object for the target project 
            _artifact = _artifact.UpdateArtifact(_project, _artifact, "Description", "DescriptionValue");
            //add the created artifact object into BP using OpenAPI call
            _artifact.AddArtifact(_artifact, _user);
        }
    }
}
