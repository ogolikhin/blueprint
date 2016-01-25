using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model
{
    public interface IACLicensesInfo
    {
        int LicenseLevel { get; set; }
        int Count { get; set; }
    }
}
