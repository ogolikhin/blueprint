using System;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Linq;
using Model.Common.Enums;

namespace Model.ArtifactModel.Impl
{
    public class Actor : NovaArtifactDetails
    {

        /// <summary>
        /// Returns ActorInheritanceValue. It represents information from Inherited from field for Actor.
        /// </summary>
        /// <exception cref="FormatException">Throws FormatException if ActorInheritanceValue doesn't correspond to server JSON.</exception>
        [JsonIgnore]
        public ActorInheritanceValue ActorInheritance
        {
            // TODO: simplify/redesign(?) code to avoid possibility to have exception
            get
            {
                return GetSpecificPropertyValue<ActorInheritanceValue>(ActorInheritanceValue.PropertyType);
            }

            set
            {
                SetSpecificPropertyValue(ActorInheritanceValue.PropertyType, value);
            }
        }

        /// <summary>
        /// Returns ActorIconValue. It represents information from Actor's icon.
        /// </summary>
        /// <exception cref="FormatException">Throws FormatException if ActorInheritanceValue doesn't correspond to server JSON.</exception>
        [JsonIgnore]
        public ActorIconValue ActorIcon
        {
            get
            {
                return GetSpecificPropertyValue<ActorIconValue>(ActorIconValue.PropertyType);
            }

            set
            {
                SetSpecificPropertyValue(ActorIconValue.PropertyType, value);
            }
        }

        /// <summary>
        /// Set specific property value from SpecificPropertyValues list
        /// Common code to use for properties of ActorIcon, ActorInheritance, DocumentFile
        /// </summary>
        /// <typeparam name="T">Type of specific property value</typeparam>
        /// <param name="propertyType">Property type to use for search in SpecificPropertyValues list</param>
        /// <param name="valueToSet">Object of T type to set</param>
        private void SetSpecificPropertyValue<T>(PropertyTypePredefined propertyType, T valueToSet)
        {
            var specificProperty = SpecificPropertyValues.FirstOrDefault(p => p.PropertyType == propertyType);

            Assert.NotNull(specificProperty, "SpecificProperty shouldn't be null");
            specificProperty.CustomPropertyValue = valueToSet;
        }
    }
}
