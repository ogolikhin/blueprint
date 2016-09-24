using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class PropertyTypeBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool? IsRichText { get; set; }
        public bool? IsRequired { get; set; }
    }

    public class OpenApiPropertyType : PropertyTypeBase
    {
        public string BasePropertyType { get; set; }
        public bool IsMultiLine { get; set; }
        public string DefaultValue { get; set; }
        public bool HasDefaultValue { get; set; }
        public bool IsReadOnly { get; set; }
    }

    public class NovaPropertyType : PropertyTypeBase
    {
        public class UserGroup
        {
            public int Id { get; set; }
            public bool IsGroup { get; set; }
        }

        public class ValidValue
        {
            public int Id { get; set; }
            public string Value { get; set; }
        }

        public int? VersionId { get; set; }
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        public int? InstancePropertyTypeId { get; set; }
        public decimal? DecimalDefaultValue { get; set; }
        public DateTime? DateDefaultValue { get; set; }
        public string StringDefaultValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public decimal? MaxNumber { get; set; }
        public decimal? MinNumber { get; set; }
        public DateTime? MaxDate { get; set; }
        public DateTime? MinDate { get; set; }
        public bool? IsMultipleAllowed { get; set; }
        public bool? IsValidated { get; set; }
        public int? DefaultValidValueIndex { get; set; }
        public List<ValidValue> ValidValues { get; } = new List<ValidValue>();
        public List<UserGroup> UserGroupDefaultValue { get; } = new List<UserGroup>();
    }

    public enum PropertyPrimitiveType
    {
        // These are the names & values defined in the production code.
        Text = 0,
        Number = 1,
        Date = 2,
        User = 3,
        Choice = 4,
        Image = 5
    }
}
