using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TMS_IDP.Models
{
   
    public class CertificateData
    {
        [JsonPropertyName("ca_chain")]
        public required List<string> CaChain { get; set; }

        [JsonPropertyName("certificate")]
        public required string Certificate { get; set; }

        [JsonPropertyName("expiration")]
        public long Expiration { get; set; }

        [JsonPropertyName("issuing_ca")]
        public required string IssuingCa { get; set; }

        [JsonPropertyName("private_key")]
        public  required string PrivateKey { get; set; }

        [JsonPropertyName("private_key_type")]
        public required string PrivateKeyType { get; set; }

        [JsonPropertyName("serial_number")]
        public required string SerialNumber { get; set; }
    }

    public class CertificateRequest
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
        public required CertificateData Data { get; set; }

        [JsonPropertyName("wrap_info")]
        public required object WrapInfo { get; set; }

        [JsonPropertyName("warnings")]
        public required object Warnings { get; set; }

        [JsonPropertyName("auth")]
        public required object Auth { get; set; }

        [JsonPropertyName("mount_type")]
        public required string MountType { get; set; }
    }
}