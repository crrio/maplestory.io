using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace maplestory.io.Services.Rethink
{
    public class RethinkDbOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public int Timeout { get; set; }
        public string Password { get; internal set; }
        public string Username { get; internal set; }
    }
}
