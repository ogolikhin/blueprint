using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AccessControl.Helpers
{
    /*
    This class is duplicated from blueprint-current BluePrintSys.RC.Data.AccessAPI.Utils.LicenceHelper.cs
    Please keep it in sync
    */
    internal static class LicenceHelper
    {
        private const string _password = "C0JmvK5akgKX5ybJ4De4zErKJGJ2Mx8ryncivWlFWF5L6bXWO31CIhDT9uBg2pBk";
        private const string _uTF8EncodingSaltString = "UCrSVx6btJxEuWdiRoLMWEzYloOYZWI3usfNtTmBNe0m8UzMRfAcNQZno99qm7GW";

        internal static int GetLicenseHoldTime(string encryptedLicenceHoldTimerString, int licenseHoldTimeDefault)
        {
            int licenceHoldTime;
            if (encryptedLicenceHoldTimerString == null)
            {
                licenceHoldTime = licenseHoldTimeDefault;
            }
            else
            {
                string decryptedString = "";
                try
                {
                    decryptedString = Decrypt(encryptedLicenceHoldTimerString);
                }
                catch (Exception)
                {
// Log.Error(exception);
                    licenceHoldTime = licenseHoldTimeDefault;
// Log.Info(String.Format("Invalid value for web.config key {0}. Using default.", licenseHoldTimeDefault));
                    return licenceHoldTime;
                }

                int result;
                if (int.TryParse(decryptedString, out result))
                {
                    licenceHoldTime = result;
                }
                else
                {
                    licenceHoldTime = licenseHoldTimeDefault;
                    // Log.Info(String.Format("Invalid resolved value for web.config key {0}. Using default.", licenseHoldTimeDefault));
                }
            }

            return licenceHoldTime;
        }

        // encrypt and decrypt methods stolen from BluePrintSys.RC.Data.AccessAPI.BackendEncryptions
        private static string Decrypt(string base64Input)
        {
            if (string.IsNullOrEmpty(base64Input))
            {
                return base64Input;
            }

            // byte[] encryptBytes = UTF8Encoding.UTF8.GetBytes(input);
            byte[] encryptBytes = Convert.FromBase64String(base64Input);
            byte[] saltBytes = Encoding.UTF8.GetBytes(_uTF8EncodingSaltString);

            // Our symmetric encryption algorithm
            AesManaged aes = new AesManaged();

            // We're using the PBKDF2 standard for password-based key generation
            Rfc2898DeriveBytes rfc = new Rfc2898DeriveBytes(_password, saltBytes);

            // Setting our parameters
            aes.BlockSize = aes.LegalBlockSizes[0].MaxSize;
            aes.KeySize = aes.LegalKeySizes[0].MaxSize;

            aes.Key = rfc.GetBytes(aes.KeySize / 8);
            aes.IV = rfc.GetBytes(aes.BlockSize / 8);

            // Now, decryption
            ICryptoTransform decryptTrans = aes.CreateDecryptor();

            // Output stream, can be also a FileStream
            MemoryStream decryptStream = new MemoryStream();
            CryptoStream decryptor = new CryptoStream(decryptStream, decryptTrans, CryptoStreamMode.Write);

            decryptor.Write(encryptBytes, 0, encryptBytes.Length);
            decryptor.Flush();
            decryptor.Close();

            // Showing our decrypted content
            byte[] decryptBytes = decryptStream.ToArray();
            string decryptedString = UTF8Encoding.UTF8.GetString(decryptBytes, 0, decryptBytes.Length);
            Debug.WriteLine(decryptedString);

            return decryptedString;
        }
    }
}