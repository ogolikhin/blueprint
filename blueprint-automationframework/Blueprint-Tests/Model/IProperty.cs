using Model.Impl;
using System.Collections.Generic;

namespace Model
{
    public interface IOpenApiProperty
    {
        int PropertyTypeId { get; set; }
        string Name { get; set; }
        string BasePropertyType { get; set; }
        string TextOrChoiceValue { get; set; }
        bool IsRichText { get; set; }
        bool IsReadOnly { get; set; }
        List<object> UsersAndGroups { get; }
        List<object> Choices { get; }
        string DateValue { get; set; }

        /// TODO: need to be updated for future script update
        /// <summary>
        /// Create a property object based on the information from DB </summary>
        /// <param name="project">project</param>
        /// <param name="propertyName">property Name</param>
        /// <param name="propertyValue">(optional) property Name</param>
        OpenApiProperty GetProperty(IProject project, string propertyName, string propertyValue = null);
    }
}
