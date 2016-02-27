namespace Model
{
    public interface IProcessLink
    {
        #region Properties

        /// <summary>
        /// Source Id for the process link
        /// </summary>
        int SourceId { get; set; }

        /// <summary>
        /// Destination Id for the process link
        /// </summary>
        int DestinationId { get; set; }

        /// <summary>
        /// Order index for the process link (Order in which the links are drawn for decision points)
        /// </summary>
        int Orderindex { get; set; }

        /// <summary>
        /// Label for the process link
        /// </summary>
        string Label { get; set; }

        #endregion Properties
    }
}
