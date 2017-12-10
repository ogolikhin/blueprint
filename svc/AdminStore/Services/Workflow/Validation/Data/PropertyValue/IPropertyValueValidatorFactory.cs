using System.Collections.Generic;
using ServiceLibrary.Models;
using ServiceLibrary.Models.ProjectMeta;

namespace AdminStore.Services.Workflow.Validation.Data.PropertyValue
{
    public interface IPropertyValueValidatorFactory
    {
        IPropertyValueValidator Create(PropertyType propertyType, IList<SqlUser> users, IList<SqlGroup> groups, bool ignoreIds);
    }
}
