namespace TMS_API.Configuration
{
    public class ApiRateLimitSettings
    {
        public const string rateLimitPolicy = "tokenBucket";
        public int ReplenishmentPeriod { get; set; } = 2;
        public int QueueLimit { get; set; } = 2;

        public int TokenLimit { get; set; } = 10;
        public int TokensPerPeriod { get; set; } = 4;
        public bool AutoReplenishment { get; set; } = true;
    }
}

