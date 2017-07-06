using System.Collections.Generic;

namespace BluePrintSys.Messaging.CrossCutting.Logging
{
    /// <summary>
    /// Log manager manages the log listeners.
    /// </summary>
    public interface ILogManager
    {
        /// <summary>
        /// Clears the collection of log listeners.
        /// </summary>
        void ClearListeners();

        /// <summary>
        /// Adds a log listener to the collection.
        /// </summary>
        /// <param name="log">The log listener to add.</param>
        void AddListener(ILogListener log);

        /// <summary>
        /// Gets the list of all log listeners.
        /// </summary>
        /// <returns>The list of log listeners.</returns>
        IEnumerable<ILogListener> GetListeners();

        /// <summary>
        /// Removes a specific log listener from the collection.
        /// </summary>
        /// <param name="log">The log listener to remove</param>
        void RemoveListener(ILogListener log);

        /// <summary>
        /// Call this if you want to explicity shut down the log manager when the process is still waiting to log
        /// </summary>
        void CloseManager();
    }
}