using Serilog;
using AVAutomation.Models;
using AVAutomation.Classes;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace AVAutomation.Controllers
{
    [Route("/api")]
    public class AVControlController : APIControllerBase
    {
        // Screen Statics
        private static bool _ScreenIsVeryBusy = false;
        private static bool _ScreenLowered = false;
        private readonly int _WaitTime;
        
        // Projector Statics
        private static bool _MustWaitForNextCommand = false;
        private const int _CommandPause = 120000;

        #region Initialisation
        public AVControlController(IConfiguration Config)
        {
            _WaitTime = Config.GetValue<int>("ScreenLowerTime");
        }
        
        [HttpGet]
        public IActionResult Index()
        {
            return BadRequest();
        }
        #endregion Initialisation
        
        /// <summary>
        /// Get AV Status
        /// </summary>
        [HttpGet("status")]
        public IActionResult Status()
        {
            // Return Status
            return Json(new JsonResponse<StatusModel>
            {
                Success = true,
                Response = new StatusModel
                {
                    ProjectorOn = Startup.Projector.GetPowerStatus(),
                    ScreenMoving = _ScreenIsVeryBusy,
                    ScreenLowered = _ScreenLowered
                }
            });
        }

        /// <summary>
        /// Lower the Projection Screen
        /// </summary>
        [HttpGet("screen/lower")]
        public IActionResult LowerScreen()
        {
            // Check if we are a busy bee
            if( _ScreenIsVeryBusy )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Screen is busy! Go away",
                    Success = false
                }); 
            }
            
            // Check if the screen is already in the lowered position
            if( _ScreenLowered )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Screen already lowered",
                    Success = false
                }); 
            }
            
            // Create stop task
            _ScreenIsVeryBusy = true;
            _ScreenLowered = true;
            Task.Run(async () =>
            {
                await Task.Delay(_WaitTime);
                if( _ScreenLowered )
                {
                    Log.Debug("Screen Stopped");
                    Startup.Screen.StopScreen();
                    _ScreenIsVeryBusy = false;
                }
            });
            
            // Request Screen Lowering
            Startup.Screen.LowerScreen();

            // Return success
            return Json(new JsonResponse
            {
                Success = true
            });
        }
        
        /// <summary>
        /// Raise the Projection Screen
        /// </summary>
        [HttpGet("screen/raise")]
        public IActionResult RaiseScreen()
        {
            // Check if we are a busy bee
            if( _ScreenIsVeryBusy )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Screen is busy! Go away",
                    Success = false
                }); 
            }
            
            // Check if the screen is already in the raised position
            if( !_ScreenLowered )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Screen already raised",
                    Success = false
                }); 
            }

            // Create Raise task
            var RaiseTime = _WaitTime;
            _ScreenIsVeryBusy = true;
            _ScreenLowered = false;
            Task.Run(async () =>
            {
                while( !_ScreenLowered && RaiseTime > 0 )
                {
                    await Task.Delay(10);
                    RaiseTime -= 10;
                }
                
                Log.Debug("Raise Completed");
                _ScreenIsVeryBusy = false;
            });
            
            // Request Screen Raising
            Startup.Screen.RaiseScreen();
            
            // Return success
            return Json(new JsonResponse
            {
                Success = true
            });
        }
        
        /// <summary>
        /// Turn on the Projector
        /// </summary>
        [HttpGet("projector/on")]
        public IActionResult TurnOnProjector()
        {
            // Check if we are a busy bee
            if( _MustWaitForNextCommand )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Nope. Too many commands! Go away",
                    Success = false
                }); 
            }
            
            // Check if the projector is already on
            if( Startup.Projector.GetPowerStatus() )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Projector is already on",
                    Success = false
                }); 
            }
            
            // Turn on Projector
            _MustWaitForNextCommand = true;
            Startup.Projector.TurnOn();
            
            // Create release task
            Task.Run(async () =>
            {
                await Task.Delay(_CommandPause);
                _MustWaitForNextCommand = false;
            });

            // Return success
            return Json(new JsonResponse
            {
                Success = true
            });
        }
        
        /// <summary>
        /// Turn off the Projector
        /// </summary>
        [HttpGet("projector/off")]
        public IActionResult TurnOffProjector()
        {
            // Check if we are a busy bee
            if( _MustWaitForNextCommand )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Nope. Too many commands! Go away",
                    Success = false
                }); 
            }
            
            // Check if the projector is already on
            if( !Startup.Projector.GetPowerStatus() )
            {
                return Json(new JsonResponse
                {
                    ErrorMessage = "Projector is already off",
                    Success = false
                }); 
            }
            
            // Turn off Projector
            _MustWaitForNextCommand = true;
            Startup.Projector.TurnOff();
            
            // Create release task
            Task.Run(async () =>
            {
                await Task.Delay(_CommandPause);
                _MustWaitForNextCommand = false;
            });

            // Return success
            return Json(new JsonResponse
            {
                Success = true
            });
        }
    }
}