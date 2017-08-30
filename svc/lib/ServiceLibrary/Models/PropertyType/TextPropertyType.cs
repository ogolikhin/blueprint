using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Models.PropertyType
{
    public class TextPropertyType : WorkflowPropertyType
    {
        public bool IsValidate { get; set; }
        public string DefaultValue { get; set; }
    }
}
