using Common;
using NUnit.Framework;
using Utilities;

namespace Model.StorytellerModel.Impl
{
    public class StorytellerProperty : IStorytellerProperty
    {
        public string Name { get; set; }
        public int PropertyTypeId { get; set; }
        public int? PropertyType { get; set; }
        public string Value { get; set; }

        /// <summary>
        /// Asserts that the properties of the two IStorytellerProperty objects are equal.
        /// </summary>
        /// <param name="expectedProperty">The expected IStorytellerProperty.</param>
        /// <param name="actualProperty">The actual IStorytellerProperty.</param>
        /// <param name="skipIds">(optional) Pass true to skip comparison of Id properties.</param>
        /// <exception cref="AssertionException">If any properties don't match.</exception>
        public static void AssertAreEqual(IStorytellerProperty expectedProperty, IStorytellerProperty actualProperty, bool skipIds = false)
        {
            ThrowIf.ArgumentNull(expectedProperty, nameof(expectedProperty));
            ThrowIf.ArgumentNull(actualProperty, nameof(actualProperty));

            Assert.AreEqual(expectedProperty.Name, actualProperty.Name, "The Name properties don't match!");
            Assert.AreEqual(expectedProperty.PropertyTypeId, actualProperty.PropertyTypeId, "The PropertyTypeId properties don't match for property: {0}!", actualProperty.Name);
            Assert.AreEqual(expectedProperty.PropertyType, actualProperty.PropertyType, "The PropertyType properties don't match for property: {0}!", actualProperty.Name);

            if (!(skipIds && actualProperty.Name.EqualsOrdinalIgnoreCase("Id")))
            {
                Assert.AreEqual(expectedProperty.Value, actualProperty.Value, "The Value properties don't match for property: {0}!", actualProperty.Name);
            }
        }
    }
}