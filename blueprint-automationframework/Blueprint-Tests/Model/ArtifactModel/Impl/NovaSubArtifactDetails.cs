using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Utilities;
using Newtonsoft.Json;

namespace Model.ArtifactModel.Impl
{
    public class NovaSubArtifactDetails
    {
        #region Serialized JSON Properties

        public bool IsDeleted { get; set; }

        public int Id { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int ParentId { get; set; }

        public double OrderIndex { get; set; }

        public int ItemTypeId { get; set; }

        public int ItemTypeVersionId { get; set; }

        public string Prefix { get; set; }

        public List<SubArtifactCustomProperty> CustomPropertyValues { get; } = new List<SubArtifactCustomProperty>();

        public List<SubArtifactCustomProperty> SpecificPropertyValues { get; } = new List<SubArtifactCustomProperty>();

        public int PredefinedType { get; set; }

        #endregion Serialized JSON Properties

        public class SubArtifactCustomProperty
        {
            public string Name { get; set; }

            public int PropertyTypeId { get; set; }

            public int? PropertyTypeVersionId { get; set; }

            public int PropertyTypePredefined { get; set; }

            public object Value { get; set; }

        }
    }
}
