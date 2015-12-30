using Model;
using Model.Impl;
using Model.Factories;
using Utilities.Factories;

namespace Helper.Factories
{

    public static class ArtifactFactory
    {

        public static IArtifact CreateArtifact(int projectId = 1, int artifactTypeId = 90, string artifactType = null, string artifactName=null, int parentId = 1, string propertyName = "Description")
        {
            IArtifact artifact = new Artifact();
            if (artifactName == null) { artifact.Name = "REST_Artifact_" + RandomGenerator.RandomAlphaNumeric(5); }
            if (artifactType == null) { artifact.BaseArtifactType = "Actor"; }
            artifact.ParentId = parentId;
            artifact.ProjectId = projectId;
            artifact.ArtifactTypeId = artifactTypeId;
            artifact.SetProperties(PropertyFactory.AddProperty(propertyName));
            return artifact;
        }

    }

}
