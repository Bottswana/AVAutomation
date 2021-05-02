using System;
using Serilog;
using System.IO;
using System.Reflection;
using AVAutomation.Classes;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace AVAutomation
{
    public class Program
    {
        private static ScreenController Screen;
        private static EpsonController Projector;
        private static SonyController Amp;
        
        /// <summary>
        /// Application Entry Point
        /// </summary>
        /// <param name="args">Optional command line arguments</param>
        public static void Main(string[] args)
        {
            string ApplicationConfigFile = null;
            
            // Configuration File Discovery
            var ExecutablePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)?.Replace("file:\\", "")?.Replace("file:", "");
            if( ExecutablePath != null )
            {
                // Check the current directory
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
            var Config = new ConfigurationBuilder().AddJsonFile(ApplicationConfigFile).Build();
            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(Config).WriteTo.Console().CreateLogger();
            
            // Initialise Classes
            Projector = new EpsonController(Config);
            Screen = new ScreenController(Config);
            Amp = new SonyController(Config);

            // Run Task
            var TheTask = Task.Run(DoMonitor);
            TheTask.Wait();
        }
        
        /// <summary>
        /// Monitor for Projector power on and trigger the screen as appropriate
        /// </summary>
        private static async Task DoMonitor()
        {
            try
            {
                var IsPoweredOn = false;
                while( true )
                {
                    // Poll projector state
                    var ProjectorState = Projector.GetPowerStatus();
                    if( ProjectorState == EpsonPowerStatus.On && !IsPoweredOn )
                    {
                        // Power on system
                        Log.Information("Projector powered on, turning on components");
                        IsPoweredOn = true;
                        
                        Screen.LowerScreen();
                        try
                        {
                            await Amp.AmpPowerRequest("active");
                        }
                        catch( Exception Ex )
                        {
                            Log.Error(Ex, "Amp API Error");
                        }
                    }
                    else if( ProjectorState == EpsonPowerStatus.Off && IsPoweredOn )
                    {
                        // Power off system
                        Log.Information("Projector powered off, shutting down components");
                        IsPoweredOn = false;
                        
                        Screen.RaiseScreen();
                        try
                        {
                            await Amp.AmpPowerRequest("off");
                        }
                        catch( Exception Ex )
                        {
                            Log.Error(Ex, "Amp API Error");
                        }
                    }
                    
                    // Wait 5 seconds
                    await Task.Delay(TimeSpan.FromSeconds(5));
                }
            }
            catch( Exception Ex )
            {
                Log.Error(Ex, "Exception Thrown");
            }
        }
    }
}