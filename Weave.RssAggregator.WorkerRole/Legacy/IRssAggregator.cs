using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Weave.RssAggregator.WorkerRole.Legacy
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IRssAggregator
    {
        //[OperationContract(AsyncPattern = true)]
        //[WebInvoke(UriTemplate = "/Weave?fsd={forceSuppressDescription}", Method = "POST")]
        //IAsyncResult BeginGetWeave(Stream feedRequests, string forceSuppressDescription, AsyncCallback callback, object state);
        //Stream EndGetWeave(IAsyncResult result);

        [OperationContract(AsyncPattern = true)]
        [WebInvoke(UriTemplate = "/Weave?fsd={forceSuppressDescription}", Method = "POST")]
        System.Threading.Tasks.Task<Stream> Get(Stream feedRequests, string forceSuppressDescription);
    }
}
