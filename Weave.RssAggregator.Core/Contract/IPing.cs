using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Weave.RssAggregator.Core
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IPing
    {
        [OperationContract]
        [WebGet(UriTemplate = "")]
        Stream Ping();
    }
}
