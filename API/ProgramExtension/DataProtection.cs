using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;
using TMS_IDP.Utilities;

public static class ServiceCollectionExtensions
{
    public static void ConfigureDataProtection(this IServiceCollection services, IConfiguration configuration)
    {
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        ICertificateService certificateService = serviceProvider.GetRequiredService<ICertificateService>();

        X509Certificate2 certificate = certificateService.GetCertificateFromVault().GetAwaiter().GetResult();

        string redisPassword = configuration["Redis:Password"] ?? throw new ArgumentNullException(nameof(redisPassword));
        ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"localhost:6379,password={redisPassword}");

        services.AddDataProtection()
            .SetApplicationName("TMS_IDP")
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
            .ProtectKeysWithCertificate(certificate)
            .SetDefaultKeyLifetime(TimeSpan.FromDays(30));
    }
}
