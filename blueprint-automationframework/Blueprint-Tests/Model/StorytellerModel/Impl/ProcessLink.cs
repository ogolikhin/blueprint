using System.Collections.Generic;
using NUnit.Framework;
using Utilities;

namespace Model.StorytellerModel.Impl
{
    public class ProcessLink : IProcessLink
    {

        /// <summary>
        /// Source Id for the process link
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>		
        /// Destination Id for the process link
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>		
        /// Order index for the process link (Order in which the links are drawn for decision points)
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>		
        /// Label for the process link
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// Find a Process Link in an enumeration of Process Links
        /// </summary>
        /// <param name="linkToFind">The process link to find</param>
        /// <param name="linksToSearchThrough">The process links to search</param>
        /// <returns>The found process link</returns>
        public static ProcessLink FindProcessLink(ProcessLink linkToFind,
            List<ProcessLink> linksToSearchThrough)
        {
            ThrowIf.ArgumentNull(linkToFind, nameof(linkToFind));
            ThrowIf.ArgumentNull(linksToSearchThrough, nameof(linksToSearchThrough));

            var linkFound = linksToSearchThrough.Find(l => l.SourceId == linkToFind.SourceId && l.DestinationId == linkToFind.DestinationId);

            Assert.IsNotNull(linkFound, "Could not find and ProcessLink with Source Id {0} and Destination Id {1}", linkToFind.SourceId, linkToFind.DestinationId);

            return linkFound;
        }

        /// <summary>
        /// Assert that Process Links are equal
        /// </summary>
        /// <param name="process1">The first process containing the links to compare</param>
        /// <param name="process2">The process containing the links being compared to the first</param>
        public static void AssertAreEqual(IProcess process1, IProcess process2)
        {
            ThrowIf.ArgumentNull(process1, nameof(process1));
            ThrowIf.ArgumentNull(process2, nameof(process2));

            foreach (var link1 in process1.Links)
            {
                var link2 = new ProcessLink();

                if (link1.SourceId > 0 && link1.DestinationId > 0)
                {
                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the destination id is < 0, we find the name of the destination shape and 
                // then locate this shape in the second process. We then replace the destination id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId > 0 && link1.DestinationId < 0)
                {
                    var link1DestinationShape = ProcessShape.FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    var link2Shape = ProcessShape.FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                // If the source id is < 0, we find the name of the source shape and 
                // then locate this shape in the second process. We then replace the source id
                // of the first link with the id of that shape.  This allows us to compare links.
                else if (link1.SourceId < 0 && link1.DestinationId > 0)
                {
                    var link1SourceShape = ProcessShape.FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = ProcessShape.FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }
                else if (link1.SourceId < 0 && link1.DestinationId < 0)
                {
                    var link1SourceShape = ProcessShape.FindProcessShapeById(link1.SourceId, process1.Shapes);
                    var link2Shape = ProcessShape.FindProcessShapeByName(link1SourceShape.Name, process2.Shapes);

                    link1.SourceId = link2Shape.Id;

                    var link1DestinationShape = ProcessShape.FindProcessShapeById(link1.DestinationId, process1.Shapes);
                    link2Shape = ProcessShape.FindProcessShapeByName(link1DestinationShape.Name, process2.Shapes);

                    link1.DestinationId = link2Shape.Id;

                    link2 = FindProcessLink(link1, process2.Links);
                }

                Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
                Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
                Assert.AreEqual(link1.Label, link2.Label, "Link labels do not match");
                Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
            }
        }
    }
}