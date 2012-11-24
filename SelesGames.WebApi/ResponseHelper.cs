using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace SelesGames.WebApi
{
    public static class ResponseHelper
    {
        public static HttpResponseException CreateResponseException(HttpStatusCode code, string reason)
        {
            return new HttpResponseException(new HttpResponseMessage(code) { ReasonPhrase = reason });
        }
    }
}
