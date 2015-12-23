using System.Collections.Generic;

namespace Model
{
    public interface IProperty
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
    }
}
