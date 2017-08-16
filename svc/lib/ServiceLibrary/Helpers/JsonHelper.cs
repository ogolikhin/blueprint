using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace ServiceLibrary.Helpers
{
    public static class JsonHelper
    {
        public static string ToJSON(this object o)
        {
            var sb = new StringBuilder();
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
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
