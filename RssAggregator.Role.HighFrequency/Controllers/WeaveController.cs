using SelesGames.WebApi;
using System.Net;
using System.ServiceModel;
using System.Web.Http;
using Weave.RssAggregator.HighFrequency;

namespace Weave.Mobilizer.Core.Controllers
{
    [ServiceBehavior(
        ConcurrencyMode = ConcurrencyMode.Multiple, 
        InstanceContextMode = InstanceContextMode.Single
    )]
    public class WeaveController : ApiController, IWeaveControllerService
    {
        readonly HighFrequencyFeedCache cache;

        public WeaveController(HighFrequencyFeedCache cache)
        {
            this.cache = cache;
        }

        public HighFrequencyFeed Get(string url)
        {
            if (cache.Contains(url))
            {
                var result = cache.Get(url);
                return result;
            }

            else
                throw ResponseHelper.CreateResponseException(
                    HttpStatusCode.NotFound,
                    string.Format("{0} was not a high frequency feed and not found in cache", url));
        }
    }

    [ServiceContract]
    public interface IWeaveControllerService
    {
        [OperationContract]
        HighFrequencyFeed Get(string url);
    }
}
