using System;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Web;

namespace Weave.Mobilizer.Core
{
    [ServiceContract(SessionMode = SessionMode.NotAllowed)]
    public interface IInstapaperMobilizerFormatter
    {
        [OperationContract(AsyncPattern = true)]
        [WebGet(UriTemplate = "/IPF?url={url}")]
        IAsyncResult BeginGetString(string url, AsyncCallback callback, object state);
        Stream EndGetString(IAsyncResult result);
    }
}
