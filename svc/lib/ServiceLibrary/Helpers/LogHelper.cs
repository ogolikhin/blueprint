using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLibrary.Helpers
{
    public class LogHelper
    {
        public static string GetStackTrace(Exception ex)
        {
            var stringBuilder = new StringBuilder();

            while (ex != null)
            {
                stringBuilder.AppendLine(ex.Message);
                stringBuilder.AppendLine(ex.StackTrace);

                ex = ex.InnerException;
            }

            return stringBuilder.ToString();
        }

    }
}
