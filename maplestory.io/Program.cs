using maplestory.io.Services.MapleStory;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

namespace maplestory.io
{
    public class Program
    {
        public static void Main(string[] args)
        {
            dynamic obj = Newtonsoft.Json.JsonConvert.DeserializeObject(File.ReadAllText("./appsettings.json"));
            string wzPath = obj.WZ.WZPath;
            LoggerFactory logging = new LoggerFactory();
            WZFactory wzFactory = new WZFactory(logging.CreateLogger<WZFactory>(), wzPath);
            ItemFactory.Load(wzFactory, logging.CreateLogger<ItemFactory>());
            ItemFactory.cacheItems();

            var host = new WebHostBuilder()
                .UseKestrel(options =>
                {
                    options.Limits.MaxRequestLineSize = 99999999;
                    options.ThreadCount = 24;
                    options.Limits.MaxRequestBufferSize = int.MaxValue;
                    options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(15);
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
