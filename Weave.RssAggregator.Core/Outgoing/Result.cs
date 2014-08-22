using System.Runtime.Serialization;

namespace Weave.RssAggregator.Core.DTOs.Outgoing
{
    [DataContract]
    public class Result
    {
        [DataMember(Order=1)]   public string Url { get; set; }
        [DataMember(Order=2)]   public bool IsLoaded { get; set; }
        [DataMember(Order=3)]   public object Meta { get; set; }

        public override string ToString()
        {
            return IsLoaded.ToString();
        }
    }
}