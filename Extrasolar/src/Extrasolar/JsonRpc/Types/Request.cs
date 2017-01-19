using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Request
    {
        public Request(string method, JToken parameters, string id)
        {
            Method = method;
            Parameters = parameters;
            Id = id;
        }

        /// <summary>
        /// JSON-RPC protocol version
        /// </summary>
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string JsonRpc => "2.0";

        /// <summary>Unique request identifier.</summary>
        [JsonProperty("id")]
        public string Id { get; private set; }

        /// <summary>The name of the remote method.</summary>
        [JsonProperty("method", Required = Required.Always)]
        public string Method { get; private set; }

        /// <summary>Optional parameters.</summary>
        [JsonProperty("params")]
        public JToken Parameters { get; private set; }

        public bool IsNotification => Id == null;

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}