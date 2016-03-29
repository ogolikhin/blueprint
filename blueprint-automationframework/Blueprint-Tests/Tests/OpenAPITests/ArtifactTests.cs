using CustomAttributes;
using Model;
using Model.Factories;
using Model.OpenApiModel;
using Model.OpenApiModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;


namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class ArtifactTests
    {
        private IUser _user;
        private IProject _project;
        private IOpenApiArtifact _artifact;
        private IAdminStore _adminStore;
        private ISession _session;

        [SetUp]
        public void SetUp()
        {
            _adminStore = AdminStoreFactory.GetAdminStoreFromTestConfig();
            _user = UserFactory.CreateUserAndAddToDatabase();
            _project = ProjectFactory.GetProject(_user);
            _session = _adminStore.AddSession(_user.Username, _user.Password);
            _user.SetToken(_session.SessionId);
            Assert.IsFalse(string.IsNullOrWhiteSpace(_user.Token.AccessControlToken), "The user didn't get an Access Control token!");
        }

        [TearDown]
        public void TearDown()
        {
            if (_user != null)
            {
                // TODO Add teardown to remove published artifact(s) before deleteting users
                _user.DeleteUser(deleteFromDatabase: true);
                _user = null;
            }
        }

        /// <summary>
        /// Create an "Actor" type artifact under the existing project.
        /// </summary>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        [Test]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void AddArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty();
            _artifact.Properties.Add(property.GetProperty(_project, "Description", "DescriptionValue"));

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
             _artifact.Save();

            Assert.NotNull(_artifact.Properties, "Properties should not be null!");

            // TODO more assertion?
        }

        [Test]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void DiscardArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty();
            _artifact.Properties.Add(property.GetProperty(_project, "Description", "DescriptionValue"));

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //Add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.Save();

            //Adding all artifact(s) to publish
            List<IOpenApiArtifact> artifactList = new List<IOpenApiArtifact>();
            artifactList.Add(_artifact);
            // TODO more assertion?

            //Discard the artifact     
            _artifact.Discard();
        }

        [Test]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        public void PublishArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            IOpenApiProperty property = new OpenApiProperty();
            _artifact.Properties.Add(property.GetProperty(_project, "Description", "DescriptionValue"));

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //Add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.Save();

            // TODO more assertion?

            //Publish artifact     
            _artifact.Publish();
        }

        /// <summary>
        /// Create an "Actor" type artifact under the existing project, adding RTF value in RTF supported property field
        /// </summary>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_01.txt")]
        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_02.txt")]
        [Explicit(IgnoreReasons.UnderDevelopment)]

        public void AddArtifact_Actor_With(string sampleTextPath)
        {
            var text = System.IO.File.ReadAllText(sampleTextPath);
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = ArtifactFactory.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //create Description property
            IOpenApiProperty property = new OpenApiProperty();
            _artifact.Properties.Add(property.GetProperty(_project, "Description", text));

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.Save();

            // TODO more assertion?
        }
    }
}
