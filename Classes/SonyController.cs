using System;
using Serilog;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace AVAutomation.Classes
{
    // https://developer.sony.com/develop/audio-control-api/api-references/api-overview-2
    public class SonyController
    {
        private readonly string _ServerAddress;
        
        public SonyController(IConfiguration Config)
        {
            _ServerAddress = Config.GetValue<string>("SonyIP");
            if( string.IsNullOrEmpty(_ServerAddress) )
            {
                Log.Error("IP for AMP not set!");
            }
        }
        
        public async Task AmpPowerRequest(string state)
        {
            var RequestModel = new SonyRequestModel
            {
                method = "setPowerStatus",
                version = "1.1",
                id = 1
            };
            
            RequestModel.paramArray.Add(new Dictionary<string, string>
            {
                {"status", state}
            });
            
            var PowerRequest = await _MakePostRequest("system", RequestModel);
            Log.Debug("Amp Response: {Response}", PowerRequest);
        }
        
        private async Task<string> _MakePostRequest(string SourceRequestPath, SonyRequestModel PostData)
        {
            // Check server IP is set
            if( string.IsNullOrEmpty(_ServerAddress) ) throw new ArgumentException("Invalid IP");
            
            // Configure WebClient
            using var WebClient = new HttpClient
            {
                DefaultRequestHeaders = { Accept = { new MediaTypeWithQualityHeaderValue("application/json") } },
                BaseAddress = new Uri($"http://{_ServerAddress}:10000/sony/"),
                Timeout = TimeSpan.FromSeconds(5)
            };

            // Perform Request
            var Result = await WebClient.PostAsync(SourceRequestPath, new StringContent(JsonConvert.SerializeObject(PostData)));
            if( Result.IsSuccessStatusCode )
            {
                return await Result.Content.ReadAsStringAsync();
            }
            else
            {
                try
                {
                    var ReadBody = await Result.Content.ReadAsStringAsync();
                    Log.Debug("Body result: {Body}", ReadBody);
                }
                catch( Exception )
                {
                    Log.Error("Unable to read body of error response");
                }
                
                // No effort here, as this api is terrible and returns 200 on an error response.
                throw new Exception("Error");
            }
        }
    }
    
    public class RequestException : Exception
    {
        #region RequestException
        /// <summary>
        /// Status code from the remote server
        /// </summary>
        public HttpStatusCode StatusCode { get; }
        
        public RequestException(string Error) : base(Error) {}
        
        public RequestException(HttpStatusCode Code, string Error) : base(Error)
        {
            StatusCode = Code;
        }
        #endregion RequestException
    }
    
    public class SonyErrorModel
    {
        public (int, string) error { get; set; }
    }
    
    public class SonyRequestModel
    {
        public string method { get; set; }
        
        public int id { get; set; }
        
        [JsonProperty("params")]
        public List<Dictionary<string, string>> paramArray { get; set; } = new List<Dictionary<string, string>>();
        
        public string version { get; set; }
    }
    
    public class SonyResposeModel<T>
    {
        public int id { get; set; }
        
        public T result { get; set; }
    }
}