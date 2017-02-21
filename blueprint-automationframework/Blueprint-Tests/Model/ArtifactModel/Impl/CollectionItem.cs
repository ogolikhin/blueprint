using System;
using Model.ArtifactModel.Enums;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class CollectionItem
    {
        public DateTime? CreatedOn { get; set; } //now this property is always null for Collection
        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ItemTypeId { get; set; }

        public ItemTypePredefined ItemTypePredefined { get; set; }

        public string Prefix { get; set; }

        public List<string> ArtifactPath { get; } = new List<string>();
    }
}
