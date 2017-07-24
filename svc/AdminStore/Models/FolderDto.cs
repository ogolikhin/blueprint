using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AdminStore.Models
{
    public class FolderDto
    {
        public string Name { get; set; }
        public int? ParentFolderId { get; set; }
    }
}