using System.Collections.Generic;
using System.Data;
using ServiceLibrary.Helpers.Validators;
using ServiceLibrary.Models;
using ServiceLibrary.Models.PropertyType;
using ServiceLibrary.Models.Reuse;

namespace BluePrintSys.Messaging.CrossCutting.Models.Interfaces
{
    public interface IExecutionParameters
    {
        ItemTypeReuseTemplate ReuseItemTemplate { get; }
        List<DPropertyType> CustomPropertyTypes { get; }
        IDbTransaction Transaction { get; }
        IReadOnlyList<IPropertyValidator> Validators { get; }
        IReusePropertyValidator ReuseValidator { get; }
        IValidationContext ValidationContext { get; }
        int UserId { get; }
    }
}
