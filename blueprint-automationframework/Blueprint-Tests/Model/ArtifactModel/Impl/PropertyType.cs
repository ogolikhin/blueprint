using System;
using System.Collections.Generic;

namespace Model.ArtifactModel.Impl
{
    public class PropertyTypeBase
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRichText { get; set; }
        public bool IsRequired { get; set; }
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

        public int VersionId { get; set; }
        public int PrimitiveType { get; set; }
        public int InstancePropertyTypeId { get; set; }
        public int DecimalDefaultValue { get; set; }
        public DateTime DateDefaultValue { get; set; }
        public string StringDefaultValue { get; set; }
        public int DecimalPlaces { get; set; }
        public int MaxNumber { get; set; }
        public int MinNumber { get; set; }
        public DateTime MaxDate { get; set; }
        public DateTime MinDate { get; set; }
        public bool IsMultipleAllowed { get; set; }
        public bool IsValidated { get; set; }
        public int DefaultValidValueIndex { get; set; }
        public List<string> ValidValues { get; } = new List<string>();
        public List<UserGroup> UserGroupDefaultValue { get; } = new List<UserGroup>();
    }
}
