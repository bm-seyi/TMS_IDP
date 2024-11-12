using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
using TMS_API.Utilities;

namespace TMS_API.Tests
{
    [TestClass]
    public class SecurityUtilsTests
    {
        private readonly SecurityUtils _securityUtils;

        public SecurityUtilsTests()
        {
            _securityUtils = new SecurityUtils();
        }

        [TestMethod]
        public void GenerateAPIKey_ShouldReturnNonNull()
        {
            string apiKey = _securityUtils.GenerateAPIKey();

            Assert.IsNotNull(apiKey);
            Assert.IsTrue(apiKey.StartsWith("TMS-"));
            Assert.IsTrue(apiKey.Length > 0);
        }

        [TestMethod]
        public void GenerateRefreshToken_ShouldReturnNonEmptyString()
        {
            string refreshToken = _securityUtils.GenerateRefreshToken();
            
            Assert.IsNotNull(refreshToken);
            Assert.IsTrue(refreshToken.Length > 0);
        }

        [TestMethod]
        public async Task EncryptPlaintTextAsync_ShouldReturnCipherText()
        {
            string plainText = "TestText";
            byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");

            byte[] cipherText = await _securityUtils.EncryptPlaintTextAsync(plainText, key);

            Assert.IsNotNull(cipherText);
            Assert.IsTrue(cipherText.Length > 0);
            Assert.IsTrue(cipherText.Length > plainText.Length);
        }

        [TestMethod]
        public async Task DecryptPlainTextAsync_ShouldReturnOriginalText()
        {
            string plainText = "TestText";
            byte[] key = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");

            byte[] cipherText = await _securityUtils.EncryptPlaintTextAsync(plainText, key);
            string decryptedText = await _securityUtils.DecryptPlainTextAsync(cipherText, key);

            Assert.IsNotNull(decryptedText);
            Assert.AreEqual(plainText, decryptedText);
        }

        [TestMethod]
        public async Task DecryptPlainTextAsync_WithInvalidKey_ShouldThrowException()
        {

            string plainText = "TestText";
            byte[] validKey = Encoding.UTF8.GetBytes("0123456789ABCDEF0123456789ABCDEF");
            byte[] invalidKey = Encoding.UTF8.GetBytes("FEDCBA9876543210FEDCBA9876543210");

            byte[] cipherText = await _securityUtils.EncryptPlaintTextAsync(plainText, validKey);

            await Assert.ThrowsExceptionAsync<CryptographicException>(() => _securityUtils.DecryptPlainTextAsync(cipherText, invalidKey));
        }

        [TestMethod]
        public async Task GenerateHashToken_ShouldReturnHashOfExpectedLength()
        {

            string token = "TestToken";

            byte[] hash = await _securityUtils.GenerateHashToken(token);

            Assert.IsNotNull(hash);
            Assert.AreEqual(32, hash.Length);
        }
    }
}