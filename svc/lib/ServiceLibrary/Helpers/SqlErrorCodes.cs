using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public enum SqlErrorCodes
    {
        None = 0,
        GeneralSqlError = 50000,
        UserLoginExist = 50001
    }
}
