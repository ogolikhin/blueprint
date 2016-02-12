using System.Collections.Generic;

namespace Model
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")] // Ignore this warning.
    public interface IProperty
    {

    }

    public interface IOpenApiProperty : IProperty
    {
        int PropertyTypeId { get; set; }
        string Name { get; set; }
        string BasePropertyType { get; set; }
        string TextOrChoiceValue { get; set; }
        bool IsRichText { get; set; }
        bool IsReadOnly { get; set; }
        List<object> UsersAndGroups { get; }
        List<object> Choices { get; }
        string DateValue { get; set; }
        IOpenApiProperty CreatePropertyBasedonDB(IProject project, string propertyName, string propertyValue = null);
    }
}
