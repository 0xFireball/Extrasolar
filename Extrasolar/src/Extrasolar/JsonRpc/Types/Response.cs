using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public abstract class Response
    {
        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version { get; set; }

        /// <summary>Unique request identifier.</summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; set; }

        /// <summary>The result if no error occured.</summary>
        [JsonProperty("result")]
        public virtual JToken Result { get; set; }

        /// <summary>The error. null if no error occured.</summary>
        [JsonProperty("error")]
        public virtual Error Error { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}