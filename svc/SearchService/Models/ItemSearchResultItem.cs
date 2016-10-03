using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SearchService.Models
{
    public class ItemSearchResultItem
    {
        public int ProjectId { get; set; }

        public int ArtifactId { get; set; }

        public int ItemId { get; set; }

        public string Name { get; set; }

        //public string PropertyName { get; set; }

        //public string SearchableValue { get; set; }

        //public int PropertyTypeId { get; set; }

        //public int? SubartifactId => ItemId != ArtifactId ? ItemId : default(int?);

        //public bool IsSubartifact => SubartifactId.HasValue;

        public int ItemTypeId { get; set; }

        public string TypeName { get; set; }

        public string TypePrefix { get; set; }

        //public int? CreatedUser { get; set; }

        //public string CreatedBy { get; set; }

        //private DateTime? _createdDateTime;
        //public DateTime? CreatedDateTime
        //{
        //    get { return _createdDateTime; }
        //    set
        //    {
        //        _createdDateTime = value != null
        //            ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc)
        //            : default(DateTime?);
        //    }
        //}

        //public int? LastModifiedUser { get; set; }

        //public string LastModifiedBy { get; set; }

        //private DateTime? _lastModifiedDateTime;
        //public DateTime? LastModifiedDateTime
        //{
        //    get { return _lastModifiedDateTime; }
        //    set
        //    {
        //        _lastModifiedDateTime = value != null
        //            ? DateTime.SpecifyKind((DateTime)value, DateTimeKind.Utc)
        //            : default(DateTime?);
        //    }
        //}
    }
}