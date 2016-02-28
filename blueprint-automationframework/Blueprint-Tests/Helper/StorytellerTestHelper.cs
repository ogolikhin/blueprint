using System.Linq;
using Common;
using Model;
using Model.Impl;
using NUnit.Framework;
using Utilities;
using Utilities.Factories;

namespace Helper
{
    public static class StorytellerTestHelper
    {
        /// <summary>
        /// Asserts that the two Processes are identical.
        /// </summary>
        /// <param name="process1">First Process</param>
        /// <param name="process2">Second Process being compared to the first</param>
        /// <exception cref="AssertionException">If process1 is not identical to process2</exception>
        public static void AssertProcessesAreIdentical(IProcess process1, IProcess process2)
        {
            ThrowIf.ArgumentNull(process1, nameof(process1));
            ThrowIf.ArgumentNull(process2, nameof(process2));

            // Assert basic Process properties
            Assert.AreEqual(process1.Id, process2.Id, "The ids of the processes don't match");
            Assert.AreEqual(process1.Name, process2.Name, "The names of the processes don't match");
            Assert.AreEqual(process1.BaseItemTypePredefined, process2.BaseItemTypePredefined,
                "The base item types of the processes don't match");
            Assert.AreEqual(process1.ProjectId, process2.ProjectId, "The project ids of the processes don't match");
            Assert.AreEqual(process1.TypePrefix, process2.TypePrefix, "The type prefixes of the processes don't match");

            // Assert that Link counts, Shape counts and Property counts are equal
            Assert.AreEqual(process1.PropertyValues.Count, process2.PropertyValues.Count,
                "The processes have different property counts");
            Assert.AreEqual(process1.Links.Count, process2.Links.Count, "The processes have different link counts");
            Assert.AreEqual(process1.Shapes.Count, process2.Shapes.Count,
                "The processes have different process shape counts");

            // Assert that Process properties are equal
            foreach (var process1Property in process1.PropertyValues)
            {
                var process2Property = process2.PropertyValues.First(p => p.Key == process1Property.Key);

                AssertPropertyValuesAreEqual(process1Property.Value, process2Property.Value);
            }

            // Assert that Process links are equal
            foreach (var process1Link in process1.Links)
            {
                var process2Link =
                    process2.Links.First(
                        l => l.SourceId == process1Link.SourceId && l.DestinationId == process1Link.DestinationId);

                AssertLinksAreEqual(process1Link, process2Link);
            }

            //Assert that Process shapes are equal
            foreach (var process1Shape in process1.Shapes)
            {
                var process2Shape = process2.Shapes.First(s => s.Name == process1Shape.Name);

                AssertShapesAreEqual(process1Shape, process2Shape);
            }
        }

        /// <summary>
        /// Assert that Property values are equal
        /// </summary>
        /// <param name="propertyValue1">The first Property value</param>
        /// <param name="propertyValue2">The Property value being compared to the first</param>
        private static void AssertPropertyValuesAreEqual(IPropertyValueInformation propertyValue1,
            IPropertyValueInformation propertyValue2)
        {
            Assert.AreEqual(propertyValue1.PropertyName, propertyValue2.PropertyName, "Property names do not match");
            Assert.AreEqual(propertyValue1.TypePredefined, propertyValue2.TypePredefined, "Property types do not match");
            Assert.AreEqual(propertyValue1.TypeId, propertyValue2.TypeId, "Property type ids do not match");
            Assert.AreEqual(propertyValue1.IsVirtual, propertyValue2.IsVirtual, "Property 'IsVirtual' does not match");

            if (propertyValue1.PropertyName == "StoryLinks")
            {
                AssertStoryLinksAreEqual((IStoryLink)propertyValue1.Value, (IStoryLink)propertyValue2.Value);
            }
            else
            {
                Assert.AreEqual(propertyValue1.Value, propertyValue2.Value, "Property values do not match");
            }
        }

        /// <summary>
        /// Assert that Process Links are equal
        /// </summary>
        /// <param name="link1">The first Link</param>
        /// <param name="link2">The Link being compared to the first</param>
        private static void AssertLinksAreEqual(IProcessLink link1, IProcessLink link2)
        {
            Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
            Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
            Assert.AreEqual(link1.Label, link2.Label, "Link labels do not match");
            Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
        }

        /// <summary>
        /// Assert that Process Shapes are equal
        /// </summary>
        /// <param name="shape1">The first Shape</param>
        /// <param name="shape2">The Shape being compared to the first</param>
        private static void AssertShapesAreEqual(IProcessShape shape1, IProcessShape shape2)
        {
            // Note that if a shape id of the first Process being compared is less than 0, then the first Process 
            // is a process that will be updated with proper id values at the back end.  If the shape id of
            // the first process being compared is greater than 0, then the shape ids should match.
            if (shape1.Id < 0)
            {
                Assert.That(shape2.Id > 0, "Returned shape id was negative");
            }
            else
            {
                Assert.AreEqual(shape1.Id, shape2.Id, "Shape ids do not match");
            }

            Assert.AreEqual(shape1.Name, shape2.Name, "Shape names do not match");
            Assert.AreEqual(shape1.BaseItemTypePredefined, shape2.BaseItemTypePredefined,
                "Shape base item types do not match");
            Assert.AreEqual(shape1.ProjectId, shape2.ProjectId, "Shape project ids do not match");
            Assert.AreEqual(shape1.ParentId, shape2.ParentId, "Shape parent ids do not match");
            Assert.AreEqual(shape1.TypePrefix, shape2.TypePrefix, "Shape type prefixes do not match");

            // Assert that Shape properties are equal
            foreach (var shape1Property in shape1.PropertyValues)
            {
                var shape2Property = shape2.PropertyValues.First(p => p.Key == shape1Property.Key);

                AssertPropertyValuesAreEqual(shape1Property.Value, shape2Property.Value);
            }
        }

        /// <summary>
        /// Assert that Story Links are equal
        /// </summary>
        /// <param name="link1">The first Story Link</param>
        /// <param name="link2">The Story Link being compared to the first</param>
        private static void AssertStoryLinksAreEqual(IStoryLink link1, IStoryLink link2)
        {
            Assert.AreEqual(link1.AssociatedReferenceArtifactId, link2.AssociatedReferenceArtifactId, "Link associated reference artifact ids do not match");
            Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
            Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
            Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
        }

        /// <summary>
        /// Create Random Value with a supplied Prefix
        /// </summary>
        /// <param name="prefix">The prefix</param>
        /// <param name="numberOfCharacters">The number of alphanumeric characters to append to the prefix</param>
        /// <returns>A random alpha numeric character value with a supplied prefix</returns>
        public static string RandomValueWithPrefix(string prefix, uint numberOfCharacters)
        {
            return I18NHelper.FormatInvariant("{0}_{1}", prefix, RandomGenerator.RandomAlphaNumericUpperAndLowerCase(numberOfCharacters));
        }
    }
}