namespace TMS_IDP.Models.RateLimit
{
    public class tokenBucketRateLimitOptions
    {    
        public int ReplenishmentPeriod { get; set; }
        public int QueueLimit { get; set; }

        public int TokenLimit { get; set; }
        public int TokensPerPeriod { get; set; }
        public bool AutoReplenishment { get; set; }
    }
}

