using System.Security.Cryptography;
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

            // Encript
            var encryptedPassword = PasswordUtility.EncryptData(generatedPassword, DataProtectionScope.LocalMachine);

            // Decript
            var decryptedPassword = PasswordUtility.DecryptData(encryptedPassword, DataProtectionScope.LocalMachine);

            Assert.Equals(generatedPassword, decryptedPassword);
        }
    }
}
