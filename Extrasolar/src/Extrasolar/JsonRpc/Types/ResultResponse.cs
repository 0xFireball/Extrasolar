using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    public class ResultResponse : Response
    {
        public ResultResponse(Request request, JToken result = null) : this(request.Id, result)
        {
        }

        public ResultResponse(string id, JToken result = null)
        {
            Id = id;
            Result = result;
        }

        [JsonProperty("error", NullValueHandling = NullValueHandling.Ignore)]
        public override Error Error { get; protected set; }
    }
}