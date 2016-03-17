using System;
using System.Collections.Generic;
using System.Linq;
using Model;
using Model.OpenApiModel;
using Model.StorytellerModel;
using Model.StorytellerModel.Impl;
using NUnit.Framework;
using Utilities;

namespace Helper
{
    public static class StorytellerTestHelper
    {
        #region Public Methods

        /// <summary>
        /// Asserts that the two Processes are identical.
        /// </summary>
        /// <param name="process1">First Process</param>
        /// <param name="process2">Second Process being compared to the first</param>
        /// <param name="allowNegativeShapeIds">Allows for inequality of shape ids where a newly added shape has a negative id</param>
        /// <exception cref="AssertionException">If process1 is not identical to process2</exception>
        /// <remarks>If 1 of the 2 processes being compared has negative Ids, that process must be the first parameter</remarks>
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
            AssertLinksAreEqual(process1, process2);

            //Assert that Process shapes are equal
            foreach (var process1Shape in process1.Shapes)
            {
                var process2Shape = FindProcessShapeByName(process1Shape.Name, process2.Shapes);

                AssertShapesAreEqual(process1Shape, process2Shape, allowNegativeShapeIds);
            }
        }

        /// <summary>
        /// Create and Get the Default Process
        /// </summary>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="project">The project where the process artifact is created</param>
        /// <param name="user">The user creating the process artifact</param>
        /// <returns>The created process</returns>
        public static IProcess CreateAndGetDefaultProcess(IStoryteller storyteller, IProject project, IUser user)
        {
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(project, nameof(project));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Create default process artifact
            var addedProcessArtifact = storyteller.CreateAndSaveProcessArtifact(project, BaseArtifactType.Process, user);

            // Get default process
            var returnedProcess = storyteller.GetProcess(user, addedProcessArtifact.Id);

            Assert.IsNotNull(returnedProcess, "The process returned from GetProcess() was null.");

            return returnedProcess;
        }

