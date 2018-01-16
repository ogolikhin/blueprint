using System.Collections.Generic;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactsOfCollection
    {
        public int ItemsCount { get; set; }

        public IEnumerable<ArtifactDto> Items { get; set; }

        public Settings Settings { get; set; }
    }
}