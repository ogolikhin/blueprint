
namespace ServiceLibrary.Models.PropertyType
{
    /// <summary>
    /// Contains error information for property update operation.
    /// </summary>
    public class PropertySetResult
    {
        #region Properties

        /// <summary>
        /// Gets or sets the property type id.
        /// </summary>
        public int PropertyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the error code.
        /// </summary>
        public int ErrorCode { get; set; }

        #endregion

        #region Construction

        /// <summary>
        /// Initializes a new instance of the <see cref="PropertySetResult"/> class.
        /// </summary>
        public PropertySetResult(int propertyTypeId, int errorCode, string message)
        {
            PropertyTypeId = propertyTypeId;
            ErrorCode = errorCode;
            Message = message;
        }

        #endregion
    }
}
