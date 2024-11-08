using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Buffers;

namespace TMS_API.Utilities
{
    public interface ISecurityUtils
    {
        string GenerateAPIKey();
        string GenerateRefreshToken(int length = 64);
        Task<byte[]> EncryptPlaintTextAsync(string plaintext, byte[] Key, byte[]? IV = null);
        Task<string> DecryptPlainTextAsync(byte[] cipherBytes, byte[] key);
        ValueTask<byte[]> GenerateHashToken(string token);

    }

    public class SecurityUtils : ISecurityUtils
    {
        private static readonly RandomNumberGenerator _rng = RandomNumberGenerator.Create();
        private static readonly ArrayPool<byte> _bytesPool = ArrayPool<byte>.Shared;
        private static readonly SHA256 _sha256Hash = SHA256.Create();

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

        public async Task<byte[]> EncryptPlaintTextAsync(string plaintext, byte[] Key, byte[]? IV = null)
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
                    memoryStream.Write(aes.IV, 0, aes.IV.Length);
                    using (CryptoStream cryptoStream = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write))
                    using (StreamWriter streamWriter = new StreamWriter(cryptoStream))
                    {
                        await streamWriter.WriteAsync(plaintext);
                    }
                    return memoryStream.ToArray();
                }
            }
            
        }

        public async Task<string> DecryptPlainTextAsync(byte[] cipherBytes, byte[] key)
        {
            byte[] iv = new byte[16]; // AES block size (IV size)
            byte[] actualCipherText = new byte[cipherBytes.Length - iv.Length];

            // Extract IV from the beginning of the ciphertext
            Array.Copy(cipherBytes, 0, iv, 0, iv.Length);
            Array.Copy(cipherBytes, iv.Length, actualCipherText, 0, actualCipherText.Length);

            using (Aes aes = Aes.Create())
            {
                aes.Key = key;
                aes.IV = iv;

                using (ICryptoTransform decryptor = aes.CreateDecryptor())
                using (MemoryStream ms = new MemoryStream(actualCipherText))
                using (CryptoStream cryptoStream = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cryptoStream, Encoding.UTF8))
                {
                    return await reader.ReadToEndAsync();
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