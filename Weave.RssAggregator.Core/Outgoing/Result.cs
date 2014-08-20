using System.Runtime.Serialization;
using Weave.RssAggregator.Core.DTOs.Incoming;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    [DataContract]
    public class Result : Request
    {
        [DataMember(Order=5)]   public bool IsLoaded { get; set; }
        [DataMember(Order=6)]   public object Meta { get; set; }

        public override string ToString()
        {
            return IsLoaded.ToString();
        }
    }
}