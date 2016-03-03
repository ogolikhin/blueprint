﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Model;
using Model.Impl;
using NUnit.Framework;
using Utilities;

namespace Helper
{
    public static class StorytellerTestHelper
    {
        /// <summary>
        /// Asserts that the two Processes are identical.
        /// </summary>
        /// <param name="process1">First Process</param>
        /// <param name="process2">Second Process being compared to the first</param>
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        /// <exception cref="AssertionException">If process1 is not identical to process2</exception>
        public static void AssertProcessesAreIdentical(IProcess process1, IProcess process2, bool allowNegativeShapeIds = false)
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

            // Assert that ArtifactPathLinks counts, Link counts, Shape counts and Property counts are equal
            Assert.AreEqual(process1.ArtifactPathLinks.Count, process2.ArtifactPathLinks.Count,
                "The processes have different artifact path link counts");
            Assert.AreEqual(process1.PropertyValues.Count, process2.PropertyValues.Count,
                "The processes have different property counts");
            Assert.AreEqual(process1.Links.Count, process2.Links.Count, "The processes have different link counts");
            Assert.AreEqual(process1.Shapes.Count, process2.Shapes.Count,
                "The processes have different process shape counts");

            // Assert that Process artifact path links are equal
            foreach (var process1ArtifactPathLink in process1.ArtifactPathLinks)
            {
                var process2ArtifactPathLink = FindArtifactPathLink(process1ArtifactPathLink, process2.ArtifactPathLinks);

                AssertArtifactPathLinksAreEqual(process1ArtifactPathLink, process2ArtifactPathLink);
            }

            // Assert that Process properties are equal
            foreach (var process1Property in process1.PropertyValues)
            {
                var process2Property = FindPropertyValue(process1Property.Key, process2.PropertyValues);

                AssertPropertyValuesAreEqual(process1Property.Value, process2Property.Value);
            }

            // Assert that process links are the same
            // This involves finding the new id of shapes that had negative ids in the source process
            // Also, shapes and links need to be modified so that either process can have the negative ids

            List<ProcessLink> links1;
            List<ProcessLink> links2;
            List<ProcessShape> shapes1;
            List<ProcessShape> shapes2;

            if (process2.Links.Find(l => l.SourceId < 0 || l.DestinationId < 0) != null)
            {
                links2 = process1.Links;  
                links1 = process2.Links;
                shapes2 = process1.Shapes;
                shapes1 = process2.Shapes;
            }
            else
            {
                links1 = process1.Links;
                links2 = process2.Links;
                shapes1 = process1.Shapes;
                shapes2 = process2.Shapes;
            }

            foreach (var link1 in links1)
            {
                if (link1.SourceId > 0 && link1.DestinationId > 0)
                {
                    var link2 = FindProcessLink(link1, links2);
                    AssertLinksAreEqual(link1, link2);
                }
                else if (link1.SourceId > 0 && link1.DestinationId < 0)
                {
                    var link1ShapeDestination = FindProcessShapeById(link1.DestinationId, shapes1);
                    var link2Shape = FindProcessShapeByName(link1ShapeDestination.Name, shapes2);

                    link1.DestinationId = link2Shape.Id;

                    var link2 = FindProcessLink(link1, links2);
                    AssertLinksAreEqual(link1, link2);
                }
                else if (link1.SourceId < 0 && link1.DestinationId > 0)
                {
                    var link1ShapeSource = FindProcessShapeById(link1.SourceId, shapes1);
                    var link2Shape = FindProcessShapeByName(link1ShapeSource.Name, shapes2);

                    link1.SourceId = link2Shape.Id;

                    var link2 = FindProcessLink(link1, links2);
                    AssertLinksAreEqual(link1, link2);
                }
            }

            //Assert that Process shapes are equal
            foreach (var process1Shape in process1.Shapes)
            {
                var process2Shape = FindProcessShapeByName(process1Shape.Name, process2.Shapes);

                AssertShapesAreEqual(process1Shape, process2Shape, allowNegativeShapeIds);
            }
        }

