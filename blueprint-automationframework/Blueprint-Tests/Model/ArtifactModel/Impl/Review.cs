using Newtonsoft.Json;
using Model.Common.Enums;
using System.Linq;

namespace Model.ArtifactModel.Impl
{
    public class Review : NovaArtifactDetails
    {
        [JsonIgnore]
        public int ReviewStatus
        {
            get
            {
                return GetSpecificPropertyValue<int>(PropertyTypePredefined.ReviewStatus);
            }
        }

        [JsonIgnore]
        public string ReviewLink
        {
            get
            {
                var specificProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.ReviewLink);
                return specificProperty.CustomPropertyValue.ToString();
            }
        }

        [JsonIgnore]
        public bool IsFormal
        {
            get
            {
                var specificProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.ReviewIsFormal);
                return (bool)specificProperty.CustomPropertyValue;
            }
        }
    }
}
