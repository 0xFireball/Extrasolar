using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    [JsonObject(MemberSerialization.OptIn)]
    public class Response
    {
        public Response(Request request, JToken result, Error error = null) : this(request.Id, result, error)
        {
        }

        public Response(string id, JToken result, Error error = null)
        {
            Id = id;
            Result = result;
            Error = error;
        }

        [JsonProperty("jsonrpc", Required = Required.Always)]
        public string Version;

        /// <summary>Unique request identifier.</summary>
        [JsonProperty("id", Required = Required.Always)]
        public string Id { get; private set; }

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