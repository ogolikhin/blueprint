
using ServiceLibrary.Models.ProjectMeta;
using System.Collections.Generic;

namespace ServiceLibrary.Models.PropertyType
{
    public class ChoicePropertyType : DPropertyType
    {
        /// <summary>
        ///
        /// </summary>
        public bool IsValidate { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool AllowsCustomValue()
        {
            if (AllowMultiple.HasValue) return false;
            return !IsValidate;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsMultiChoice()
        {
            return AllowMultiple.HasValue ? AllowMultiple.Value : false;
        }

        /// <summary>
        ///
        /// </summary>
        public bool IsSingleChoice()
        {
            return AllowMultiple.HasValue ? !AllowMultiple.Value : true;
        }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        public List<ValidValue> ValidValues { get; set; }
    }
}