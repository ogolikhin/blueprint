namespace ServiceLibrary.Models.VersionControl
{
    public class SqlDiscardPublishDetails
    {
        public int ItemId { get; set; }

        public int RevisionId { get; set; }

        public bool AddDrafts { get; set; }

        public ItemTypePredefined PrimitiveItemTypePredefined { get; set; }

        public int VersionProjectId { get; set; }

        public int? ParentId { get; set; }

        public double? OrderIndex { get; set; }

        public string Name { get; set; }

        public int ItemType_ItemTypeId { get; set; }

        public string Prefix { get; set; }

        public int? Icon_ImageId { get; set; }

        public int VersionsCount { get; set; }
    }

    public class DiscardPublishDetails
    {
        public int ItemId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public ItemTypePredefined BaseItemTypePredefined
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int ProjectId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int? ParentId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public double? OrderIndex
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public string Name
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int ItemTypeId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public string ItemTypePrefix
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int? ItemTypeIconId
        {
            get;
            internal set;
        }

        /// <summary>
        ///
        /// </summary>
        public int VersionsCount
        {
            get;
            internal set;
        }
    }
}
