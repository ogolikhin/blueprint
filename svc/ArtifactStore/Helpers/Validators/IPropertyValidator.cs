using System;
using System.Collections.Generic;
using ServiceLibrary.Models;

namespace ArtifactStore.Helpers.Validators
{
    interface IPropertyValidator
    {
        PropertySetResult Validate(PropertyLite property, object context);
    }

    public class PropertyLite
    {
        /// <summary>
        /// Gets or sets the property type id.
        /// </summary>
        public int PropertyTypeId { get; set; }

        /// <summary>
        /// Gets or sets the text or choice value.
        /// </summary>
        public string TextOrChoiceValue { get; set; }

        /// <summary>
        /// Gets or sets the number value.
        /// </summary>
        public decimal? NumberValue { get; set; }

        /// <summary>
        /// Gets or sets the date value.
        /// </summary>
        public DateTime? DateValue { get; set; }

        /// <summary>
        /// Gets or sets the users and groups.
        /// </summary>
        public List<UserGroup> UsersAndGroups { get; } = new List<UserGroup>();

        /// <summary>
        /// Gets or sets the choices.
        /// </summary>
        public List<string> Choices { get; } = new List<string>();
    }

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

    //public class ValidationContext
    //{
    //    #region Properties

    //    /// <summary>
    //    /// Gets all available users.
    //    /// </summary>
    //    public IEnumerable<DUser> Users { get; private set; }

    //    /// <summary>
    //    /// Gets all available groups.
    //    /// </summary>
    //    public IEnumerable<DGroup> Groups { get; private set; }

    //    /// <summary>
    //    /// Gets the property types map.
    //    /// </summary>
    //    public IReadOnlyDictionary<int, DPropertyType> PropertyTypesMap { get; private set; }

    //    /// <summary>
    //    /// Gets the fake property types map.
    //    /// </summary>
    //    public IReadOnlyDictionary<int, DPropertyType> FakePropertyTypesMap { get; private set; }

    //    /// <summary>
    //    /// Reuse Settings Template
    //    /// </summary>
    //    public ReuseContext ReuseContext { get; private set; }

    //    #endregion

    //    #region Constrcution

    //    /// <summary>
    //    /// Initializes a new instance of the <see cref="ValidationContext"/> class.
    //    /// </summary>
    //    public ValidationContext(IEnumerable<UserGroup> usersAndGroups, IDictionary<int, DPropertyType> propertyTypesMap, IDictionary<int, DPropertyType> fakePropertyTypesMap, ReuseContext reuseContext = null)
    //    {
    //        Users = users.ToList().AsReadOnly();
    //        Groups = groups.ToList().AsReadOnly();
    //        PropertyTypesMap = new ReadOnlyDictionary<int, DPropertyType>(propertyTypesMap);
    //        FakePropertyTypesMap = new ReadOnlyDictionary<int, DPropertyType>(fakePropertyTypesMap);
    //        ReuseContext = reuseContext;
    //    }

    //    #endregion
    //}
}