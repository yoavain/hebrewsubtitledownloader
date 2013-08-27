using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Security;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SratimUtils;

namespace HebrewSubtitleDownloaderTest
{
    [TestClass]
    public class PasswordUtilityTest
    {
        [TestMethod]
        public void TestRandomEncryptDecrypt()
        {
            // Generate password
            var generatedPassword = Membership.GeneratePassword(12, 0);
            var passwordBytes = Encoding.ASCII.GetBytes(generatedPassword);

            // Encript
            var encryptedData = PasswordUtility.EncryptData(passwordBytes, DataProtectionScope.LocalMachine);

            // Decript
            var decryptedData = PasswordUtility.DecryptData(encryptedData, DataProtectionScope.LocalMachine);

            Assert.IsTrue(passwordBytes.SequenceEqual(decryptedData));
        }
    }
}
