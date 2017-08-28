namespace ServiceLibrary.Models.PropertyType
{
    public class DNumberPropertyType : DPropertyType
    {
        public bool IsValidate { get; set; }
        /// <summary>
        ///
        /// </summary>
        public Range<decimal> Range { get; set; }

        /// <summary>
        ///
        /// </summary>
        public int DecimalPlaces { get; set; }

        /// <summary>
        ///
        /// </summary>
        public decimal? DefaultValue { get; set; }
    }
}
