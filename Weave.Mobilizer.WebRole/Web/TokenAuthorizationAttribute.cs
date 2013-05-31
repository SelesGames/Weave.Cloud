using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Weave.Mobilizer.Core.Web
{
    public class TokenAuthorizationAttribute : AuthorizationFilterAttribute
    {
        string tokenKey = "token";
        string hardCodedTokenVal = "hxyuiplkx78!ksdfl";

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var url = actionContext.Request.RequestUri.OriginalString;
            var queryString = string.Join(string.Empty, url.Split('?').Skip(1));
            var queryNameValues = HttpUtility.ParseQueryString(queryString);
            var tokenValue = queryNameValues[tokenKey];
            if (tokenValue == null)
            {
                actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                {
                    ReasonPhrase = "You must provide a 'token' value as an Http query parameter."
                };
            }
            else
            {
                if (tokenValue.Equals(hardCodedTokenVal))
                {
                    base.OnAuthorization(actionContext);
                }
                else
                {
                    actionContext.Response = new HttpResponseMessage(HttpStatusCode.Forbidden)
                    {
                        ReasonPhrase = "Invalid token."
                    };
                }
            }

            DebugEx.WriteLine(actionContext);
        }

        //object GetParametersFromUrl(string url)
        //{
        //    var queryString = string.Join(string.Empty, url.Split('?').Skip(1));
        //    var vals = HttpUtility.ParseQueryString(queryString);
        //    var parameters = vals.AllKeys.SelectMany(ak => vals[ak].Split(',').Select(x => new { key = ak, val = x })).ToLookup(ak => ak.key, ak => ak.val).Dump();
        //    foreach (var p in parameters)
        //    {
        //        p.AsEnumerable().Dump();
        //    }
        //}
    }

    //public class RequireHttpsAttribute : AuthorizationFilterAttribute
    //{
    //    public override void OnAuthorization(HttpActionContext actionContext)
    //    {
    //        if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps)
    //        {
    //            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Forbidden)
    //            {
    //                ReasonPhrase = "HTTPS Required"
    //            };
    //        }
    //        else
    //        {
    //            base.OnAuthorization(actionContext);
    //        }
    //    }
    //}
}
