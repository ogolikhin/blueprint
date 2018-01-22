using System.Linq;
using ArtifactStore.ArtifactList.Models;

namespace ArtifactStore.ArtifactList.Helpers
{
    public static class ArtifactListHelper
    {
        public static XmlProfileSettings ConvertProfileColumnsSettingsToXmlProfileSettings(
            ProfileColumnsSettings columnSettings)
        {
            return new XmlProfileSettings
            {
                Columns = columnSettings.Items.ToList()
            };
        }

        public static ProfileColumnsSettings ConvertXmlProfileSettingsToProfileColumnSettings(
            XmlProfileSettings settings)
        {
            return new ProfileColumnsSettings
            {
                Items = settings.Columns.ToList()
            };
        }
    }
}
