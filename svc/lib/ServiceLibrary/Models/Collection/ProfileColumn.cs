using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ServiceLibrary.Models.ProjectMeta;

namespace ServiceLibrary.Models.Collection
{
    public class ProfileColumn
    {
        public string PropertyName { get; set; }

        public int? PropertyTypeId { get; set; }

        public int Predefined { get; set; }
    }
}