        /// <summary>
        /// Updates and verifies the processes returned from UpdateProcess and GetProcess
        /// </summary>
        /// <param name="processToVerify">The process to verify</param>
        /// <param name="storyteller">The storyteller instance</param>
        /// <param name="user">The user that updates the process</param>
        public static void UpdateAndVerifyProcess(IProcess processToVerify, IStoryteller storyteller, IUser user)
        {
            ThrowIf.ArgumentNull(processToVerify, nameof(processToVerify));
            ThrowIf.ArgumentNull(storyteller, nameof(storyteller));
            ThrowIf.ArgumentNull(user, nameof(user));

            // Update the process using UpdateProcess
            var processReturnedFromUpdate = storyteller.UpdateProcess(user, processToVerify);

            Assert.IsNotNull(processReturnedFromUpdate, "UpdateProcess() returned a null process.");

            // Assert that process returned from the UpdateProcess method is identical to the process sent with the UpdateProcess method
            // Allow negative shape ids in the process being verified
            AssertProcessesAreIdentical(processToVerify, processReturnedFromUpdate, allowNegativeShapeIds: true);

            // Get the process using GetProcess
            var processReturnedFromGet = storyteller.GetProcess(user, processToVerify.Id);

            Assert.IsNotNull(processReturnedFromGet, "GetPRocess() returned a null process.");

            // Assert that the process returned from the GetProcess method is identical to the process returned from the UpdateProcess method
            // Don't allow and negative shape ids
            AssertProcessesAreIdentical(processReturnedFromUpdate, processReturnedFromGet);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        /// Assert that Process Artifact Path Links are equal
        /// </summary>
        /// <param name="artifactPathlink1">The first ArtifactPath Link</param>
        /// <param name="artifactPathlink2">The Artifact Path Link being compared to the first</param>
        /// <param name="doDeepCompare">If false, only compare Ids, else compare all properties</param>
        private static void AssertArtifactPathLinksAreEqual(ArtifactPathLink artifactPathlink1, ArtifactPathLink artifactPathlink2, bool doDeepCompare = true)
        {
            if ((artifactPathlink1 == null) || (artifactPathlink2 == null))
            {
                Assert.That((artifactPathlink1 == null) && (artifactPathlink2 == null), "One of the artifact path links is null while the other is not null");
            }

            if (artifactPathlink1 != null)
            {
                Assert.AreEqual(artifactPathlink1.Id, artifactPathlink2.Id, "Artifact path link ids do not match");

                if (doDeepCompare)
                {
                    Assert.AreEqual(artifactPathlink1.BaseItemTypePredefined, artifactPathlink2.BaseItemTypePredefined,
                        "Artifact path link base item types do not match");
                    Assert.AreEqual(artifactPathlink1.Link, artifactPathlink2.Link, "Artifact path link links do not match");
                    Assert.AreEqual(artifactPathlink1.Name, artifactPathlink2.Name, "Artifact path link names do not match");
                    Assert.AreEqual(artifactPathlink1.ProjectId, artifactPathlink2.ProjectId, "Artifact path link project ids do not match");
                    Assert.AreEqual(artifactPathlink1.TypePrefix, artifactPathlink2.TypePrefix, "Artifact path link type prefixes do not match");
                }
            }
        }

        /// <summary>
        /// Assert that Property values are equal
        /// </summary>
        /// <param name="propertyValue1">The first Property value</param>
        /// <param name="propertyValue2">The Property value being compared to the first</param>
        private static void AssertPropertyValuesAreEqual(PropertyValueInformation propertyValue1,
            PropertyValueInformation propertyValue2)
        {
            Assert.AreEqual(propertyValue1.PropertyName, propertyValue2.PropertyName, "Property names do not match: {0} != {1}", propertyValue1.PropertyName, propertyValue2.PropertyName);
            Assert.AreEqual(propertyValue1.TypePredefined, propertyValue2.TypePredefined, "Property types do not match");
            Assert.AreEqual(propertyValue1.TypeId, propertyValue2.TypeId, "Property type ids do not match");

            // Asserts story links only if not null
            if (propertyValue1.PropertyName == "StoryLinks" && propertyValue1.Value != null)
            {
                AssertStoryLinksAreEqual((StoryLink)propertyValue1.Value, (StoryLink)propertyValue2.Value);
            }
            // TODO: To be removed when link labels removed from backend model
            else if (propertyValue1.PropertyName != "LinkLabels")
            {
                Assert.AreEqual(propertyValue1.Value, propertyValue2.Value, "Property values do not match: {0} != {1} for Property name: {2}", propertyValue1.Value, propertyValue2.Value, propertyValue1.PropertyName);
            }
        }

        /// <summary>
        /// Assert that Process Links are equal
        /// </summary>
        /// <param name="process1">The first process containing the links to compare</param>
        /// <param name="process2">The process containing the links being compared to the first</param>
        private static void AssertLinksAreEqual(IProcess process1, IProcess process2)
        {
            foreach (var link1 in process1.Links)
            {
                ProcessLink link2 = new ProcessLink();

                if (link1.SourceId > 0 && link1.DestinationId > 0)
                {
                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the destination id is < 0, we find the name of the destination shape and 
                // then locate this shape in the second process. We then replace the destination id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId > 0 && link1.DestinationId < 0)
                {
                    var link1DestinationShape = FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the source id is < 0, we find the name of the source shape and 
                // then locate this shape in the second process. We then replace the source id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId < 0 && link1.DestinationId > 0)
                {
                    var link1SourceShape = FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                else if (link1.SourceId < 0 && link1.DestinationId < 0)
                {
                    var link1SourceShape = FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    var link1DestinationShape = FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    link2Shape = FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }

                Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
                Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
                Assert.AreEqual(link1.Label, link2.Label, "Link labels do not match");
                Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
            }
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

            // Assert associated artifacts are equal by checking artifact Id only
            AssertArtifactPathLinksAreEqual(shape1.AssociatedArtifact, shape2.AssociatedArtifact, doDeepCompare: false);

            // Assert that Shape properties are equal
            foreach (var shape1Property in shape1.PropertyValues)
            {
                var shape2Property = FindPropertyValue(shape1Property.Key, shape2.PropertyValues);

                AssertPropertyValuesAreEqual(shape1Property.Value, shape2Property.Value);
            }
        }

        /// <summary>
        /// Assert that Story Links are equal
        /// </summary>
        /// <param name="link1">The first Story Link</param>
        /// <param name="link2">The Story Link being compared to the first</param>
        private static void AssertStoryLinksAreEqual(StoryLink link1, StoryLink link2)
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
        private static ArtifactPathLink FindArtifactPathLink(ArtifactPathLink linkToFind,
            IEnumerable<ArtifactPathLink> linksToSearchThrough)
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
        private static ProcessLink FindProcessLink(ProcessLink linkToFind,
            List<ProcessLink> linksToSearchThrough)
        {
            var linkFound = linksToSearchThrough.Find(l => l.SourceId == linkToFind.SourceId && l.DestinationId == linkToFind.DestinationId);
 
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
            List<ProcessShape> shapesToSearchThrough)
        {
            ThrowIf.ArgumentNull(shapesToSearchThrough, nameof(shapesToSearchThrough));

            var shapeFound = shapesToSearchThrough.Find(s => s.Name == shapeName);

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
            List<ProcessShape> shapesToSearchThrough)
        {
            var shapeFound = shapesToSearchThrough.Find(s => s.Id == shapeId);

            Assert.IsNotNull(shapeFound, "Could not find a Process Shape with Id {0}", shapeId);

            return shapeFound;
        }

        #endregion Private Methods
    }
}
