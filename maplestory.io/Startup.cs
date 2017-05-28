using maplestory.io.Services.MapleStory;
using maplestory.io.Services.Rethink;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WZData;

namespace maplestory.io
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(options => options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented)
                .AddJsonOptions(options => options.SerializerSettings.Converters.Add(new ImageConverter()));

            services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDb"));
            services.Configure<WZOptions>(Configuration.GetSection("WZ"));
            services.AddSingleton<IRethinkDbConnectionFactory, RethinkDbConnectionFactory>();
            services.AddSingleton<IWZFactory, WZFactory>();
            services.AddSingleton<IItemFactory, ItemFactory>();
            services.AddSingleton<ISkillFactory, SkillFactory>();
            services.AddSingleton<IMusicFactory, MusicFactory>();
            services.AddSingleton<IMapFactory, MapFactory>();
            services.Configure<GzipCompressionProviderOptions>(options => options.Level = System.IO.Compression.CompressionLevel.Optimal);
            services.AddResponseCompression();
            services.AddResponseCaching();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IRethinkDbConnectionFactory connectionFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseResponseCompression();
            app.UseResponseCaching();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            using (var con = connectionFactory.CreateConnection())
                con.CheckOpen();

        }
    }
}
