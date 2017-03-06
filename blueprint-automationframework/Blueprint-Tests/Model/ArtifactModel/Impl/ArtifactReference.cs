using Model.ArtifactModel.Enums;
using NUnit.Framework;

namespace Model.ArtifactModel.Impl
{
    // Found in: blueprint-current/Source/BluePrintSys.RC.Service.Business/Repository/Models/Storyteller/ArtifactReference.cs
    public class ArtifactReference
    {
        /// <summary>
        /// The Id of the artifact
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The project Id for the artifact
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The name of the artifact
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type prefix for the artifact
        /// </summary>
        public string TypePrefix { get; set; }

        /// <summary>
        /// The name of the project
        /// </summary>
        public string ProjectName { get; set; }

        /// <summary>
        /// The base item type for the artifact
        /// </summary>
        public ItemTypePredefined BaseItemTypePredefined { get; set; }

        /// <summary>
        /// The version of the artifact
        /// </summary>
        public int? Version { get; set; }        
        
        /// <summary>
        /// The link to navigate to the artifact
        /// </summary>
        public string Link { get; set; }

        /// <summary>
        /// Assert that Artifact References are equal
        /// </summary>
        /// <param name="artifactReference1">The first artifact reference</param>
        /// <param name="artifactReference2">The artifact reference being compared to the first</param>
        /// <param name="doDeepCompare">If false, only compare Ids, else compare all properties</param>
        public static void AssertAreEqual(ArtifactReference artifactReference1, ArtifactReference artifactReference2, bool doDeepCompare = true)
        {
            if ((artifactReference1 == null) || (artifactReference2 == null))
            {
                Assert.That((artifactReference1 == null) && (artifactReference2 == null), "One of the artifact references is null while the other is not null");
            }
            else
            {
                Assert.AreEqual(artifactReference1.Id, artifactReference2.Id, "Artifact references ids do not match");

                if (doDeepCompare)
                {
                    Assert.AreEqual(artifactReference1.BaseItemTypePredefined, artifactReference2.BaseItemTypePredefined,
                        "Artifact reference base item types do not match");
                    Assert.AreEqual(artifactReference1.Link, artifactReference2.Link, "Artifact reference links do not match");
                    Assert.AreEqual(artifactReference1.Name, artifactReference2.Name, "Artifact reference names do not match");
                    Assert.AreEqual(artifactReference1.ProjectId, artifactReference2.ProjectId, "Artifact reference project ids do not match");
                    Assert.AreEqual(artifactReference1.TypePrefix, artifactReference2.TypePrefix, "Artifact reference type prefixes do not match");
                }
            }
        }
    }
}
