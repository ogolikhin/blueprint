using System.Collections.Generic;

namespace ServiceLibrary.Models.Collection
{
    public class ArtifactsOfCollectionDto
    {
        public int ItemsCount { get; set; }

        public IEnumerable<ArtifactDto> Items { get; set; }

        public SettingsDto Settings { get; set; }
    }
}