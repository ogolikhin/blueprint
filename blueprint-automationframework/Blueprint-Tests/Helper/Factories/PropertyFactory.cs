using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Impl;

namespace Helper.Factories
{

    public static class PropertyFactory
    {

        /// <summary>
        /// Class that contains default value for standard properties
        /// Currently contains minimum default property values to create a simple artifact: "Name" and "Desciption" properties
        /// Next update will take these value from following view from Blueprint DB: dbo.TipPropertyTypesView
        /// </summary>
        private static Dictionary<string, IProperty> DefaultProperties()
        {
            Dictionary<string, IProperty> properties = new Dictionary<string, IProperty>();
            IProperty property = new Property();
            //first entry: set default value for the "Name" Property
            property.PropertyTypeId = 50;
            property.Name = "Name";
            property.BasePropertyType = "Text";
            property.TextOrChoiceValue = "DefaultValue_Name";
            property.IsRichText = false;
            property.IsReadOnly = false ;
            properties.Add("Name", property);
            //second entry: set default value for the "Description" Property
            property.PropertyTypeId = 51;
            property.Name = "Description";
            property.BasePropertyType = "Text";
            property.TextOrChoiceValue = "DefaultValue_Description";
            property.IsRichText = true;
            property.IsReadOnly = false;
            properties.Add("Description", property);
            return properties;
        }

        /// <summary>
        /// Adding a property instance into property list
        /// </summary>
        /// <param name="propertyName">property name</param>
        /// <param name="propertyTextOrChoiceValue">(Optional) property value</param>
        public static List<IProperty> AddProperty(String propertyName, string propertyTextOrChoiceValue = null)
        {
            List<IProperty> properties = new List<IProperty>();
            Dictionary<string, IProperty> defaultProperties = DefaultProperties();
            if (propertyTextOrChoiceValue != null)
            {
                defaultProperties[propertyName].TextOrChoiceValue = propertyTextOrChoiceValue;
            }
            properties.Add(defaultProperties[propertyName]);
            return properties;
        }

    }

}
