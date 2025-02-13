using System.Text.Json.Serialization;

namespace TMS_IDP.Models.DataProtection
{
    public class ListCertificateModel
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
        public required ListDataContent Data { get; set; }
        
        [JsonPropertyName("wrap_info")]
        public required object WrapInfo { get; set; }
        
        [JsonPropertyName("warnings")]
        public required object Warnings { get; set; }
        
        [JsonPropertyName("auth")]
        public required object Auth { get; set; }
        
        [JsonPropertyName("mount_type")]
        public required string MountType { get; set; }
    }

    public class ListDataContent
    {
        [JsonPropertyName("keys")]
        public required List<string> Keys { get; set; }
    }

}
