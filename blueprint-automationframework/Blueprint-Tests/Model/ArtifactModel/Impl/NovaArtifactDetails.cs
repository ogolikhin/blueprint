using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Utilities;

namespace Model.ArtifactModel.Impl
{


    public class NovaArtifactDetails:INovaArtifactDetails
    {
        #region Serialized JSON Properties

        public IUser CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        // TODO I will replace PropertyForUpdate with correct implementation if there is change or mismatch of returned JSON structure
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<PropertyForUpdate>>))]
        public List<PropertyForUpdate> CustomPropertyValues { get; set; } = new List<PropertyForUpdate>();
        public string Description { get; set; }
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public int ItemTypeVersionId { get; set; }
        public IUser LastEditedBy { get; set; }
        public DateTime LastEditedOn { get; set; }
        public IUser LockedByUser { get; set; }
        public DateTime? LockedDateTime { get; set; }
        public string Name { get; set; }
        public double OrderIndex { get; set; }
        public int ParentId { get; set; }
        public int Permissions { get; set; }
        public int ProjectId { get; set; }
        // TODO I will replace PropertyForUpdate with correct implementation if there is change or mismatch of returned JSON structure
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        [JsonConverter(typeof(Deserialization.ConcreteConverter<List<PropertyForUpdate>>))]
        public List<PropertyForUpdate> SpecificPropertyValues { get; set; } = new List<PropertyForUpdate>();
        public int Version { get; set; }

        #endregion Serialized JSON Properties

        #region Constructors

        public NovaArtifactDetails() : base()
        {
            //base constructor
        }

        #endregion Constructors

    }
}
