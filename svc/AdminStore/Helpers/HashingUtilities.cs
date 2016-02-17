using System;
using System.Security.Cryptography;
using System.Text;

namespace AdminStore.Helpers
{
    public class HashingUtilities
    {
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            var plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];
            Buffer.BlockCopy(plainTextBytes, 0, plainTextWithSaltBytes, 0, plainTextBytes.Length);
            Buffer.BlockCopy(saltBytes, 0, plainTextWithSaltBytes, plainTextBytes.Length, saltBytes.Length);
            var hash = new SHA256Managed().ComputeHash(plainTextWithSaltBytes); // Use a new instance each time for thread safety

            return Convert.ToBase64String(hash);
        }

        public static string GenerateSaltedHash(string plainText, Guid salt)
        {
            return GenerateSaltedHash(plainText, salt.ToString());
        }
    }
}
