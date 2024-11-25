using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Security.Cryptography;
using System.Text;
using TMS_API.Utilities;

namespace TMS_API.Tests
{
    [TestClass]
    public class SecurityUtilsTests
    {
        private SecurityUtils _securityUtils = null!;
        
        [TestInitialize]
        public void Setup()
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
        public void GenerateCodeVerifier_ShouldReturnNonEmptyString()
        {
            string codeVerifier = _securityUtils.GenerateCodeVerifier(32);
            Assert.IsNotNull(codeVerifier);
            Assert.IsTrue(codeVerifier.Length > 0);
        }

        [TestMethod]
        public async Task GenerateCodeChallengerAsync_ShouldReturnNonEmptyString()
        {
            string codeVerifier = _securityUtils.GenerateCodeVerifier(32);
            string codeChallenger =  await _securityUtils.GenerateCodeChallengerAsync(codeVerifier);
            Console.WriteLine(codeChallenger);
            Assert.IsNotNull(codeChallenger);
            Assert.IsTrue(codeChallenger.Length > 0);
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