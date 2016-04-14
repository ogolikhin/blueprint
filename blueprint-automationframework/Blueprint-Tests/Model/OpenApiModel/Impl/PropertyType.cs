namespace Model.OpenApiModel.Impl
{
    public class PropertyType
    {
        #region properties
        
        public string BasePropertyType { get; set; }
        public bool IsRichText { get; set; }
        public bool IsMultiLine { get; set; }
        public string DefaultValue { get; set; }
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsRequired { get; set; }
        public bool HasDefaultValue { get; set; }
        public bool IsReadOnly { get; set; }

        #endregion properties

    }
}
