using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using ServiceLibrary.Helpers;
using ServiceLibrary.Models;

namespace ArtifactStore.Models
{
    [JsonObject]
    public class PropertyType
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? VersionId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public PropertyPrimitiveType? PrimitiveType { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? InstancePropertyTypeId { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRichText { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? DecimalDefaultValue { get; set; }

        [JsonProperty(PropertyName = "DateDefaultValue", NullValueHandling = NullValueHandling.Ignore)]
        internal string _dateDefaultValue;
        [JsonIgnore]
        public DateTime? DateDefaultValue { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public List<UserGroup> UserGroupDefaultValue { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string StringDefaultValue { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DecimalPlaces { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MaxNumber { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public decimal? MinNumber { get; set; }

        [JsonProperty(PropertyName = "MaxDate", NullValueHandling = NullValueHandling.Ignore)]
        internal string _maxDate;
        [JsonIgnore]
        public DateTime? MaxDate { get; set; }

        [JsonProperty(PropertyName = "MinDate", NullValueHandling = NullValueHandling.Ignore)]
        internal string _minDate;
        [JsonIgnore]
        public DateTime? MinDate { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsMultipleAllowed { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsRequired { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public bool? IsValidated { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227: Collection properties should be read only", Justification = "For JSON serialization, the property sometimes needs to be null")]
        public List<ValidValue> ValidValues { get; set; }

        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? DefaultValidValueId { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly"), OnSerializing]
        internal void OnSerializing(StreamingContext context)
        {
            //Serializing Date Values
            if (DateDefaultValue.HasValue)
                _dateDefaultValue = I18NHelper.DateTimeParseToIso8601Invariant(DateDefaultValue.Value);

            if (MinDate.HasValue)
                _minDate = I18NHelper.DateTimeParseToIso8601Invariant(MinDate.Value);

            if (MaxDate.HasValue)
                _maxDate = I18NHelper.DateTimeParseToIso8601Invariant(MaxDate.Value);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2238:ImplementSerializationMethodsCorrectly"), OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            //Deserializing  Date Values
            DateTime dateValue;

            if (DateTime.TryParse(_dateDefaultValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                DateDefaultValue = dateValue;

            if (DateTime.TryParse(_minDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                MinDate = dateValue;

            if (DateTime.TryParse(_maxDate, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
                MaxDate = dateValue;
        }
    }

    [JsonObject]
    public class ValidValue
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? Id { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }
}