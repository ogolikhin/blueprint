using System;
using System.Security.Cryptography;
using System.Text;

namespace Model.Impl
{
    // NOTE: This class was copied from the blueprint-current.git repo (and slightly modified).
    public static class HashingUtilities
    {
        /// <summary>
        /// Encrypts a password using the specified salt.
        /// </summary>
        /// <param name="plainText">The password to encrypt.</param>
        /// <param name="salt">The salt to use for the hash.</param>
        /// <returns>The encrypted password.</returns>
        public static string GenerateSaltedHash(string plainText, string salt)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] saltBytes = Encoding.UTF8.GetBytes(salt);

            using (HashAlgorithm algorithm = new SHA256Managed())
            {
                byte[] plainTextWithSaltBytes = new byte[plainTextBytes.Length + saltBytes.Length];

                for (int i = 0; i < plainTextBytes.Length; i++)
                {
                    plainTextWithSaltBytes[i] = plainTextBytes[i];
                }

                for (int i = 0; i < saltBytes.Length; i++)
                {
                    plainTextWithSaltBytes[plainTextBytes.Length + i] = saltBytes[i];
                }

                byte[] hash = algorithm.ComputeHash(plainTextWithSaltBytes);

                return Convert.ToBase64String(hash);
            }
        }
    }
}
