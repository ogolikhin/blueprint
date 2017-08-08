using System.Collections.Generic;
using ServiceLibrary.Models.Reuse;

namespace ServiceLibrary.Models.Workflow
{
    public class ExecutionParameters
    {
        public ItemTypeReuseTemplate ReuseItemTemplate { get; private set; }

        public Dictionary<int, CustomProperties> StandardToCustomPropertyMap { get; private set; }

        public ExecutionParameters(ItemTypeReuseTemplate reuseTemplate,
            Dictionary<int, CustomProperties> standardToCustomPropertyMap)
        {
            ReuseItemTemplate = reuseTemplate;
            StandardToCustomPropertyMap = standardToCustomPropertyMap;
        }
    }
}