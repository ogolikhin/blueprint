using Common;
using NUnit.Framework;

namespace Utilities
{
    public static class ObjectsCompare
    {
        /// <summary>
        /// Compare fields of string, bool or int? types. Uses Assert.AreEqual from NUnit
        /// </summary>
        /// <param name="expectedObject">expected object</param>
        /// <param name="actualObject">actual object</param>
        public static void CompareBasicTypeFields(object expectedObject, object actualObject)
        {
            ThrowIf.ArgumentNull(expectedObject, nameof(expectedObject));
            ThrowIf.ArgumentNull(actualObject, nameof(actualObject));

            var objectType = expectedObject.GetType();
            var expectedProperties = expectedObject.GetType().GetProperties();
            var actualProperties = actualObject.GetType().GetProperties();
            for (int i = 0; i < expectedProperties.Length; i++)
            {
                var expectedPropertyValue = expectedProperties[i].GetValue(expectedObject, null);
                var actualPropertyValue = actualProperties[i].GetValue(actualObject, null);
                var propertyType = expectedPropertyValue?.GetType();
                if ((propertyType == typeof(bool)) || (propertyType == typeof(string)) || (propertyType == typeof(int?)))
                {
                    Assert.AreEqual(expectedPropertyValue, actualPropertyValue,
                         I18NHelper.FormatInvariant("{0} should have expected value of {1}", objectType.Name,
                         expectedProperties[i].Name));
                }
            }
        }
    }
}
