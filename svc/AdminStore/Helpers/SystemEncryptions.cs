using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AdminStore.Helpers
{
    /// <summary>
    /// Provides encryption for both Client and Server sides. There is also another class:BackendEncryptions that
    /// is supposed to be availabled to ecnrypt/decrypt on server side only.
    /// </summary>
    public static class SystemEncryptions
    {
        private const string Utf8EncodingSaltString = "UTF8EncodingSalt-RaptorRocks-Backend";
        private const string Utf8EncodingSaltStringSilverlight = "UTF8EncodingSalt-RaptorRocks";

        public static string Encrypt(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }

            // Test data
            var data = input;
            var utfdata = Encoding.UTF8.GetBytes(data);
            var saltBytes = Encoding.UTF8.GetBytes(Utf8EncodingSaltString);

            // Our symmetric encryption algorithm
            var aes = new AesManaged();

            // We're using the PBKDF2 standard for password-based key generation
            var rfc = new Rfc2898DeriveBytes("thePassword", saltBytes);

            // Setting our parameters
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;

            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // Encryption
            var encryptTransf = aes.CreateEncryptor();

            // Output stream, can be also a FileStream
            var encryptStream = new MemoryStream();
            var encryptor = new CryptoStream(encryptStream, encryptTransf, CryptoStreamMode.Write);

            encryptor.Write(utfdata, 0, utfdata.Length);
            encryptor.Flush();
            encryptor.Close();

            // Showing our encrypted content
            var encryptBytes = encryptStream.ToArray();
            //string encryptedString = UTF8Encoding.UTF8.GetString(encryptBytes, 0, encryptBytes.Length);
            var encryptedString = Convert.ToBase64String(encryptBytes);

            return encryptedString;
        }

        public static string Decrypt(string base64Input)
        {
            return DecryptInternal(base64Input, Utf8EncodingSaltString);
        }

        public static string DecryptFromSilverlight(string base64Input)
        {
            return DecryptInternal(base64Input, Utf8EncodingSaltStringSilverlight);
        }

        private static string DecryptInternal(string base64Input, string salt)
        {
            if (string.IsNullOrEmpty(base64Input))
            {
                return base64Input;
            }

            var encryptBytes = Convert.FromBase64String(base64Input);
            var saltBytes = Encoding.UTF8.GetBytes(salt);

            // Our symmetric encryption algorithm
            var aes = new AesManaged();

            // We're using the PBKDF2 standard for password-based key generation
            var rfc = new Rfc2898DeriveBytes("thePassword", saltBytes);

            // Setting our parameters
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;

            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // Now, decryption
            var decryptTrans = aes.CreateDecryptor();

            // Output stream, can be also a FileStream
            var decryptStream = new MemoryStream();
            var decryptor = new CryptoStream(decryptStream, decryptTrans, CryptoStreamMode.Write);

            decryptor.Write(encryptBytes, 0, encryptBytes.Length);
            decryptor.Flush();
            decryptor.Close();

            // Showing our decrypted content
            var decryptBytes = decryptStream.ToArray();
            var decryptedString = Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);

            return decryptedString;
        }

        public static string Decode(string input)
        {
            return input == null ? null : Encoding.UTF8.GetString(Convert.FromBase64String(input));
        }

        public static string EncodeTo64UTF8(string m_enc)
        {
            byte[] toEncodeAsBytes =
            System.Text.Encoding.UTF8.GetBytes(m_enc);
            string returnValue =
            System.Convert.ToBase64String(toEncodeAsBytes);
            return returnValue;
        }

        public static Guid CreateCryptographicallySecureGuid()
        {
            //http://stackoverflow.com/questions/37170388/create-a-cryptographically-secure-random-guid-in-net/37170593
            using (var provider = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[16];
                provider.GetBytes(bytes);

                return new Guid(bytes);
            }
        }
    }
}
