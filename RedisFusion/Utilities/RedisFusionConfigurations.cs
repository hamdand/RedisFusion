namespace RedisFusion.Utilities
{
    public class RedisFusionConfigurations
    {
        public bool IsEnabled { get; set; }
        public string ConnectionString { get; set; }
        public string? InstanceName { get; set; }
        public int ExpirationTimeSpanInMinutes { get; set; }
    }
}
