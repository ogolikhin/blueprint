namespace Model
{
    public enum ProcessType
    {
        None = 0,
        BusinessProcess = 1,
        UserToSystemProcess = 2,
        SystemToSystemProcess = 3
    }

    public interface IProcess
    {
        #region Properties

        /// <summary>
        /// The artifact Id of the Process
        /// </summary>
        int Id { get; set; }

        /// <summary>
        /// The name of the Process
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The parent Id of the Process
        /// </summary>
        int ParentId { get; set; }

        /// <summary>
        /// The order index of the Process
        /// </summary>
        int OrderIndex { get; set; }

        /// <summary>
        /// The artifact item type Id
        /// </summary>
        int TypeId { get; set; }

        /// <summary>
        /// The Process type prefix
        /// </summary>
        string TypePrefix { get; set; }

        /// <summary>
        /// The version Id of the Process
        /// </summary>
        int VersionId { get; set; }

        /// <summary>
        /// The description of the Process
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// The Process Type of the Process
        /// </summary>
        ProcessType Type { get; set; }

        /// <summary>
        /// The Process raw data
        /// </summary>
        string RawData { get; set; }

        /// <summary>
        /// User id of the the user that has a lock on the Process
        /// </summary>
        int? LockedByUserId { get; set; }

        #endregion Properties
    }
}
