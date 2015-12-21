using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class AProperty : IAProperty
    {
        public int PropertyTypeId { get; set; }
        public string Name { get; set; }
        public string BasePropertyType { get; set; }
        public string TextOrChoiceValue { get; set; }
        public bool IsRichText { get; set; }
        public bool IsReadOnly { get; set; }
        public List<object> UsersAndGroups { get; }
        public List<object> Choices { get; }
        public string DateValue { get; set; }
    }
}
