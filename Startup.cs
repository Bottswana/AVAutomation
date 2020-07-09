using AVAutomation.Classes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace AVAutomation
{
    public class Startup
    {
        /// <summary>
        /// Screen Controller
        /// </summary>
        public static ScreenController Screen { get; private set; }
        
        /// <summary>
        /// Projector Controller
        /// </summary>
        public static EpsonController Projector { get; private set; }
        
        public Startup(IConfiguration Config)
        {
            // Initialise Screen
            Screen = new ScreenController(Config);
            Screen.RaiseScreen();
            
            
            // Initialise Projector
            Projector = new EpsonController(Config);
        }
        
        #region Service Configuration
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure routing
            services.AddControllers().AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // App Configuration
            app.UseStatusCodePages();
            app.UseRouting();
            app.UseCors();

            // Endpoint Router
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default", pattern: "{controller}/{action=Index}/{id?}");
            });

            // Configure Development Features
            if( env.IsDevelopment() )
            {
                // Enable development error diagnostics
                app.UseDeveloperExceptionPage();
            }
        }
        #endregion Service Configuration
    }
}