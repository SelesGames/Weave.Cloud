using System.Runtime.Serialization;

namespace Weave.RssAggregator.Core.DTOs.Incoming
{
    [DataContract]
    public class Request
    {
        [DataMember(Order=1)]   public string Url { get; set; }

        public override string ToString()
        {
            return string.Format("Url: {0}", Url);
        }
    }
}