namespace Model
{
    public interface ILinkLabelInfo
    {
        #region Properties

        /// <summary>
        /// The id of the link
        /// </summary>
        int LinkId { get; set; }

        /// <summary>
        /// The label of the link
        /// </summary>
        string Label { get; set; }

        #endregion Properties
    }
}
