using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    /// <summary>
    /// Represents a response that contains an error from the server
    /// </summary>
    public class ErrorResponse : Response
    {
        [JsonConstructor]
        internal ErrorResponse()
        {
        }

        public ErrorResponse(Request request, Error error = null) : this(request.Id, error)
        {
        }

        public ErrorResponse(string id, Error error = null)
        {
            Id = id;
            Error = error;
        }

        [JsonProperty("result", NullValueHandling = NullValueHandling.Ignore)]
        public override JToken Result { get; set; }
    }
}