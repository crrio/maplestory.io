using RethinkDb.Driver.Net;

namespace maplestory.io.Services.Rethink
{
    public interface IRethinkDbConnectionFactory
    {
        Connection CreateConnection();
        RethinkDbOptions GetOptions();
    }
}
