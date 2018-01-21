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
    }
}
