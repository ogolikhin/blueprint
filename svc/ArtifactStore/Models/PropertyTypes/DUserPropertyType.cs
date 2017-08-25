using ServiceLibrary.Models.PropertyType;

namespace ArtifactStore.Models.PropertyTypes
{
    public class DUserPropertyType: DPropertyType
    {
        public string DefaultLabels { get; set; }
        public string DefaultValues { get; set; }
    }
}