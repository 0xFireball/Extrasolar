using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    public class Request
    {
        /// <summary>
        /// JSON-RPC protocol version
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRpc => "2.0";

        /// <summary>Unique request identifier.</summary>
        [JsonProperty("id", Required = Required.Always)]
        public int Id { get; private set; }

        /// <summary>The name of the remote method.</summary>
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; private set; }

        /// <summary>Optional parameters.</summary>
        [JsonProperty("params")]
        public JToken Parameters { get; private set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}