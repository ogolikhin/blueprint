using System;
using System.Collections.Generic;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public class PropertyValueValidatorFactory : IPropertyValueValidatorFactory
    {
        public IPropertyValueValidator Create(PropertyType propertyType, IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds)
        {
            if (propertyType == null)
            {
                throw new ArgumentNullException(nameof(propertyType));
            }

            switch (propertyType.PrimitiveType)
            {
                case PropertyPrimitiveType.Text:
                    return new TextPropertyValueValidator();

                case PropertyPrimitiveType.Number:
                    return new NumberPropertyValueValidator();

                case PropertyPrimitiveType.Date:
                    return new DatePropertyValueValidator();

                case PropertyPrimitiveType.Choice:
                    return new ChoicePropepertyValueValidator(ignoreIds);

                case PropertyPrimitiveType.User:
                    return new UserPropertyValueValidator(users, groups, ignoreIds);

                default:
                    throw new ArgumentOutOfRangeException(nameof(propertyType.PrimitiveType));
            }
        }
    }
}
