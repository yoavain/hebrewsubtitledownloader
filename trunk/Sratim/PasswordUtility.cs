using System;
using System.Security.Cryptography;

namespace Sratim
{
    public class PasswordUtility
    {
        private static readonly byte[] Entropy = System.Text.Encoding.Unicode.GetBytes("Your password is incorrect");

        public static byte[] EncryptData(byte[] decryptedData, DataProtectionScope scope)
        {
            if (decryptedData == null)
            {
                throw new ArgumentNullException("decryptedData");
            }
            if (decryptedData.Length <= 0)
            {
                throw new ArgumentException("decryptedData");
            }

            // Encrypt the data
            var encrptedData = ProtectedData.Protect(decryptedData, Entropy, scope);

            // Return encyrpted data
            return encrptedData;
        }

        public static byte[] DecryptData(byte[] encrptedData, DataProtectionScope scope)
        {
            if (encrptedData == null)
            {
                throw new ArgumentNullException("encrptedData");
            }
            if (encrptedData.Length <= 0)
            {
                throw new ArgumentException("encrptedData");
            }

            // Decrypt the data
            byte[] decryptedData = ProtectedData.Unprotect(encrptedData, Entropy, scope);

            // Return decrypted data
            return decryptedData;
        }
    }
}
