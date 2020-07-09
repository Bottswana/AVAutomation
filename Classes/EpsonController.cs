using System;
using Serilog;
using System.Text;
using System.IO.Ports;
using Microsoft.Extensions.Configuration;

namespace AVAutomation.Classes
{
    public class EpsonController : IDisposable
    {
        private readonly SerialPort _Port = new SerialPort();

        #region Initialisation
        public EpsonController(IConfiguration Config)
        {
            // Get configuration
            var SerialConfig = Config.GetSection("SerialPorts");
            
            // Configure port
            _Port.PortName = SerialConfig.GetValue<string>("Projector");
            _Port.Handshake = Handshake.None;
            _Port.StopBits = StopBits.One;
            _Port.Parity = Parity.None;
            _Port.BaudRate = 9600;
            _Port.DataBits = 8;
            _Port.Open();
        }
        
        public void Dispose()
        {
            _Port?.Dispose();
        }
        #endregion Initialisation
        
        #region Public Methods
        /// <summary>
        /// Get Projector Power Status
        /// </summary>
        /// <returns></returns>
        public bool GetPowerStatus()
        {
            try
            {
                var PowerStatus = _ExecuteCommand("PWR?\r\n");
                return PowerStatus.Contains("PWR=01") || PowerStatus.Contains("PWR=02"); 
            }
            catch( Exception Ex )
            {
                Log.Error(Ex, "Error requesting projector power status");
                return false;
            }
        }

        /// <summary>
        /// Turn On Projector
        /// </summary>
        /// <exception cref="Exception">Projector Error Received</exception>
        public void TurnOn()
        {
            _ExecuteCommand("PWR ON\r\n");
        }

        /// <summary>
        /// Turn Off Projector
        /// </summary>
        /// <exception cref="Exception">Projector Error Received</exception>
        public void TurnOff()
        {
            _ExecuteCommand("PWR OFF\r\n");
        }
        #endregion Public Methods
        
        #region Private Methods
        private string _ExecuteCommand(string Command)
        {
            // Write command to Projector
            var CommandBytes = Encoding.ASCII.GetBytes(Command);
            _Port.Write(CommandBytes, 0, CommandBytes.Length);
            Log.Information($"Projector Command: {Command}");
            
            // Read response (Up to the : From the Projector)
            var ResponseData = new StringBuilder();
            while( true )
            {
                // Read next byte
                var ResponseArray = new byte[1];
                if( _Port.Read(ResponseArray, 0, 1) != 1 ) throw new Exception("Failed to read response");
                Log.Debug($"Read byte from Projector {ResponseArray[0]}");
                
                // Check if this byte is a :
                if( ResponseArray[0] == 0x3A ) break;
                ResponseData.Append((char)ResponseArray[0]);
            }
            
            // Check for Projector Error
            Log.Information($"Projector response: {ResponseData}");
            if( ResponseData.ToString().Contains("ERR") )
            {
                throw new Exception($"Projector Error: {ResponseData}");
            }
            
            // Return response
            return ResponseData.ToString();
        }
        #endregion Private Methods
    }
}