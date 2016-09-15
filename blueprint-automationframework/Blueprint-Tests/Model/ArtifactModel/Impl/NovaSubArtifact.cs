using System;
using System.Collections.Generic;
using Utilities;
using Newtonsoft.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.ArtifactModel.Impl
{
    public class NovaSubArtifact : INovaSubArtifact
    {
        #region Serialized JSON Properties
        public int Id { get; set; }

        public int ParentId { get; set; }

        public int ItemTypeId { get; set; }

        public string DisplayName { get; set; }

        public int PredefinedType { get; set; }

        public bool HasChildren { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteListConverter<INovaSubArtifact, NovaSubArtifact>))]
        public List<INovaSubArtifact> Children { get; set; } = new List<INovaSubArtifact>();

        #endregion Serialized JSON Properties
    }
}
