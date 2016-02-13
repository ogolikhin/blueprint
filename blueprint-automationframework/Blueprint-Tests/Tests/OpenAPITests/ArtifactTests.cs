using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using NUnit.Framework;
using System.Globalization;
using System.IO;
using System.Net;

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
        /// Create an "Actor" type artifact under the existing project.
        /// </summary>
        /// <param name="_project">the existing project</param>
        /// <param name="_artifact">artifact object that contain artifactType information belong to property</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        public void AddArtifact_Actor()
        {
            //Set artifact Type with an actor type
            _artifact.ArtifactTypeName = "Actor";

            //update the artifact object for the target project 
            _artifact.UpdateArtifact(_project, _artifact, "Description", "DescriptionValue");

            //add the created artifact object into BP using OpenAPI call
            var artifactResult = _artifact.AddArtifact(_artifact, _user);

            //TODO Assertion to check ResultCode
            Assert.That(artifactResult.Message == "Success", I18NHelper.FormatInvariant("The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, "Success"));
            
            //Assertion to check ResultCode
            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture), I18NHelper.FormatInvariant("The returned ResultCode was '{0}' but '{1}' was expected", artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture)));
        }

        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_01.txt")]
        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_02.txt")]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void AddArtifact_Actor_With(string sampleTextPath)
        {
            var text = File.ReadAllText(sampleTextPath);
            //Set artifact Type with an actor type
            _artifact.ArtifactTypeName = "Actor";

            //update the artifact object for the target project 
            _artifact.UpdateArtifact(_project, _artifact, "Description", text);

            //add the created artifact object into BP using OpenAPI call
            IOpenApiArtifactResult artifactResult = _artifact.AddArtifact(_artifact, _user);

            //TODO Assertion to check ResultCode
            Assert.That(artifactResult.Message == "Success", I18NHelper.FormatInvariant("The returned Message was '{0}' but '{1}' was expected", artifactResult.Message, "Success"));
            
            //Assertion to check ResultCode
            Assert.That(artifactResult.ResultCode == ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture), I18NHelper.FormatInvariant("The returned ResultCode was '{0}' but '{1}' was expected", artifactResult.ResultCode, ((int)HttpStatusCode.Created).ToString(CultureInfo.InvariantCulture)));
        }
    }
}
