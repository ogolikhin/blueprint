using System.Collections.Generic;
using System.Linq;

namespace ServiceLibrary.Models
{
    public class Icon
    {
        public IEnumerable<byte> Content { get; set; }

        public bool IsSvg { get; set; }
    }
}
