using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using WZData.MapleStory.Items;
using WZData.MapleStory.Images;
using System.Linq;
using System.Threading.Tasks;
using PKG1;
using System.Diagnostics;
using WZData.MapleStory.Characters;
using ImageSharp;

namespace maplestory.io
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText("./appsettings.json"));
            WZOptions wzPath = Newtonsoft.Json.JsonConvert.DeserializeObject<WZOptions>(obj.WZ.ToString());
            ILoggerFactory logging = (new LoggerFactory()).AddConsole(LogLevel.Trace);
            WZFactory.Load(logging.CreateLogger<WZFactory>());

            ILogger<Package> packageLogger = logging.CreateLogger<Package>();
            ILogger<PackageCollection> packageCollectionLogger = logging.CreateLogger<PackageCollection>();
            ILogger<VersionGuesser> versionGuesserLogger = logging.CreateLogger<VersionGuesser>();
            ILogger<WZReader> readerLogging = logging.CreateLogger<WZReader>();

            readerLogging.LogDebug("Initializing Keys");
            WZReader.InitializeKeys();
            readerLogging.LogDebug("Done");

            PackageCollection.Logging = (s) => packageCollectionLogger.LogInformation(s);
            VersionGuesser.Logging = (s) => versionGuesserLogger.LogInformation(s);
            Package.Logging = (s) => packageLogger.LogInformation(s);
            Parallel.ForEach(wzPath.versions, (version) => WZFactory.AddWz(version.path, version.region, version.version));

            readerLogging.LogDebug("Caching item requirements");
            WZFactory wzFactory = new WZFactory();
            ItemFactory.CacheEquipMeta(wzFactory, logging.CreateLogger<ItemFactory>());

            ILogger prog = logging.CreateLogger<Program>();
            watch.Stop();
            prog.LogInformation($"Starting aspnet kestrel, took {watch.ElapsedMilliseconds}ms to initialize");

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestLineSize = 99999999;
                    options.ThreadCount = 32;
                    options.Limits.MaxRequestBufferSize = int.MaxValue;
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
                    options.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(120);
                })
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseUrls("http://*:5000")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
