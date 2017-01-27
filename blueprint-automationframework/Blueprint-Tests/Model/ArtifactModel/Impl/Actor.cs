﻿using System;
using Model.ArtifactModel.Impl.PredefinedProperties;
using Newtonsoft.Json;
using System.Linq;
using Common;
using NUnit.Framework;

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

        private T GetSpecificPropertyValue<T>(PropertyTypePredefined propertyType)
        {
            var specificProperty = SpecificPropertyValues.FirstOrDefault(
                p => p.PropertyType == propertyType);
            if (specificProperty?.CustomPropertyValue == null)
            {
                return default(T);
            }
            // Deserialization
            string actorInheritancePropertyString = specificProperty.CustomPropertyValue.ToString();
            var specificPropertyValue = JsonConvert.DeserializeObject<T>(actorInheritancePropertyString);

            CheckIsJsonChanged<T>(specificProperty);

            return specificPropertyValue;
        }

        private void SetSpecificPropertyValue<T>(PropertyTypePredefined propertyType, T valueToSet)
        {
            var specificProperty = SpecificPropertyValues.FirstOrDefault(p => p.PropertyType == propertyType);

            Assert.NotNull(specificProperty, "SpecificProperty shouldn't be null");
            specificProperty.CustomPropertyValue = valueToSet;
        }

        /// <summary>
        /// Checks that CustomProperty model corresponds to JSON from Blueprint server 
        /// </summary>
        /// <param name="property">property to check</param>
        private static void CheckIsJsonChanged<T>(CustomProperty property)
        {
            // Deserialization
            string specificPropertyString = property.CustomPropertyValue.ToString();
            var specificPropertyValue = JsonConvert.DeserializeObject<T>(specificPropertyString);

            // Try to serialize and compare with JSON from the server
            string serializedObject = JsonConvert.SerializeObject(specificPropertyValue, Formatting.Indented);
            bool isJsonChanged = !(string.Equals(specificPropertyString, serializedObject, StringComparison.OrdinalIgnoreCase));
            string msg = I18NHelper.FormatInvariant("JSON for {0} has been changed!", nameof(T));
            Assert.IsFalse(isJsonChanged, msg);
        }
    }
}
