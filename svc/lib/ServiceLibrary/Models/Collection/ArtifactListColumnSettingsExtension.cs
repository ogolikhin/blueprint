using System.Linq;

namespace ServiceLibrary.Models.Collection
{
    public static class ArtifactListColumnsSettingsExtension
    {
        public static ArtifactListColumnsSettingsXml ConvertToArtifactListColumnsSettingsXmlModel(this ArtifactListColumnsSettings settings)
        {
            return new ArtifactListColumnsSettingsXml
            {
                Columns = settings.Columns.ToList()
            };
        }
    }
}
