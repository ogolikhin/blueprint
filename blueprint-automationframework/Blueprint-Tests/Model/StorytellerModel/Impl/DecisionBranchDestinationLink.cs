namespace Model.StorytellerModel.Impl
{
    public class DecisionBranchDestinationLink : IProcessLink
    {
        /*
        /// This class is also used to represents DecisionBranchDestinationLink
        /// The Id of the shape after the merge point
        /// (e.g. endShape ID in the picture)	
        [S]--[P]--+--<UD1>--+--[UT1]--+--[ST2]--+--[E]
                       |                        |
                       +-------[UT3]--+--[ST4]--+
        */

        /// <summary>
        /// The Id of source decision process shape (e.g. UD1)
        /// </summary>
        public int SourceId { get; set; }

        /// <summary>		
        /// The Id of the shape after the merge point
        /// (e.g. endShape ID in the picture)
        /// </summary>
        public int DestinationId { get; set; }

        /// <summary>		
        /// Order index for the process link (Order in which the links are drawn for decision points)
        /// (e.g. orderindex value of the first branch of UD1)
        /// </summary>
        public double Orderindex { get; set; }

        /// <summary>		
        /// Label for the DecisionBranchDestinationLink
        /// </summary>
        public string Label { get; set; }

    }
}