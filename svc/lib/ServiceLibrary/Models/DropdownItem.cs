namespace ServiceLibrary.Models
{
    public class DropdownItem
    {
        public string Label { get; set; }
        public object Value { get; set; }

        public DropdownItem(string label, object value)
        {
            Label = label;
            Value = value;
        }
    }
}
