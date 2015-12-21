using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IAttachment
    {
        int Id { get; set; }
        string FileName { get; set; }
        string Link { get; set; }
        bool IsReadOnly { get; set; }
    }
}
