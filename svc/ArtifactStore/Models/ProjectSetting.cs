using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtifactStore.Models
{
    public class ProjectSetting
    {
        public int ProjectSettingId { get; set; }
        public int ProjectId { get; set; }
        public bool ReadOnly { get; set; }
        public PropertyTypePredefined Predefined { get; set; }
        public string Setting { get; set; }
        public int OrderIndex { get; set; }
        public bool Deleted { get; set; }
    }
}
