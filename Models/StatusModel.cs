namespace AVAutomation.Models
{
    public class StatusModel
    {
        /// <summary>
        /// If the screen is lowered
        /// </summary>
        public bool ScreenLowered { get; set; }
        
        /// <summary>
        /// If the screen is currently moving
        /// </summary>
        public bool ScreenMoving { get; set; }
        
        /// <summary>
        /// Projector Power Status
        /// </summary>
        public bool ProjectorOn { get; set; }
    }
}