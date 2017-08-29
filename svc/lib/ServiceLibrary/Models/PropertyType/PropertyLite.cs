using System;
using System.Collections.Generic;

namespace ServiceLibrary.Models.PropertyType
{
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

}
