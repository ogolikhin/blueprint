using System;

namespace ServiceLibrary.Models.PropertyType
{
    public class DatePropertyType : WorkflowPropertyType
    {
        public bool IsValidate { get; set; }
        public Range<DateTime?> Range { get; set; }
        public DateTime? DefaultValue { get; set; }
    }
}
