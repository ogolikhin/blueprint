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
    }
}