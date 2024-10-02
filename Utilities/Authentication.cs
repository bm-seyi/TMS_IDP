using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

using TMS_API.Configuration;

namespace TMS_API.Utilities
{
    public interface ISecurityUtils
    {
        byte[] GenerateSalt();
        byte[] GenerateHash(string password, byte[] salt);
        string GenerateAPIKey();
        string GenerateRefreshToken(int length = 64);
        (byte[] encryptedText, byte[] iv) EncryptPlaintText(string plaintext, byte[] Key, byte[]? IV = null);
        string DecryptPlainText (byte[] cipherBytes, byte[] key, byte[] IV);
        byte[] GenerateHashToken(string token);

    }

    public class SecurityUtils : ISecurityUtils
    {
        private readonly Argon2Settings _options;
        public SecurityUtils(IOptions<Argon2Settings> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public byte[] GenerateSalt()
        {   
            var buffer = new byte[_options.Storage];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }
            return buffer;
        }

        public byte[] GenerateHash(string plaintext, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(plaintext)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = _options.Parallelism;
                argon2.MemorySize = _options.Memory;
                argon2.Iterations = _options.Iterations;
                argon2.AssociatedData = null;
                argon2.KnownSecret = null;
                
                return argon2.GetBytes(_options.Storage);
            }
            
        }

        public string GenerateAPIKey()
        {
            byte[] storage = new byte[32];

            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(storage);
            }

            string key = Base64UrlEncoder.Encode(storage);

            return "TMS-" + key;
        }

        public string GenerateRefreshToken(int length = 64)
        {
            var randomBytes = new byte[length];
            using (var generator = RandomNumberGenerator.Create())
            {
                generator.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        public (byte[] encryptedText, byte[] iv) EncryptPlaintText(string plaintext, byte[] Key, byte[]? IV = null)
        {
            using (Aes aes = Aes.Create())
            {
                aes.KeySize = 256;
                aes.Key = Key;

                if (IV != null) aes.IV = IV;
                else aes.GenerateIV();

                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform cryptoTransform = aes.CreateEncryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                    {
                        using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                        {
                            streamWriter.Write(plaintext);
                        }
                        return (memoryStream.ToArray(), aes.IV);
                    }
                }
            }
        }

        public string DecryptPlainText (byte[] cipherBytes, byte[] key, byte[] IV)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;

                ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV);

                using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                    {
                        using (StreamReader streamWriter = new StreamReader(cryptoStream))
                        {
                            return streamWriter.ReadToEnd();
                        }
                    }
                }
            }
        }

        public byte[] GenerateHashToken(string token)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(token));
                return bytes;
            }
        }
    }
}