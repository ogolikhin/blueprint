using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Helpers;
using ServiceLibrary.Repositories.ProjectMeta;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml;
using ServiceLibrary.Repositories.ProjectMeta.PropertyXml.Models;

namespace ServiceLibrary.Models.ProjectMeta.Sql
{
    public static class PropertyTypeVersionExtension
    {
        public static PropertyType ConvertToPropertyType(this SqlProjectMetaRepository.PropertyTypeVersion pv)
        {
            // Property XmlInfo is not supposed to be null, see bug 4819
            var propertyFromXml = pv.PrimitiveType == PropertyPrimitiveType.Choice
                ? XmlModelSerializer.DeserializeCustomProperties(pv.XmlInfo).CustomProperties[0]
                : null;

            return new PropertyType
            {
                Id = pv.PropertyTypeId,
                Name = pv.Name,
                VersionId = pv.VersionId,
                InstancePropertyTypeId = pv.InstancePropertyTypeId,
                PrimitiveType = pv.PrimitiveType,
                IsRichText = pv.PrimitiveType == PropertyPrimitiveType.Text ? pv.RichText : null,
                IsRequired = pv.Required,
                IsValidated = pv.PrimitiveType == PropertyPrimitiveType.Number
                                    || pv.PrimitiveType == PropertyPrimitiveType.Date
                                    || pv.PrimitiveType == PropertyPrimitiveType.Choice
                                    ? pv.Validate : null,
                IsMultipleAllowed = pv.PrimitiveType == PropertyPrimitiveType.Text
                                    || pv.PrimitiveType == PropertyPrimitiveType.Choice
                                    ? pv.AllowMultiple : null,
                StringDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Text ? pv.StringDefaultValue : null,
                DateDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Date ? pv.DateDefaultValue : null,
                DecimalDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.Number
                                      ? PropertyHelper.ToDecimal(pv.DecimalDefaultValue) : null,
                UserGroupDefaultValue = pv.PrimitiveType == PropertyPrimitiveType.User
                                      ? PropertyHelper.ParseUserGroups(pv.UserDefaultValue) : null,
                MinDate = pv.PrimitiveType == PropertyPrimitiveType.Date && pv.Validate.GetValueOrDefault() ? pv.MinDate : null,
                MaxDate = pv.PrimitiveType == PropertyPrimitiveType.Date && pv.Validate.GetValueOrDefault() ? pv.MaxDate : null,
                MinNumber = pv.PrimitiveType == PropertyPrimitiveType.Number && pv.Validate.GetValueOrDefault()
                                      ? PropertyHelper.ToDecimal(pv.MinNumber) : null,
                MaxNumber = pv.PrimitiveType == PropertyPrimitiveType.Number && pv.Validate.GetValueOrDefault()
                                      ? PropertyHelper.ToDecimal(pv.MaxNumber) : null,
                DecimalPlaces = pv.PrimitiveType == PropertyPrimitiveType.Number ? pv.DecimalPlaces : null,
                ValidValues = pv.PrimitiveType == PropertyPrimitiveType.Choice
                                      ? propertyFromXml?.ValidValues.OrderBy(v => I18NHelper.Int32ParseInvariant(v.OrderIndex))
                                      .Select(v =>
                                      {
                                          int? vvId = null;
                                          if (!string.IsNullOrWhiteSpace(v.LookupListItemId))
                                          {
                                              int intValue;
                                              if (int.TryParse(v.LookupListItemId, out intValue))
                                                  vvId = intValue;
                                          }
                                          return new ValidValue { Id = vvId, Value = v.Value };
                                      }).ToList()
                                      : null,
                DefaultValidValueId = pv.PrimitiveType == PropertyPrimitiveType.Choice
                                      ? FindDefaultValidValueId(propertyFromXml.ValidValues) // TODO
                                      : null,
                BaseArtifactTypeId = pv.BaseArtifactTypeId < 1 ? null : (int?)pv.BaseArtifactTypeId
            };
        }

        private static int? FindDefaultValidValueId(List<XmlCustomPropertyValidValue> validValues)
        {
            if (validValues == null)
                return null;

            var orderedValidValues = validValues.OrderBy(v => I18NHelper.Int32ParseInvariant(v.OrderIndex)).ToList();
            for (var i = 0; i < orderedValidValues.Count; i++)
            {
                var validValue = orderedValidValues.ElementAt(i);
                if (validValue?.Selected == "1")
                {
                    int? vvId = null;
                    if (!string.IsNullOrWhiteSpace(validValue.LookupListItemId))
                    {
                        int intValue;
                        if (int.TryParse(validValue.LookupListItemId, out intValue))
                            vvId = intValue;
                    }
                    return vvId;
                }
            }

            return null;
        }
    }
}
