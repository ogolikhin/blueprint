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

            var algorithm = new SHA256Managed();
            var plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];
            for (var i = 0; i < plainTextBytes.Length; i++)
            {
                plainTextWithSaltBytes[i] = plainTextBytes[i];
            }
            for (int i = 0; i < saltBytes.Length; i++)
            {
                plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
            }
            var hash = algorithm.ComputeHash(plainTextWithSaltBytes);

            return Convert.ToBase64String(hash);
        }

        public static string GenerateSaltedHash(string plainText, Guid salt)
        {
            return GenerateSaltedHash(plainText, salt.ToString());
        }
    }
}