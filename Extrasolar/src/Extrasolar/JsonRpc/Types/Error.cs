using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Extrasolar.JsonRpc.Types
{
    public class Error
    {
        [JsonConstructor]
        public Error()
        {
        }

        public Error(JsonRpcErrorCode code, string message, JToken data) : this((int)code, message, data)
        {
        }

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

        public JsonRpcErrorCode GetErrorCode()
        {
            if (Code <= -32000 && Code >= -32768)
            {
                // Spec error code
                return (JsonRpcErrorCode)Code;
            }
            return JsonRpcErrorCode.ApplicationDefined;
        }

        /// <summary>
        /// Used to specify a specific error code.
        /// </summary>
        public enum JsonRpcErrorCode
        {
            // User-defined

            ApplicationDefined = 0,

            // Specification

            ParseError = -32700,
            InvalidRequest = -32600,
            MethodNotFound = -32601,
            InvalidParams = -32602,
            InternalError = -32603,
            ServerError = -32000
        }
    }
}