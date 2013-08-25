using System;
using System.IO;
using System.Security.Cryptography;

namespace Sratim
{
    internal class PasswordUtility
    {
        private static readonly byte[] Entropy = System.Text.Encoding.Unicode.GetBytes("Your password is incorrect");

        public static int EncryptDataToStream(byte[] buffer, DataProtectionScope scope, Stream stream)
        {
            if (buffer.Length <= 0)
            {
                throw new ArgumentException("buffer");
            }
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }

            var length = 0;

            // Encrypt the data in memory. The result is stored in the same same array as the original data. 
            var encrptedData = ProtectedData.Protect(buffer, Entropy, scope);

            // Write the encrypted data to a stream. 
            if (stream.CanWrite)
            {
                stream.Write(encrptedData, 0, encrptedData.Length);

                length = encrptedData.Length;
            }

            // Return the length that was written to the stream.  
            return length;
        }

        public static byte[] DecryptDataFromStream(DataProtectionScope scope, Stream stream, int length)
        {
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (length <= 0)
            {
                throw new ArgumentException("length");
            }

            var inBuffer = new byte[length];
            byte[] outBuffer;

            // Read the encrypted data from a stream. 
            if (stream.CanRead)
            {
                stream.Read(inBuffer, 0, length);

                outBuffer = ProtectedData.Unprotect(inBuffer, Entropy, scope);
            }
            else
            {
                throw new IOException("Could not read the stream.");
            }

            // Return the length that was written to the stream.  
            return outBuffer;
        }
    }
}
