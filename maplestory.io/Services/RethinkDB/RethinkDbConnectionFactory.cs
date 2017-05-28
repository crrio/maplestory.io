using Microsoft.Extensions.Options;
using RethinkDb.Driver;
using RethinkDb.Driver.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Services.Rethink
{
    public class RethinkDbConnectionFactory : IRethinkDbConnectionFactory
    {
        private static RethinkDB R = RethinkDB.R;
        private RethinkDbOptions _options;

        public RethinkDbConnectionFactory(IOptions<RethinkDbOptions> options)
        {
            _options = options.Value;
        }

        // TODO: Connection pool somehow kty
        public Connection CreateConnection()
        {
            Connection conn = R.Connection()
                    .Hostname(_options.Host)
                    .Port(_options.Port)
                    .Timeout(_options.Timeout)
                    .User(_options.Username, _options.Password).Connect();

            if (!conn.Open)
            {
                conn.Reconnect();
            }

            return conn;
        }

        public RethinkDbOptions GetOptions()
        {
            return _options;
        }
    }
}
