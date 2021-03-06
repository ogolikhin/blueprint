﻿using System;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models.PropertyType
{
    public class SqlPropertyType
    {
        public int ItemTypeId { get; set; }
        public int PropertyTypeId { get; set; }
        public int? VersionId { get; set; }
        public string Name { get; set; }
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        public PropertyTypePredefined Predefined { get; set; }
        public int? InstancePropertyTypeId { get; set; }
        public bool? IsRichText { get; set; }
        public object DecimalDefaultValue { get; set; }
        public DateTime? DateDefaultValue { get; set; }
        public string StringDefaultValue { get; set; }
        public int? DecimalPlaces { get; set; }
        public object NumericDefaultValue { get; set; }
        public object NumberRange_End { get; set; }
        public object NumberRange_Start { get; set; }
        public DateTime? DateRange_End { get; set; }
        public DateTime? DateRange_Start { get; set; }
        public bool? AllowMultiple { get; set; }
        public bool? Required { get; set; }
        public bool? Validate { get; set; }
        public int? DefaultValidValueId { get; set; }
        public string UserDefaultValue { get; set; }
        public string UserDefaultLabel { get; set; }
        public string CustomProperty { get; set; }
    }

    public class SqlPropertyInfo
    {
        public int PropertyTypeId { get; set; }

        public int PrimitiveType { get; set; }

        public string PropertyName { get; set; }

        public int? InstancePropertyTypeId { get; set; }

        public string PropertyValue { get; set; }

        public int? PropertyValuePrimitiveType { get; set; }

        public string StringValue { get; set; }

        public string StringDefaultValue { get; set; }

        public string UserValue { get; set; }

        public string UserDefaultValue { get; set; }
    }
}