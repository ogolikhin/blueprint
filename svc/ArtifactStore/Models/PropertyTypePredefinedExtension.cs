namespace ArtifactStore.Models
{
    public static class PropertyTypePredefinedExtension
    {
        public static bool IsSystem(this PropertyTypePredefined propertyTypePredefined)
        {
            return (((int)PropertyTypePredefined.GroupMask & (int)propertyTypePredefined) == (int)PropertyTypePredefined.SystemGroup);
        }

        public static bool IsVisualization(this PropertyTypePredefined propertyTypePredefined)
        {
            return (((int)PropertyTypePredefined.GroupMask & (int)propertyTypePredefined) == (int)PropertyTypePredefined.VisualizationGroup);
        }

        public static bool IsCustom(this PropertyTypePredefined propertyTypePredefined)
        {
            return (((int)PropertyTypePredefined.GroupMask & (int)propertyTypePredefined) == (int)PropertyTypePredefined.CustomGroup);
        }

        public static bool IsFake(this PropertyTypePredefined propertyTypePredefined)
        {
            return ((propertyTypePredefined == PropertyTypePredefined.CreatedBy)
                || (propertyTypePredefined == PropertyTypePredefined.CreatedOn)
                || (propertyTypePredefined == PropertyTypePredefined.LastEditedBy)
                || (propertyTypePredefined == PropertyTypePredefined.LastEditedOn));
        }

        public static bool IsFake(int propertyTypePredefined)
        {
            return ((propertyTypePredefined == (int)PropertyTypePredefined.CreatedBy)
                || (propertyTypePredefined == (int)PropertyTypePredefined.CreatedOn)
                || (propertyTypePredefined == (int)PropertyTypePredefined.LastEditedBy)
                || (propertyTypePredefined == (int)PropertyTypePredefined.LastEditedOn));
        }

        /// <summary>
        ///
        /// </summary>
        public static bool IsVisibleInApi(this PropertyTypePredefined propertyTypePredefined)
        {
            return (((int)PropertyTypePredefined.GroupMask & (int)propertyTypePredefined) == (int)PropertyTypePredefined.CustomGroup)
                        || (propertyTypePredefined == PropertyTypePredefined.ID)
                        || (propertyTypePredefined == PropertyTypePredefined.Name)
                        || (propertyTypePredefined == PropertyTypePredefined.Description)
                        || (propertyTypePredefined == PropertyTypePredefined.UseCaseLevel)
                        || (propertyTypePredefined == PropertyTypePredefined.CreatedBy)
                        || (propertyTypePredefined == PropertyTypePredefined.CreatedOn)
                        || (propertyTypePredefined == PropertyTypePredefined.LastEditedBy)
                        || (propertyTypePredefined == PropertyTypePredefined.LastEditedOn);
        }

        /// <summary>
        ///
        /// </summary>
        public static bool IsAvailableForDefaultTemplateValue(this PropertyTypePredefined propertyTypePredefined)
        {
            return propertyTypePredefined == PropertyTypePredefined.Description;
        }
    }
}