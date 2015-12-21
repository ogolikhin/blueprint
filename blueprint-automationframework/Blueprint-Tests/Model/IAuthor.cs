using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IAuthor
    {
        string Type { get; set; }
        int Id { get; set; }
        string DisplayName { get; set; }
    }
}
