using System;
#if NET40
using System.Web;
#else
using System.Net;
#endif

namespace SelesGames.Rest
{
    public static class UriBuilderExtensions
    {
        //public static void AppendQuery(this UriBuilder uri, string query)
        //{
        //    var encodedQuery = HttpUtility.UrlEncode(query);

        //    if (uri.Query != null && uri.Query.Length > 1)
        //        uri.Query = uri.Query.Substring(1) + "&" + encodedQuery;
        //    else
        //        uri.Query = encodedQuery;
        //}

        public static void AppendQuery(this UriBuilder uri, bool useEncoding, string queryFormat, params object[] args)
        {
            var encodedQuery = useEncoding ?
                HttpUtility.UrlEncode(string.Format(queryFormat, args))
                :
                string.Format(queryFormat, args);

            if (uri.Query != null && uri.Query.Length > 1)
                uri.Query = uri.Query.Substring(1) + "&" + encodedQuery;
            else
                uri.Query = encodedQuery;
        }

        public static void AppendQuery(this UriBuilder uri, string queryFormat, params object[] args)
        {
            uri.AppendQuery(true, queryFormat, args);
        }

        public static void AppendParameter(this UriBuilder uri, string parameterName, string parameterValue)
        {
            uri.AppendQuery(false, "{0}={1}", parameterName, parameterValue);
        }
    }
}
