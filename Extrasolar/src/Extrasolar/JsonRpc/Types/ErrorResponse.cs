namespace Extrasolar.JsonRpc.Types
{
    public class ErrorResponse : Response
    {
        public ErrorResponse(Request request, Error error = null) : this(request.Id, error)
        {
        }

        public ErrorResponse(string id, Error error = null)
        {
            Id = id;
            Error = error;
        }
    }
}