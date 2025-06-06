namespace PropertiesService.Config
{
    public class CassandraOptions
    {
        public string Keyspace { get; set; } = default!;
        public string SecureConnectBundlePath { get; set; } = default!;
        public string ClientId { get; set; } = default!;
        public string ClientSecret { get; set; } = default!;
    }
}