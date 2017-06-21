namespace ServiceLibrary.Models.VersionControl
{
    public class SqlDiscardPublishDetails
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
