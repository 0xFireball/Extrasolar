using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    /// <summary>
    /// Represents a successful response that has a result returned
    /// by the server.
    /// </summary>
    public class ResultResponse : Response
    {
        [JsonConstructor]
        internal ResultResponse()
        {
        }

        public ResultResponse(Request request, JToken result = null) : this(request.Id, result)
        {
        }

        public ResultResponse(string id, JToken result = null)
        {
            Id = id;
            Result = result;
        }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public override Error Error { get; set; }
    }
}