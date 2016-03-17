using System.Collections.Generic;

namespace Model
{
    /// <summary>
    /// Enumeration for Message Level Type
    /// </summary>
    public enum MessageLevel
    {
        Info,
        Warning,
        Error
    }

    /// <summary>
    /// A generic update result interface that allows for different Item types to be included
    /// </summary>
    /// <typeparam name="T">The type of the item included in the update result</typeparam>
    public interface IUpdateResult<T> where T : class
    {
        IEnumerable<UpdateInformation> UpdateInfos { get; set; }

        T item { get; set; }
    }

    /// <summary>
    /// The UpdateInformation class used by the IUpdateResult interface
    /// </summary>
    public class UpdateInformation
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