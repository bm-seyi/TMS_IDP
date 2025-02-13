using System.Text.Json.Serialization;

namespace TMS_IDP.Models.DataProtection
{
    public class ReceiveCertificateModel
    {
        [JsonPropertyName("request_id")]
        public required string RequestId { get; set; }
        
        [JsonPropertyName("lease_id")]
        public required string LeaseId { get; set; }
        
        [JsonPropertyName("renewable")]
        public bool Renewable { get; set; }
        
        [JsonPropertyName("lease_duration")]
        public int LeaseDuration { get; set; }
        
        [JsonPropertyName("data")]
        public required RetrieveCertificateDataWrapper Data { get; set; }
        
        [JsonPropertyName("wrap_info")]
        public required object WrapInfo { get; set; }
        
        [JsonPropertyName("warnings")]
        public required object Warnings { get; set; }
        
        [JsonPropertyName("auth")]
        public required object Auth { get; set; }
        
        [JsonPropertyName("mount_type")]
        public required string MountType { get; set; }
    }

    public class RetrieveCertificateDataWrapper
    {
        [JsonPropertyName("data")]
        public required RetrieveCertificateData Data { get; set; }
        
        [JsonPropertyName("metadata")]
        public required Metadata Metadata { get; set; }
    }

    public class RetrieveCertificateData
    {
        [JsonPropertyName("certificate")]
        public required string Certificate { get; set; }

        [JsonPropertyName("private_key")]
        public required string PrivateKey { get; set; }
    }

    public class Metadata
    {
        [JsonPropertyName("created_time")]
        public DateTime CreatedTime { get; set; }
        
        [JsonPropertyName("custom_metadata")]
        public required object CustomMetadata { get; set; }
        
        [JsonPropertyName("deletion_time")]
        public required string DeletionTime { get; set; }
        
        [JsonPropertyName("destroyed")]
        public bool Destroyed { get; set; }
        
        [JsonPropertyName("version")]
        public int Version { get; set; }
    }
}