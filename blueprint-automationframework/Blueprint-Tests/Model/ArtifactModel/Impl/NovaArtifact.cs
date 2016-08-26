using System;

namespace Model.ArtifactModel.Impl
{
    public class NovaArtifact : INovaArtifact
    {

        #region Serialized JSON Properties
            
        public bool HasChildren { get; set; }
        public string Description { get; set; }
        public int Id { get; set; }
        public int ItemTypeId { get; set; }
        public IUser LockedByUser { get; set; }
        public DateTime LockedDateTime { get; set; }
        public string Name { get; set; }
        public int OrderIndex { get; set; }
        public int ParentId { get; set; }
        public int Permissions { get; set; }
        public int PredefinedType { get; set; }
        public string Prefix { get; set; }
        public int ProjectId { get; set; }
        public int Version { get; set; }

        #endregion Serialized JSON Properties

        #region Constructors

        public NovaArtifact() : base()
        {
            //base constructor
        }

        #endregion Constructors
        }
    }
