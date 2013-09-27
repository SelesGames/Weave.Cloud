using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;

namespace SelesGames.HttpClient
{
    public static class HttpResponseMessageExtensions
    {
        public static string GetValueForHeader(this HttpHeaders headers, string header)
        {
            IEnumerable<string> headerValues;
            if (headers.TryGetValues(header, out headerValues))
                return headerValues.FirstOrDefault();
            else
                return null;
        }
    }
}
