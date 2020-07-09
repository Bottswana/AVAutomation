using System;
using System.Text;
using System.Collections;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace AVAutomation.Classes
{
    public class APIControllerBase : Controller
    {
        /// <summary>
        /// Validate model
        /// </summary>
        /// <param name="Model">Model to Validate</param>
        /// <param name="ValidationMessage">Validation Feedback</param>
        /// <returns>True on valid, false otherwise</returns>
        protected bool ValidateModel(object Model, out string ValidationMessage)
        {
            var ValidationResultList = new List<ValidationResult>();
            if( Model == null )
            {
                ValidationMessage = "No model provided";
                return false;
            }
            else if( _DoValidate(Model, ValidationResultList) )
            {
                ValidationMessage = "";
                return true;
            }
            else
            {
                var ErrorString = new StringBuilder();
                foreach( var Item in ValidationResultList )
                {
                    if( ErrorString.Length > 0 ) ErrorString.Append("; ");
                    ErrorString.Append(Item.ErrorMessage.TrimEnd('.'));
                }

                ValidationMessage = ErrorString.ToString();
                return false;
            }
        }
        
        /// <summary>
        /// Allow us to validate input that is an iterable object
        /// </summary>
        /// <param name="Model"></param>
        /// <param name="ValidationResults"></param>
        /// <returns></returns>
        private bool _DoValidate(object Model, List<ValidationResult> ValidationResults)
        {
            // If this item implements IEnumerable, iterate over top level and validate
            if( Model is IEnumerable model )
            {
                foreach( var Item in model )
                {
                    if( !_DoValidate(Item, ValidationResults) ) return false;
                }
                
                return true;
            }
            else
            {
                return Validator.TryValidateObject(Model, new ValidationContext(Model), ValidationResults);
            }
        }

        /// <summary>
        /// Get the client IP, preferring X-Forwarded-For over HttpContext IP
        /// </summary>
        /// <returns>IP as Text</returns>
        protected string GetClientIP()
        {
            try
            {
                if( Request.Headers.ContainsKey("X-Real-IP") )
                {
                    return Request.Headers["X-Real-IP"].ToString();
                }
                else if( Request.Headers.ContainsKey("x-forwarded-for") )
                {
                    return Request.Headers["x-forwarded-for"].ToString();
                }
                else if( Request.Headers.ContainsKey("X-Forwarded-For") )
                {
                    return Request.Headers["X-Forwarded-For"].ToString();
                }

                // Use Socket Address
                return Request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
            catch( Exception )
            {
                return null;
            }
        }
    }

    public class JsonResponse<T>
    {
        /// <summary>
        /// A detailed error message, if any
        /// </summary>
        public string ErrorMessage { get; set; } = null;
        
        /// <summary>
        /// If the operation was successful
        /// </summary>
        public bool Success { get; set; } = false;

        /// <summary>
        /// Response to the operation
        /// </summary>
        public T Response { get; set; }
    }

    public class JsonResponse
    {
        /// <summary>
        /// A detailed error message, if any
        /// </summary>
        public string ErrorMessage { get; set; } = null;
        
        /// <summary>
        /// If the operation was successful
        /// </summary>
        public bool Success { get; set; } = false;
    }
}