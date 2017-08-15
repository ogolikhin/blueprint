using System.Collections.Generic;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;

namespace ServiceLibrary.Models.Workflow
{
    public class ExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; private set; }

        public List<DPropertyType> InstancePropertyTypes { get; private set; }
        
        public IPropertyValidator[] Validators;

        public ExecutionParameters(
            ItemTypeReuseTemplate reuseTemplate,
            List<DPropertyType> instancePropertyTypes)
        {
            ReuseItemTemplate = reuseTemplate;
            InstancePropertyTypes = instancePropertyTypes;
            Validators  = new IPropertyValidator[]
            {
                new NumberPropertyValidator()
            }; 
        }
    }
}