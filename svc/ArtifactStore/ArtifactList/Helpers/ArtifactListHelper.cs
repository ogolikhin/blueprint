using System.Linq;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.ArtifactList.Models.Xml;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Helpers
{
    public static class ArtifactListHelper
    {
        public static XmlProfileSettings ConvertProfileColumnsSettingsToXmlProfileSettings(
            ProfileColumnsSettings columnSettings)
        {
            return new XmlProfileSettings
            {
                Columns = columnSettings.Items?
                    .Select(column => new XmlProfileColumn
                    {
                        PropertyName = column.PropertyName,
                        PropertyTypeId = column.PropertyTypeId,
                        Predefined = (int)column.Predefined,
                        PrimitiveType = (int)column.PrimitiveType
                    })
                    .ToList()
            };
        }

        public static ProfileColumnsSettings ConvertXmlProfileSettingsToProfileColumnSettings(
            XmlProfileSettings settings)
        {
            return new ProfileColumnsSettings
            {
                Items = settings.Columns?
                    .Select(xmlColumn => new ProfileColumn(
                        xmlColumn.PropertyName,
                        (PropertyTypePredefined)xmlColumn.Predefined,
                        (PropertyPrimitiveType)xmlColumn.PrimitiveType,
                        xmlColumn.PropertyTypeId))
                    .ToList()
            };
        }
    }
}
