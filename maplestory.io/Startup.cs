using maplestory.io.Data;
using maplestory.io.Entities;
using maplestory.io.Services.Implementations.MapleStory;
using maplestory.io.Services.Interfaces.MapleStory;
using maplestory.io.Services.Rethink;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Swashbuckle.AspNetCore.Swagger;

namespace maplestory.io
{
    public class Startup
    {
        public static bool Ready;
        public static bool Started;

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
            // ===== Add our DbContext ========
            services.AddDbContext<ApplicationDbContext>();

            // ===== Add Identity ========
            services.AddIdentity<IdentityUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 1;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Add framework services.
            services.AddMvc()
                .AddJsonOptions(options => {
                    options.SerializerSettings.Formatting = Newtonsoft.Json.Formatting.Indented;
                    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
                })
                .AddJsonOptions(options => options.SerializerSettings.Converters.Add(new ImageConverter()));

            services.AddCors();

            services.Configure<RethinkDbOptions>(Configuration.GetSection("RethinkDb"));
            services.Configure<WZOptions>(Configuration.GetSection("WZ"));
            services.AddSingleton<IRethinkDbConnectionFactory, RethinkDbConnectionFactory>();

            services.AddScoped<IWZFactory, WZFactory>();
            services.AddScoped<IItemFactory, ItemFactory>();
            services.AddScoped<ISkillFactory, SkillFactory>();
            services.AddScoped<IMusicFactory, MusicFactory>();
            services.AddScoped<IMapFactory, MapFactory>();
            services.AddScoped<IMobFactory, MobFactory>();
            services.AddScoped<INPCFactory, NPCFactory>();
            services.AddScoped<IQuestFactory, QuestFactory>();
            services.AddScoped<ITipFactory, TipFactory>();
            services.AddScoped<IZMapFactory, ZMapFactory>();
            services.AddScoped<ICharacterFactory, CharacterFactory>();
            services.AddScoped<ICraftingEffectFactory, CraftingEffectFactory>();
            services.AddScoped<IAndroidFactory, AndroidFactory>();
            services.AddScoped<IPetFactory, PetFactory>();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("V2", new Info { Title = "MapleStory.IO", Version = "V2", Contact = new Contact() { Email = "andy@crr.io", Name = "Andy", Url = "https://github.com/crrio/maplestory.io" }, Description = "The unofficial MapleStory API Documentation for developers." });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IRethinkDbConnectionFactory connectionFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));

            if (env.IsDevelopment())
            {
                loggerFactory.AddDebug(LogLevel.Debug);
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                loggerFactory.AddDebug();
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod());

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });

            // Enable middleware to serve generated Swagger as a JSON endVector2.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS etc.), specifying the Swagger JSON endVector2.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/V2/swagger.json", "MapleStory.IO V2");
            });

            //using (var con = connectionFactory.CreateConnection())
            //    con.CheckOpen();
        }
    }
}
