using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Model;
using Model.Impl;

namespace Helper.Factories
{
    public class PropertyFactory
    {
        private static Dictionary<string, IProperty> DefaultProperties()
        {
            Dictionary<string, IProperty> _properties = new Dictionary<string, IProperty>();
            IProperty _property = new Property();
            //first entry
            _property.PropertyTypeId = 50;
            _property.Name = "Name";
            _property.BasePropertyType = "Text";
            _property.TextOrChoiceValue = "DefaultValue_Name";
            _property.IsRichText = false;
            _property.IsReadOnly = false ;
            _properties.Add("Name", _property);
            //second entry
            _property.PropertyTypeId = 51;
            _property.Name = "Description";
            _property.BasePropertyType = "Text";
            _property.TextOrChoiceValue = "DefaultValue_Description";
            _property.IsRichText = true;
            _property.IsReadOnly = false;
            _properties.Add("Description", _property);

            return _properties;
        }

        public static List<IProperty> AddProperty(String propertyName, string propertyTextOrChoiceValue = null)
        {
            List<IProperty> _properties = new List<IProperty>();
            Dictionary<string, IProperty> defaultProperties = DefaultProperties();
            if (propertyTextOrChoiceValue != null)
            {
                defaultProperties[propertyName].TextOrChoiceValue = propertyTextOrChoiceValue;
            }
            _properties.Add(defaultProperties[propertyName]);
            return _properties;
        }
    }
}