        /// <summary>
        /// Assert that Process Artifact Path Links are equal
        /// </summary>
        /// <param name="artifactPathlink1">The first ArtifactPath Link</param>
        /// <param name="artifactPathlink2">The Artifact Path Link being compared to the first</param>
        private static void AssertArtifactPathLinksAreEqual(IArtifactPathLink artifactPathlink1, IArtifactPathLink artifactPathlink2)
        {
            Assert.AreEqual(artifactPathlink1.BaseItemTypePredefined, artifactPathlink2.BaseItemTypePredefined,
                "Artifact path link base item types do not match");
            Assert.AreEqual(artifactPathlink1.Id, artifactPathlink2.Id, "Artifact path link ids do not match");
            Assert.AreEqual(artifactPathlink1.Link, artifactPathlink2.Link, "Artifact path link links do not match");
            Assert.AreEqual(artifactPathlink1.Name, artifactPathlink2.Name, "Artifact path link names do not match");
            Assert.AreEqual(artifactPathlink1.ProjectId, artifactPathlink2.ProjectId, "Artifact path link project ids do not match");
            Assert.AreEqual(artifactPathlink1.TypePrefix, artifactPathlink2.TypePrefix, "Artifact path link type prefixes do not match");
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

            if (propertyValue1.PropertyName == "StoryLinks" && propertyValue1.Value != null)
            {
                AssertStoryLinksAreEqual((IStoryLink)propertyValue1.Value, (IStoryLink)propertyValue2.Value);
            }
            else
            {
                Assert.AreEqual(propertyValue1.Value, propertyValue2.Value, "Property names do not match");
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
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        private static void AssertShapesAreEqual(IProcessShape shape1, IProcessShape shape2, bool allowNegativeShapeIds)
        {

            // Note that if a shape id of the first Process being compared is less than 0, then the first Process 
            // is a process that will be updated with proper id values at the back end.  If the shape id of
            // the first process being compared is greater than 0, then the shape ids should match.
            if (allowNegativeShapeIds && shape1.Id < 0)
            {
                Assert.That(shape2.Id > 0, "Returned shape id was negative");
            }
            else if (allowNegativeShapeIds && shape2.Id < 0)
            {
                Assert.That(shape1.Id > 0, "Returned shape id was negative");
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
                var shape2Property = FindPropertyValue(shape1Property.Key, shape2.PropertyValues);

                AssertPropertyValuesAreEqual(shape1Property.Value, shape2Property.Value);
            }
        }

        /// <summary>
        /// Toggle the Case of the First Character in a String
        /// </summary>
        /// <param name="valueToModify">The string to modify</param>
        /// <returns>The modified string</returns>
        public static string ToggleCaseOfFirstCharacter(string valueToModify)
        {
            ThrowIf.ArgumentNull(valueToModify, nameof(valueToModify));

            if (char.IsUpper(valueToModify[0]))
            {
                valueToModify = char.ToLower(valueToModify[0], CultureInfo.InvariantCulture) + valueToModify.Substring(1);
            }
            else
            {
                valueToModify = char.ToUpper(valueToModify[0], CultureInfo.InvariantCulture) + valueToModify.Substring(1);
            }

            return valueToModify;
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
        /// Find an Artifact Path Link in an enumeration of Artifact Path Links
        /// </summary>
        /// <param name="linkToFind">The artifact path link to find</param>
        /// <param name="linksToSearchThrough">The artifact path links to search through</param>
        /// <returns>The artifact path link that is found</returns>
        private static IArtifactPathLink FindArtifactPathLink(IArtifactPathLink linkToFind,
            IEnumerable<IArtifactPathLink> linksToSearchThrough)
        {
            var linkFound = linksToSearchThrough.ToList().Find(p => p.Id == linkToFind.Id);

            Assert.IsNotNull(linkFound, "Could not find and ArtifactPathLink with Id {0}", linkToFind.Id);

            return linkFound;
        }

        /// <summary>
        /// Find a Property in an enumeration of Properties
        /// </summary>
        /// <param name="keyToFind">The property to find</param>
        /// <param name="propertiesToSearchThrough">The properties to search though</param>
        /// <returns>The found Property</returns>
        private static KeyValuePair<string, PropertyValueInformation> FindPropertyValue(string keyToFind,
        Dictionary<string, PropertyValueInformation> propertiesToSearchThrough)
        {
            var propertyFound = propertiesToSearchThrough.ToList().Find(p => string.Equals(p.Key, keyToFind, StringComparison.CurrentCultureIgnoreCase));

            Assert.IsNotNull(propertyFound, "Could not find a Property with Name: {0}", keyToFind);

            return propertyFound;
        }

        /// <summary>
        /// Find a Process Link in an enumeration of Process Links
        /// </summary>
        /// <param name="linkToFind">The process link to find</param>
        /// <param name="linksToSearchThrough">The process links to search</param>
        /// <returns>The found process link</returns>
        private static IProcessLink FindProcessLink(IProcessLink linkToFind,
            IEnumerable<IProcessLink> linksToSearchThrough)
        {
            var linkFound = linksToSearchThrough.ToList()
                    .Find(l => l.SourceId == linkToFind.SourceId && l.DestinationId == linkToFind.DestinationId);
 
            Assert.IsNotNull(linkFound, "Could not find and ProcessLink with Source Id {0} and Destination Id {1}", linkToFind.SourceId, linkToFind.DestinationId);

            return linkFound;
        }

        /// <summary>
        /// Find a Process Shape by name in an enumeration of Process Shapes
        /// </summary>
        /// <param name="shapeName">The name of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        private static IProcessShape FindProcessShapeByName(string shapeName,
            IEnumerable<IProcessShape> shapesToSearchThrough)
        {
            var shapeFound = shapesToSearchThrough.ToList().Find(s => s.Name == shapeName);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Name {0}", shapeName);

            return shapeFound;
        }

        /// <summary>
        /// Find a Process Shape by id in an enumeration of Process Shapes
        /// </summary>
        /// <param name="shapeId">The id of the process shape to find</param>
        /// <param name="shapesToSearchThrough">The process shapes to search</param>
        /// <returns>The found process shape</returns>
        private static IProcessShape FindProcessShapeById(int shapeId,
            IEnumerable<IProcessShape> shapesToSearchThrough)
        {
            var shapeFound = shapesToSearchThrough.ToList().Find(s => s.Id == shapeId);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Id {0}", shapeId);

            return shapeFound;
        }
    }
}
