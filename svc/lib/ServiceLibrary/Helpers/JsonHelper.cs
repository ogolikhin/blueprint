using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServiceLibrary.Helpers
{
    public static class JsonHelper
    {
        public static string ToJSON(this object o)
        {

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
                using (var writer = new JsonTextWriter(sw))
                {
                    writer.QuoteChar = '\'';
                    JsonSerializer ser = new JsonSerializer();
                    ser.Serialize(writer, o);
                }

            return sb.ToString();
        }
    }
}
