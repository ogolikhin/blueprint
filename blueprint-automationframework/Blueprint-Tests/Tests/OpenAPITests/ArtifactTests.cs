﻿using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;


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
            _project = ProjectFactory.GetProject(_user);
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
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.GetProperty(_project, "Description", "DescriptionValue"));
            _artifact.SetProperties(properties);

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            var artifact = _artifact.AddArtifact(_artifact, _user);

            Assert.NotNull(artifact.Properties, "Properties should not be null!");

            // TODO more assertion?
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
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.GetProperty(_project, "Description", text));
            _artifact.SetProperties(properties);

            //Set to add in root of the project
            _artifact.ParentId = _artifact.ProjectId;

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.AddArtifact(_artifact, _user);
            // TODO more assertion?
        }
    }
}
