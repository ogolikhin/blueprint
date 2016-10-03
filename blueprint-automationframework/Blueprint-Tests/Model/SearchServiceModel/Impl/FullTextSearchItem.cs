using System;

namespace Model.SearchServiceModel.Impl
{
    public class FullTextSearchItem
    {
        private DateTime? _createdDateTime;
        private DateTime? _lastModifiedDateTime;

        /// <summary>
        /// The project id for the search result item
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// The artifact id of the search result item
        /// </summary>
        public int ArtifactId { get; set; }

        /// <summary>
        /// The item id of the search result item
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// The name of the search result item
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The property name of the search result item
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The text of the property where the search criteria matched
        /// </summary>
        public string SearchableValue { get; set; }

        /// <summary>
        /// The property type id of the property where the search criteria matched
        /// </summary>
        public int PropertyTypeId { get; set; }

        /// <summary>
        /// The sub artifact id where the search criteria matched, if it exists
        /// </summary>
        public int? SubartifactId => ItemId != ArtifactId ? ItemId : default(int?);

        /// <summary>
        /// A flag indicating if the item where the search criteria matched was a sub artifact
        /// </summary>
        public bool IsSubartifact => SubartifactId.HasValue;

        /// <summary>
        /// The item type id
        /// </summary>
        public int ItemTypeId { get; set; }

        /// <summary>
        /// The item type name
        /// </summary>
        public string TypeName { get; set; }

        /// <summary>
        /// The item type prefix
        /// </summary>
        public string TypePrefix { get; set; }

        /// <summary>
        /// The id of the user that created the returned item
        /// </summary>
        public int? CreatedUser { get; set; }

        /// <summary>
        /// The name of the user that created the returned item
        /// </summary>
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date & time the returned item was created
        /// </summary>
        public DateTime? CreatedDateTime
        {
            get { return _createdDateTime; }
            set
            {
                _createdDateTime = value != null
                    ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc)
                    : default(DateTime?);
            }
        }

        /// <summary>
        /// The id of the user that last modified the returned item
        /// </summary>
        public int? LastModifiedUser { get; set; }

        /// <summary>
        /// The name of the user that last modified the returned item
        /// </summary>
        public string LastModifiedBy { get; set; }  

        /// <summary>
        /// The date & time that the returned item was last modified
        /// </summary>
        public DateTime? LastModifiedDateTime
        {
            get { return _lastModifiedDateTime; }
            set
            {
                _lastModifiedDateTime = value != null
                    ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc) 
                    : default(DateTime?);
            }
        }
    }
}