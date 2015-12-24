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
        private static Dictionary<string, IProperty> DefaultProperties()
        {
            Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();
            IProperty property = new Property();
            //first entry
            property.PropertyTypeId = 50;
            property.Name = "Name";
            property.BasePropertyType = "Text";
            property.TextOrChoiceValue = "DefaultValue_Name";
            property.IsRichText = false;
            property.IsReadOnly = false ;
            _properties.Add("Name", property);
            //second entry
            property.PropertyTypeId = 51;
            property.Name = "Description";
            property.BasePropertyType = "Text";
            property.TextOrChoiceValue = "DefaultValue_Description";
            property.IsRichText = true;
            property.IsReadOnly = false;
            _properties.Add("Description", property);

            return _properties;
        }

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
