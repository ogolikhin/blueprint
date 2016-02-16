﻿using Common;
using CustomAttributes;
using Model;
using Model.Factories;
using Model.Impl;
using NUnit.Framework;
using System.Collections.Generic;
using System.Globalization;
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

            //Update artifact with artifactTypeId and artifactTypeName based on the target project
            _artifact.UpdateArtifactType(_project.Id, _artifact);

            //create Description property
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.GetProperty(_project, "Description", "DescriptionValue"));

            //update the artifact attributes 
            _artifact.UpdateArtifactAttributes(_artifact, properties);

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.AddArtifact(_artifact, _user);
            // TODO more assertion?
        }

        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_01.txt")]
        [TestCase("C:/Users/akim/Documents/GitHub/blueprint-automationframework/Blueprint-Tests/Tests/OpenAPITests/Files/testHTML_supported_02.txt")]
        [Explicit(IgnoreReasons.UnderDevelopment)]
        /// <summary>
        /// Create an "Actor" type artifact under the existing project, adding RTF value in RTF supported property field
        /// </summary>
        /// <param name="_project">the existing project</param>
        /// <param name="_artifact">artifact object that contain artifactType information belong to property</param>
        /// <exception cref="NUnit.Framework.AssertionException">If the response contains other than 201 status code.</exception>
        public void AddArtifact_Actor_With(string sampleTextPath)
        {
            var text = System.IO.File.ReadAllText(sampleTextPath);
            //Set artifact Type with an actor type
            _artifact.ArtifactTypeName = "Actor";

            //Update artifact with artifactTypeId and artifactTypeName based on the target project
            _artifact.UpdateArtifactType(_project.Id, _artifact);

            //create Description property
            List<IOpenApiProperty> properties = new List<IOpenApiProperty>();
            IOpenApiProperty property = new OpenApiProperty();
            properties.Add(property.GetProperty(_project, "Description", text));

            //update the artifact attributes  
            _artifact.UpdateArtifactAttributes(_artifact, properties);

            //add the created artifact object into BP using OpenAPI call - assertions are inside of AddArtifact
            _artifact.AddArtifact(_artifact, _user);
            // TODO more assertion?
        }
    }
}
