using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models.PropertyType
{
    //TODO: remove properties from this class as we create the new ones
    public class DPropertyType
    {
        public int PropertyTypeId { get; set; }
        public int? VersionId { get; set; }
        public string Name { get; set; }
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        public PropertyTypePredefined Predefined { get; set; }
        public int? InstancePropertyTypeId { get; set; }
        public bool? IsRichText { get; set; }
        //public decimal? DecimalDefaultValue { get; set; }
        //public DateTime? DateDefaultValue { get; set; }
        //public List<UserGroup> UserGroupDefaultValue { get; set; }
        public string StringDefaultValue { get; set; }
        //public int? DecimalPlaces { get; set; }
        //public decimal? NumericDefaultValue { get; set; }
        //public decimal? NumberRange_End { get; set; }
        //public decimal? NumberRange_Start { get; set; }
        //public DateTime? DateRange_End { get; set; }
        //public DateTime? DateRange_Start { get; set; }
        public bool? AllowMultiple { get; set; }
        public bool IsRequired { get; set; }
        public bool? Validate { get; set; }
        //public List<ValidValue> ValidValues { get; set; }
        public int? DefaultValidValueId { get; set; }
    }
}
