using System;

namespace ServiceLibrary.Helpers
{
    public static class PropertyHelper
    {
        // Convert the byte array of the number property stored in the database to decimal.
        internal static decimal? ToDecimal(byte[] value)
        {
            if (value == null)
            {
                return null;
            }
            int[] bits = { BitConverter.ToInt32(value, 0), BitConverter.ToInt32(value, 4), BitConverter.ToInt32(value, 8), BitConverter.ToInt32(value, 12) };
            return new decimal(bits);
        }
    }
}