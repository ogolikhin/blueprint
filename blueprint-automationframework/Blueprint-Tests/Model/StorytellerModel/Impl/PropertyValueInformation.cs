using System;
using System.Collections.Generic;
using System.Linq;
using Model.Common.Enums;
using NUnit.Framework;
using Utilities;

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

        /// <summary>
        /// Find a Property in an enumeration of Properties
        /// </summary>
        /// <param name="keyToFind">The property to find</param>
        /// <param name="propertiesToSearchThrough">The properties to search though</param>
        /// <returns>The found Property</returns>
        public static KeyValuePair<string, PropertyValueInformation> FindPropertyValue(string keyToFind,
        Dictionary<string, PropertyValueInformation> propertiesToSearchThrough)
        {
            var propertyFound = propertiesToSearchThrough.ToList().Find(p => String.Equals(p.Key, keyToFind, StringComparison.CurrentCultureIgnoreCase));

            Assert.IsNotNull(propertyFound, "Could not find a Property with Name: {0}", keyToFind);

            return propertyFound;
        }

        /// <summary>
        /// Assert that Property values are equal
        /// </summary>
        /// <param name="propertyValue1">The first Property value</param>
        /// <param name="propertyValue2">The Property value being compared to the first</param>
        public static void AssertAreEqual(PropertyValueInformation propertyValue1,
            PropertyValueInformation propertyValue2)
        {
            ThrowIf.ArgumentNull(propertyValue1, nameof(propertyValue1));
            ThrowIf.ArgumentNull(propertyValue2, nameof(propertyValue2));

            Assert.AreEqual(propertyValue1.PropertyName, propertyValue2.PropertyName, "Property names do not match: {0} != {1}", propertyValue1.PropertyName, propertyValue2.PropertyName);
            Assert.AreEqual(propertyValue1.TypePredefined, propertyValue2.TypePredefined, "Property types do not match");
            Assert.AreEqual(propertyValue1.TypeId, propertyValue2.TypeId, "Property type ids do not match");

            // Asserts story links only if not null
            if (propertyValue1.PropertyName == "StoryLinks" && propertyValue1.Value != null)
            {
                StoryLink.AssertAreEqual((StoryLink)propertyValue1.Value, (StoryLink)propertyValue2.Value);
            }
        }
    }
}