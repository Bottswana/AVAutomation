using System;
using Serilog;
using System.IO;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace AVAutomation
{
    public class Program
    {
        /// <summary>
        /// The current configuration file we are using to load global and app specific settings
        /// </summary>
        public static string ApplicationConfigFile { get; private set; } = null;
        
        /// <summary>
        /// Application Entry Point
        /// Configure configuration & application logging, then hand over control to Kestrel
        /// </summary>
        /// <param name="args">Optional command line arguments</param>
        public static void Main(string[] args)
        {
            // Configuration File Discovery
            var ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase)?.Replace("file:\\", "")?.Replace("file:", "");
            if( ApplicationConfigFile == null && ExecutablePath != null )
            {
                // Check the current directory (standard asp.net core implementation)
                var ProjectAppConfig = Path.Combine(ExecutablePath, "appsettings.json");
                if( File.Exists(ProjectAppConfig) )
                {
                    Console.WriteLine($"Configuration file loaded from path: '{ProjectAppConfig}");
                    ApplicationConfigFile = ProjectAppConfig;
                }
            }
            
            // Handle missing configuration file
            if( ApplicationConfigFile == null )
            {
                Console.WriteLine("Error: Configuration file is missing. Please ensure the application has access to appsettings.json in the binary directory");
                Environment.Exit(2);
            }

            // Log Configuration
            var LoggingConfig = new ConfigurationBuilder().AddJsonFile(ApplicationConfigFile).Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(LoggingConfig).WriteTo.Console().CreateLogger();
            
            // Start Kestrel
            try
            {
                CreateWebHostBuilder().Build().Run();
            }
            catch( Exception Ex )
            {
                Log.Fatal(Ex, "Host terminated unexpectedly");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        /// <summary>
        /// Create the Kestrel instance
        /// </summary>
        /// <returns>IWebHostBuilder Instance</returns>
        private static IWebHostBuilder CreateWebHostBuilder()
        {
            return new WebHostBuilder()
            .UseKestrel()
            .UseStartup<Startup>()
            .UseUrls("http://0.0.0.0:5000")
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureLogging(logging => logging.AddSerilog())
            .UseDefaultServiceProvider((context, options) =>
            {
                options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
            })
            .ConfigureAppConfiguration( (config) =>
            {
                config.AddJsonFile(ApplicationConfigFile, optional: false, reloadOnChange: true);
            });
        }
    }
}