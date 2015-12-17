using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Model;
using Model.Impl;
using Model.Factories;
using TestConfig;
using Logging;

namespace Helper.Factories
{
    public static class ArtifactFactory
    {
        public static IArtifact CreateArtifact(int projectId = 0, int artifactTypeId = 0, string artifactType = null, string artifactName=null, int parentId = 0, string propertyName = "Description")
        {
            IArtifact _artifact = new Artifact();
            if (projectId == 0) { _artifact.ProjectId = 1; }
            if (artifactTypeId == 0) { _artifact.ArtifactTypeId = 90; }
            if (artifactName == null) { _artifact.Name = "REST_Artifact_" + RandomGenerator.RandomAlphaNumeric(5); }
            if (parentId == 0) { _artifact.ParentId = 1; }
            if (artifactType == null)
            {
                _artifact.BaseArtifactType = "Actor";
            }
            _artifact.ProjectId = projectId;
            _artifact.ArtifactTypeId = artifactTypeId;
            _artifact.Properties = PropertyFactory.AddProperty(propertyName);
            return _artifact;
        }
    }
}
