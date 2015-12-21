using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Impl
{
    public class Author : IAuthor
    {
        public string Type { get; set; }
        public int Id { get; set; }
        public string DisplayName { get; set; }
    }
}
