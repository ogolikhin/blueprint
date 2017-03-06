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
        /// <param name="link1">The first process link</param>
        /// <param name="link2">The process linkc being compared to the first</param>
        public static void AssertAreEqual(ProcessLink link1, ProcessLink link2)
        {
            ThrowIf.ArgumentNull(link1, nameof(link1));
            ThrowIf.ArgumentNull(link2, nameof(link2));

            Assert.AreEqual(link1.SourceId, link2.SourceId, "Link sources do not match");
            Assert.AreEqual(link1.DestinationId, link2.DestinationId, "Link destinations do not match");
            Assert.AreEqual(link1.Label, link2.Label, "Link labels do not match");
            Assert.AreEqual(link1.Orderindex, link2.Orderindex, "Link order indexes do not match");
        }
    }
}