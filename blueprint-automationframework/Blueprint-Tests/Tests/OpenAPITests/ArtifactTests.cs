using CustomAttributes;
using Model;
using Model.Factories;
using Model.ArtifactModel;
using Model.ArtifactModel.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using Helper;
using TestCommon;


namespace OpenAPITests
{
    [TestFixture]
    [Category(Categories.OpenApi)]
    public class ArtifactTests : TestBase
    {
        private IUser _user;
        private IProject _project;
        private IOpenApiArtifact _artifact;

        #region Setup and Cleanup

        [TestFixtureSetUp]
        public void SetUp()
        {
            Helper = new TestHelper();
            _user = Helper.CreateUserAndAuthenticate(TestHelper.AuthenticationTokenTypes.OpenApiToken);
            _project = ProjectFactory.GetProject(_user, shouldRetrievePropertyTypes: true);
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Helper?.Dispose();
        }
        
        #endregion Setup and Cleanup

        /// <summary>
        /// Create an "Actor" type artifact under the existing project.
        /// </summary>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // Needs more asserts.
        public void AddArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = Helper.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            OpenApiProperty property = new OpenApiProperty(_artifact.Address);
            
            //Set property value
            _artifact.Properties.Add(
                property.SetPropertyAttribute(
                    _project,
                    _user,
                    baseArtifactType: BaseArtifactType.Actor,
                    propertyName: nameof(NovaArtifactDetails.Description),
                    propertyValue: "Testing Set Property Value"
                    )
                );

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
             _artifact.Save();

            Assert.NotNull(_artifact.Properties, "Properties should not be null!");

            // TODO more assertion?
        }

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // Needs more asserts.
        public void DiscardArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = Helper.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            OpenApiProperty property = new OpenApiProperty(_artifact.Address);
            _artifact.Properties.Add(
                property.SetPropertyAttribute(
                    _project,
                    _user,
                    baseArtifactType: BaseArtifactType.Actor,
                    propertyName: nameof(NovaArtifactDetails.Description),
                    propertyValue: "DescriptionValue"
                    )
                );

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

        [TestCase]
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // Needs more asserts.
        public void PublishArtifact_Actor()
        {
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = Helper.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //Create Description property
            OpenApiProperty property = new OpenApiProperty(_artifact.Address);
            _artifact.Properties.Add(
                property.SetPropertyAttribute(
                    _project,
                    _user,
                    baseArtifactType: BaseArtifactType.Actor,
                    propertyName: nameof(NovaArtifactDetails.Description),
                    propertyValue: "DescriptionValue"
                    )
                );

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
        [Explicit(IgnoreReasons.UnderDevelopmentQaDev)] // Needs more asserts and fix input data.
        public void AddArtifact_Actor_With(string sampleTextPath)
        {
            var text = System.IO.File.ReadAllText(sampleTextPath);
            //Create an artifact with ArtifactType and populate all required values without properties
            _artifact = Helper.CreateOpenApiArtifact(project: _project, user: _user, artifactType: BaseArtifactType.Actor);

            //create Description property
            OpenApiProperty property = new OpenApiProperty(_artifact.Address);
            _artifact.Properties.Add(
                property.SetPropertyAttribute(
                    _project,
                    _user,
                    baseArtifactType: BaseArtifactType.Actor,
                    propertyName: nameof(NovaArtifactDetails.Description),
                    propertyValue: text
                    )
                );

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.Save();

            // TODO more assertion?
        }
    }
}
