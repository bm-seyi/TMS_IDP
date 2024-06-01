using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;

namespace TMS_API.Utilities;

public class Auth 
{
    private const int parallelism = 8;
    private const int memory = 1024 * 1024;
    private const int iterations = 10;
    private const int storage = 64;

    public byte[] GenerateSalt()
    { 
        var buffer = new byte[storage];

        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(buffer);
        }
        return buffer;
    }

    public byte[] passwordHasher(string password, byte[] salt)
    {
        using (var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)))
        {
            argon2.Salt = salt;
            argon2.DegreeOfParallelism = parallelism;
            argon2.MemorySize = memory;
            argon2.Iterations = iterations;
            argon2.AssociatedData = null;
            argon2.KnownSecret = null;
            
            return argon2.GetBytes(storage);
        }
        
    }

    private string GenerateKey()
    {
        byte[] storage = new byte[32];

        using (var generator = RandomNumberGenerator.Create())
        {
            generator.GetBytes(storage);
        }

        string key = Convert.ToBase64String(storage)
        .Replace("+", "-")
        .Replace("/", "_")
        .TrimEnd('=');

       return "TMS-" + key;
    }
}