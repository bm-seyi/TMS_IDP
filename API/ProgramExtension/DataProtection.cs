using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using StackExchange.Redis;
using TMS_IDP.Utilities;

public static class ServiceCollectionExtensions
{
    public static async Task AddDataProtectionAsync(this IServiceCollection services, ICertificateService certificateService, IConnectionMultiplexer redis)
    {        
        string path = "/v1/secret/data/latest/certificate";
        X509Certificate2? certificate = await certificateService.RetrieveAsync(path);
    
        if (certificate == null)
        {
            certificate = await certificateService.GenerateAsync();
            await certificateService.StoreAsync(path, certificate);
        }
        else if (DateTime.Now > certificate.NotAfter)
        {
            await certificateService.StoreAsync($"/v1/secret/data/archive/{certificate.Thumbprint}", certificate);
            await certificateService.DeleteAsync(path);
            certificate = await certificateService.GenerateAsync();
            await certificateService.StoreAsync(path, certificate);
        }

        List<string>? certificateList =  await certificateService.ListAsync("v1/secret/metadata/archive?list=true");
        
        X509Certificate2Collection x509CertificateList = [certificate];
        if (certificateList != null)
        {
            foreach (string cert in certificateList)
            {
                X509Certificate2 x509Certificate = await certificateService.RetrieveAsync($"/v1/secret/data/archive/{cert}") ?? throw new ArgumentNullException(nameof(x509Certificate));
                x509CertificateList.Add(x509Certificate);
            }
        }
        
        services.AddDataProtection()
            .SetApplicationName("TMS_IDP")
            .ProtectKeysWithCertificate(certificate)
            .UnprotectKeysWithAnyCertificate(x509CertificateList.ToArray())
            .PersistKeysToStackExchangeRedis(redis, "DataProtection-Keys")
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256,
            })
            .SetDefaultKeyLifetime(TimeSpan.FromDays(30));
    }
}
