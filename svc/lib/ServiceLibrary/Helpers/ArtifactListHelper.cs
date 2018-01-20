using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models.Collection;

namespace ServiceLibrary.Helpers
{
    public static class ArtifactListHelper
    {
        public static XmlProfileSettings ConvertProfileColumnsSettingsToXmlProfileSettings(ProfileColumnsSettings profileColumnsSettings)
        {
            return new XmlProfileSettings { Columns = profileColumnsSettings.Columns.ToList() };
        }
    }
}
