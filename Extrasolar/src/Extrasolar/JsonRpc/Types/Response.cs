using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    public class Response
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version;

        /// <summary>Unique request identifier.</summary>
        [JsonProperty("id", Required = Required.Always)]
        public int Id;

        /// <summary>The result if no error occured.</summary>
        [JsonProperty("result")]
        public JToken Result;

        /// <summary>The error. null if no error occured.</summary>
        [JsonProperty("error")]
        public Error Error;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}