using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    public class Error
    {
        public Error(int code, string message, JToken data)
        {
            Code = code;
            Message = message;
            Data = data;
        }

        [JsonProperty("code", Required = Required.Always)]
        public int Code { get; set; }

        [JsonProperty("message", Required = Required.Always)]
        public string Message { get; set; }

        [JsonProperty("data")]
        public JToken Data { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}