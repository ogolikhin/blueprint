using Model.Common.Enums;

namespace Model.StorytellerModel.Impl
{
    public class PropertyValueInformation
    {
        /// <summary>
        /// The name of the property
        /// </summary>
        public string PropertyName { get; set; }

        /// <summary>
        /// The predefined property type
        /// </summary>
        public PropertyTypePredefined TypePredefined { get; set; }

        /// <summary>
        /// Property Type Id as defined in the blueprint project metadata
        /// </summary>
        public int? TypeId { get; set; }

        /// <summary>
        /// The value of the property
        /// </summary>
        public object Value { get; set; }
    }
}