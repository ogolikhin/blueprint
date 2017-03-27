using System.Collections.Generic;
using Newtonsoft.Json;

namespace Model
{
    /// <summary>
    /// Enumeration for Message Level Type
    /// </summary>
    public enum MessageLevel
    {
        None = 0,
        Info = 1,
        Warning = 2,
        Error = 3
    }

    /// <summary>
    /// A generic update result class that allows for different result types to be included
    /// </summary>
    /// <typeparam name="T">The type of the result included in the update result</typeparam>
    public class UpdateResult<T> where T : class
    {
        /// <summary>
        /// The list of messages returned by the update method
        /// </summary>
        public IEnumerable<OperationMessageResult> Messages { get; set; }

        /// <summary>
        /// The result returned by the update method
        /// </summary>
        [JsonProperty(NullValueHandling = NullValueHandling.Include)]
        public T Result { get; set; }
    }

    // Found in: blueprint-current/Source/BluePrintSys.RC.Business.Internal/Components/Shared/Models/OperationMessageResult.cs
    /// <summary>
    /// The OperationMessageResult class used by the UpdateResult class. This defines a returned
    /// message and various descriptive properties related to the message.
    /// </summary>
    public class OperationMessageResult
    {
        /// <summary>
        /// The message level of the returned information message
        /// </summary>
        public MessageLevel Level { get; set; }

        /// <summary>
        /// The property type id of the item that the message is about
        /// </summary>
        public int PropertyTypeId { get; set; }

        /// <summary>
        /// The id of the item that the message is about
        /// </summary>
        public int ItemId { get; set; }

        /// <summary>
        /// The returned code number of the message
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// The content of the information message
        /// </summary>
        public string Message { get; set; }
    }
}