using System;

namespace ServiceLibrary.Helpers
{
    public static class PropertyHelper
    {
        // Convert the byte array of the number property stored in the database to decimal.
        public static decimal? ToDecimal(byte[] value)
        {
            if (value == null)
            {
                return null;
            }
            int[] bits = { BitConverter.ToInt32(value, 0), BitConverter.ToInt32(value, 4), BitConverter.ToInt32(value, 8), BitConverter.ToInt32(value, 12) };
            return new decimal(bits);
        }
        public static byte[] GetBytes(decimal? value)
        {
            if (value == null)
            {
                return null;
            }
            byte[] bytes = new byte[16];
            int[] bits = Decimal.GetBits((decimal)value);
            Array.Copy(BitConverter.GetBytes(bits[0]), 0, bytes, 0, 4);
            Array.Copy(BitConverter.GetBytes(bits[1]), 0, bytes, 4, 4);
            Array.Copy(BitConverter.GetBytes(bits[2]), 0, bytes, 8, 4);
            Array.Copy(BitConverter.GetBytes(bits[3]), 0, bytes, 12, 4);
            return bytes;
        }
    }
}