using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IAProperty
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
