using System.Runtime.Serialization;

namespace Weave.RssAggregator.Core.DTOs.Incoming
{
    [DataContract]
    public class Request
    {
        [DataMember(Order=1)]   public string Url { get; set; }
        [DataMember(Order=2)]   public string MostRecentNewsItemPubDate { get; set; }  // optional to get all
        [DataMember(Order=3)]   public string Etag { get; set; }  //optional
        [DataMember(Order=4)]   public string LastModified { get; set; }  //optional

        public override string ToString()
        {
            return string.Format("Url: {0}  MostRecent:{1}  Etag:{2}  LastModified:{3}",
                Url,
                MostRecentNewsItemPubDate ?? "{null}",
                Etag ?? "{null}",
                LastModified ?? "{null}");
        }
    }
}