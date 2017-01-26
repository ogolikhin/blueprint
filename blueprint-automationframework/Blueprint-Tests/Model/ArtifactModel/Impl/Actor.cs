using System;
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
                // Finding ActorInheritence among other properties
                var actorInheritanceProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.ActorInheritance);
                if ((actorInheritanceProperty == null) || (actorInheritanceProperty.CustomPropertyValue == null))
                {
                    return null;
                }
                // Deserialization
                string actorInheritancePropertyString = actorInheritanceProperty.CustomPropertyValue.ToString();
                var actorInheritanceValue = JsonConvert.DeserializeObject<ActorInheritanceValue>(actorInheritancePropertyString);

                CheckIsJsonChanged<ActorInheritanceValue>(actorInheritanceProperty);

                return actorInheritanceValue;
            }

            set
            {
                var actorInheritanceProperty = SpecificPropertyValues.FirstOrDefault(
                    p => p.PropertyType == PropertyTypePredefined.ActorInheritance);

                Assert.NotNull(actorInheritanceProperty, "ActorInheritanceProperty shouldn't be null");
                actorInheritanceProperty.CustomPropertyValue = value;
            }
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
