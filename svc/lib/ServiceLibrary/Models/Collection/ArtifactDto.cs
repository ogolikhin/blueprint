using System.Collections.Generic;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactDto
    {
        public int ArtifactId { get; set; }

        public IEnumerable<PropertyInfoDto> PropertyInfos { get; set; }
    }
}