using System.Collections.Generic;
using ServiceLibrary.Models.PropertyType;

namespace ServiceLibrary.Helpers.Validators
{

    public interface IPropertyValidator
    {
        PropertySetResult Validate(PropertyLite property, List<DPropertyType> propertyTypes);
    }
}
