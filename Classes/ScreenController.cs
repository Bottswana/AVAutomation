using System;
using System.IO.Ports;
using Microsoft.Extensions.Configuration;

namespace AVAutomation.Classes
{
    public class ScreenController : IDisposable
    {
        private readonly SerialPort _Port = new SerialPort();

        #region Initialisation
        public ScreenController(IConfiguration Config)
        {
            // Get configuration
            var SerialConfig = Config.GetSection("SerialPorts");
            
            // Configure port
            _Port.PortName = SerialConfig.GetValue<string>("Screen");
            _Port.Handshake = Handshake.None;
            _Port.StopBits = StopBits.One;
            _Port.Parity = Parity.None;
            _Port.BaudRate = 2400;
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
        /// Lower the Projection Screen
        /// </summary>
        public void LowerScreen()
        {
            _Port.Write(new byte[]{ 0xFF, 0xEE, 0xEE, 0xEE, 0xEE }, 0, 5);
        }
        
        /// <summary>
        /// Raise the Projection Screen
        /// </summary>
        public void RaiseScreen()
        {
            _Port.Write(new byte[]{ 0xFF, 0xEE, 0xEE, 0xEE, 0xDD }, 0, 5);
        }
        
        /// <summary>
        /// Stop the Projection Screen
        /// </summary>
        public void StopScreen()
        {
            _Port.Write(new byte[]{ 0xFF, 0xEE, 0xEE, 0xEE, 0xCC }, 0, 5);
        }
        #endregion Public Methods
    }
}