using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Weave.RssAggregator.Core
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IRssAggregator
    {
        [OperationContract(AsyncPattern = true)]
        [WebInvoke(UriTemplate = "/Weave?schema={schema}&fsd={forceSuppressDescription}", Method = "POST")]
        IAsyncResult BeginGetWeave(Stream feedRequests, string schema, string forceSuppressDescription, AsyncCallback callback, object state);
        Stream EndGetWeave(IAsyncResult result);
    }
}
