using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Buffers;
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
        Task<(byte[] encryptedText, byte[] iv)> EncryptPlaintTextAsync(string plaintext, byte[] Key, byte[]? IV = null);
        Task<string> DecryptPlainTextAsync(byte[] cipherBytes, byte[] key, byte[] IV);
        ValueTask<byte[]> GenerateHashToken(string token);

    }

    public class SecurityUtils : ISecurityUtils
    {
        private readonly Argon2Settings _options;
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly ArrayPool<byte> _bytesPool = ArrayPool<byte>.Shared;
        private static readonly SHA256 _sha256Hash = SHA256.Create();
        public SecurityUtils(IOptions<Argon2Settings> options)
        {
            _options = options.Value ?? throw new ArgumentNullException(nameof(options));
        }

        public byte[] GenerateSalt()
        {   
            byte[] buffer = _bytesPool.Rent(_options.Storage);
            try
            {
                _rng.GetBytes(buffer, 0, _options.Storage);
                return buffer;
            }
            finally
            {
                _bytesPool.Return(buffer, clearArray: true);
            }
        }

        public byte[] GenerateHash(string plaintext, byte[] salt)
        {
            using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(plaintext)))
            {
                argon2.Salt = salt;
                argon2.DegreeOfParallelism = _options.Parallelism;
                argon2.MemorySize = _options.Memory;
                argon2.Iterations = _options.Iterations;
                return argon2.GetBytes(_options.Storage);
            }
            
        }

        public string GenerateAPIKey()
        {
            byte[] randomBytes = _bytesPool.Rent(32);

            try
            {
                _rng.GetBytes(randomBytes, 0, 32);
                return "TMS-" + Base64UrlEncoder.Encode(randomBytes);
            }
            finally
            {
                _bytesPool.Return(randomBytes, clearArray: true);
            }
        }

        public string GenerateRefreshToken(int length = 64)
        {
            byte[] randomBytes = _bytesPool.Rent(length);
            try
            {
                _rng.GetBytes(randomBytes, 0, length);
                return Convert.ToBase64String(randomBytes);
            }
            finally
            {
                _bytesPool.Return(randomBytes, clearArray: true);
            }
        }

        public async Task<(byte[] encryptedText, byte[] iv)> EncryptPlaintTextAsync(string plaintext, byte[] Key, byte[]? IV = null)
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
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        await streamWriter.WriteAsync(plaintext);
                    }
                    return (memoryStream.ToArray(), aes.IV);
                }
            }
            
        }

        public async Task<string> DecryptPlainTextAsync(byte[] cipherBytes, byte[] key, byte[] IV)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = IV;

                using (ICryptoTransform cryptoTransform = aes.CreateDecryptor(aes.Key, aes.IV))
                using (MemoryStream memoryStream = new MemoryStream(cipherBytes))
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read))
                using (StreamReader streamReader = new StreamReader(cryptoStream, Encoding.UTF8))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public ValueTask<byte[]> GenerateHashToken(string token)
        {
            byte[] tokenBytes = Encoding.UTF8.GetBytes(token);

            byte[] hash = _sha256Hash.ComputeHash(tokenBytes);

            return new ValueTask<byte[]>(hash);
        }
    }
}