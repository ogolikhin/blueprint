using System.Globalization;

namespace ServiceLibrary.Repositories.ProjectMeta.PropertyXml
{
    public static class XmlModelConvert
    {

        public static string FromInt32(int value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static int ToInt32(string value)
        {
            return int.Parse(value, NumberFormatInfo.InvariantInfo);
        }

        public static string FromNullableInt32(int? value)
        {
            return value.HasValue ? value.Value.ToString(NumberFormatInfo.InvariantInfo) : null;
        }

        public static int? ToNullableInt32(string value)
        {
            return (value != null) ? int.Parse(value, NumberFormatInfo.InvariantInfo) : null as int?;
        }

        public static string FromBoolean(bool value)
        {
            return (value ? 1 : 0).ToString(NumberFormatInfo.InvariantInfo);
        }

        public static bool ToBoolean(string value)
        {
            return int.Parse(value, NumberFormatInfo.InvariantInfo) != 0;
        }

        public static string FromDouble(double value)
        {
            return value.ToString(NumberFormatInfo.InvariantInfo);
        }

        public static double ToDouble(string value)
        {
            return double.Parse(value, NumberFormatInfo.InvariantInfo);
        }
    }
}
