namespace Model.SearchServiceModel.Impl
{
    public class FullTextSearchTypeItem
    {
        /// <summary>
        /// The item type id
        /// </summary>
        public int ItemTypeId { get; set; }

        /// <summary>
        /// The item type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The number of hits for the search criteria within the scope of the search
        /// </summary>
        public int Count { get; set; }
    }
}