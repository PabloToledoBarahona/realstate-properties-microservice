using Cassandra;
using Microsoft.Extensions.Options;
using PropertiesService.Config;

namespace PropertiesService.Services
{
    public class CassandraSessionFactory
    {
        private readonly CassandraOptions _options;
        private Cassandra.ISession? _session;

        public CassandraSessionFactory(IOptions<CassandraOptions> options)
        {
            _options = options.Value;
        }

        public Cassandra.ISession GetSession()
        {
            if (_session == null)
            {
                var cluster = Cluster.Builder()
                    .AddContactPoint(_options.ContactPoint)
                    .WithPort(_options.Port)
                    .Build();

                _session = cluster.Connect(_options.Keyspace);
            }

            return _session;
        }
    }
}