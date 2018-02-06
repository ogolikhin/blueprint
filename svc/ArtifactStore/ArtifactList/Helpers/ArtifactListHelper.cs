using System.Collections.Generic;
using System.Linq;
using ArtifactStore.ArtifactList.Models;
using ArtifactStore.ArtifactList.Models.Xml;
using ServiceLibrary.Models.ProjectMeta;

namespace ArtifactStore.ArtifactList.Helpers
{
    public static class ArtifactListHelper
    {
        public static List<XmlProfileColumn> ConvertProfileColumnsToXmlProfileSettings(ProfileColumns profileColumns) =>
            profileColumns.Items?
                .Select(column => new XmlProfileColumn
                {
                    PropertyName = column.PropertyName,
                    PropertyTypeId = column.PropertyTypeId,
                    Predefined = (int)column.Predefined,
                    PrimitiveType = (int)column.PrimitiveType
                })
                .ToList();

        public static int? ConvertXmlProfileSettingsToPaginationLimit(XmlProfileSettings settings) =>
            settings.PaginationLimit;

        public static ProfileColumns ConvertXmlProfileSettingsToProfileColumns(XmlProfileSettings settings) =>
            new ProfileColumns(
                settings.Columns?
                    .Select(xmlColumn => new ProfileColumn(
                        xmlColumn.PropertyName,
                        (PropertyTypePredefined)xmlColumn.Predefined,
                        (PropertyPrimitiveType)xmlColumn.PrimitiveType,
                        xmlColumn.PropertyTypeId))
                    .ToList());
    }
}
